module PseApi.IntegrationTests.Tests
open System
open System.Collections.Generic
open System.Net
open System.Net.Http
open System.Linq
open System.Threading.Tasks
open Foq
open FsUnit
open Hellang.Middleware.ProblemDetails
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Infrastructure
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.Logging
open PseApi
open PseApi.Data
open PseApi.Services
open Xunit
open OpenAPITypeProvider

let configureApp (app : IApplicationBuilder) = 
    let env = app.ApplicationServices.GetService<IHostEnvironment>()
    match env.IsDevelopment() with
     | _ -> app.UseDeveloperExceptionPage()
     |> ignore

let configureServices (services : IServiceCollection) = 
    let env = services.BuildServiceProvider().GetService<IHostEnvironment>();

    // AddDbContext composes option callbacks (EF Core 8+), so drop the app's PostgreSQL configuration first
    services.RemoveAll<IDbContextOptionsConfiguration<PseContext>>() |> ignore

    services.AddDbContext<PseContext>
        (fun (options : DbContextOptionsBuilder) -> 
        match env.IsEnvironment("Test") with
        | true -> options.UseInMemoryDatabase("client_tests") |> ignore
        | false -> options.UseNpgsql("Host=localhost;Database=pse_tests") |> ignore)
    |> ignore
    services.AddCors() |> ignore

type CustomWebApplicationFactory<'TStartup when 'TStartup : not struct>() = 
    inherit Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<'TStartup>()
    override _this.ConfigureWebHost builder =
        builder.UseEnvironment("Test")
            // .Configure(configureApp)
            .ConfigureTestServices(Action<IServiceCollection> configureServices)
        |> ignore

let createTestServer =
    let server = new CustomWebApplicationFactory<Startup>()
    server

let server = createTestServer

[<Literal>]
let swaggerPath = """swagger.json"""; // server.CreateClient().BaseAddress.AbsolutePath + "";

type PseApiDefinition = OpenAPIV3Provider<"swagger.json">
let pseApi = new PseApiDefinition();

[<Fact>]
let ``GET api/about/version should return information about version`` () = async {
    use client = server.CreateClient()
    
    let! data = (client.GetAsync "api/about/version" |> Async.AwaitTask)
    let! response = data.Content.ReadAsStringAsync() |> Async.AwaitTask

    let responseJson = PseApiDefinition.Schemas.VersionDto.Parse(response);

    Assert.True data.IsSuccessStatusCode
    Assert.True responseJson.BuildDate.IsSome
    Assert.True responseJson.Version.IsSome
}

[<Fact>]
let ``POST api/about/version should return problem detail with status 405`` () = async {
    use client = server.CreateClient()
    
    let! data = client.PostAsync("api/about/version", new StringContent("")) |> Async.AwaitTask
    let! response = data.Content.ReadAsStringAsync() |> Async.AwaitTask
    
    let responseJson = PseApiDefinition.Schemas.ProblemDetails.Parse(response);
 
    Assert.Equal(Microsoft.AspNetCore.Http.StatusCodes.Status405MethodNotAllowed, int data.StatusCode)
    Assert.Equal(Microsoft.AspNetCore.Http.StatusCodes.Status405MethodNotAllowed, responseJson.Status.Value)
}

[<Fact>]
let ``GET /some/totally/invalid/url should return problem detail with error 404`` () = async {
    use client = server.CreateClient()
    
    let! data = client.GetAsync "some/totally/invalid/url" |> Async.AwaitTask
    let! response = data.Content.ReadAsStringAsync() |> Async.AwaitTask
    
    let responseJson = PseApiDefinition.Schemas.ProblemDetails.Parse(response);

    Assert.Equal(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, int data.StatusCode)
    Assert.Equal(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, responseJson.Status.Value)
}


(** Testing global exception logger *)
let loggerMock = Mock<ILogger<ProblemDetailsMiddleware>>()
                     .As<ILogger<ProblemDetailsMiddleware>>()
                     .Setup(fun x -> <@ x.IsEnabled(any()) @>).Returns(true)
                     .Create()
let thrownExc = NotImplementedException("Testing if this exception will be logged")
let tradeServiceMock =
    Mock<ITradeService>()
        .Setup(fun x -> <@ x.IsValidDate(any()) @>).Raises(thrownExc)
        .Setup(fun x -> <@ x.GetTradesForDay(any()) @>).Returns(Task.FromResult<IEnumerable<Trade>>(Array.Empty<Trade>()))
        .Create()

let configureTestServices (services: IServiceCollection) =
    services.AddScoped<ILogger<ProblemDetailsMiddleware>>(fun serviceProvider -> loggerMock)
        .AddScoped<ITradeService>(fun sp -> tradeServiceMock)
    |> ignore

type WebAppFactoryForTestingLogging<'TStartup when 'TStartup : not struct>() = 
    inherit Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<'TStartup>()
    override _this.ConfigureWebHost builder =
        builder.UseEnvironment("Test")
            .ConfigureTestServices(Action<IServiceCollection> configureServices)
            .ConfigureTestServices(Action<IServiceCollection> configureTestServices)
        |> ignore

[<Fact>]
let ``Test global exception handling is configured`` () = async { 
    use server = new WebAppFactoryForTestingLogging<Startup>()
    use client = server.CreateClient()
    
    let! data = client.GetAsync "/api/trades/day/2020-01-01" |> Async.AwaitTask
    let! response = data.Content.ReadAsStringAsync() |> Async.AwaitTask
    
    let responseJson = PseApiDefinition.Schemas.ProblemDetails.Parse(response)
    
    data.StatusCode |> should equal HttpStatusCode.InternalServerError
    Mock.Verify(<@ loggerMock.Log(any(), any(), any(), thrownExc, any()) @>, once)
}


(** Trade queries *)
let private seedTrades () =
    use scope = server.Services.CreateScope()
    let context = scope.ServiceProvider.GetRequiredService<PseContext>()
    if not (context.Trades.Any(fun t -> t.BIC = "BAATEST")) then
        // Inserted out of date order on purpose
        for (id, date) in [ (1003L, DateTime(2024, 1, 4)); (1001L, DateTime(2024, 1, 2)); (1002L, DateTime(2024, 1, 3)) ] do
            context.Trades.Add(Trade(Id = id, BIC = "BAATEST", ISIN = "CZ0000000001", Name = "TEST", Date = date)) |> ignore
        context.Stocks.Add(Stock(Id = 1L, BIC = "BAATEST", ISIN = "CZ0000000001", Name = "TEST")) |> ignore
        context.SaveChanges() |> ignore

let private getJson (url : string) = async {
    use client = server.CreateClient()
    let! data = client.GetAsync url |> Async.AwaitTask
    let! body = data.Content.ReadAsStringAsync() |> Async.AwaitTask
    return data.StatusCode, System.Text.Json.JsonDocument.Parse(body).RootElement
}

let private dates (json : System.Text.Json.JsonElement) =
    [ for trade in json.EnumerateArray() -> trade.GetProperty("date").GetDateTime() ]

[<Fact>]
let ``GET api/trades without a filter should return 400`` () = async {
    let! status, _ = getJson "api/trades"
    status |> should equal HttpStatusCode.BadRequest

    let! status, _ = getJson "api/trades?limit=5"
    status |> should equal HttpStatusCode.BadRequest
}

[<Fact>]
let ``GET api/trades with a limit below 1 should return 400`` () = async {
    let! status, _ = getJson "api/trades?bic=BAATEST&limit=0"
    status |> should equal HttpStatusCode.BadRequest
}

[<Fact>]
let ``GET api/trades should order by date before applying the limit`` () = async {
    seedTrades ()

    let! status, json = getJson "api/trades?bic=BAATEST&limit=2"
    status |> should equal HttpStatusCode.OK
    dates json |> should equal [ DateTime(2024, 1, 2); DateTime(2024, 1, 3) ]
}

[<Fact>]
let ``GET api/trades/{bic} and api/trades/isin/{isin} should return trades ordered by date`` () = async {
    seedTrades ()
    let expected = [ DateTime(2024, 1, 2); DateTime(2024, 1, 3); DateTime(2024, 1, 4) ]

    let! status, json = getJson "api/trades/BAATEST"
    status |> should equal HttpStatusCode.OK
    dates json |> should equal expected

    let! status, json = getJson "api/trades/isin/CZ0000000001"
    status |> should equal HttpStatusCode.OK
    dates json |> should equal expected

    let! status, json = getJson "api/trades/isin/CZ0000000001?limit=1"
    status |> should equal HttpStatusCode.OK
    dates json |> should equal [ DateTime(2024, 1, 2) ]
}

let private noDataServiceMock =
    Mock<ITradeService>()
        .Setup(fun x -> <@ x.IsValidDate(any()) @>).Returns(Task.FromResult true)
        .Setup(fun x -> <@ x.GetTradesForDay(any()) @>).Raises(TradingDataNotFoundException(DateTime(2026, 11, 17), null))
        .Create()

type WebAppFactoryWithoutTradingData<'TStartup when 'TStartup : not struct>() =
    inherit Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<'TStartup>()
    override _this.ConfigureWebHost builder =
        builder.UseEnvironment("Test")
            .ConfigureTestServices(Action<IServiceCollection> configureServices)
            .ConfigureTestServices(fun services -> services.AddScoped<ITradeService>(fun _ -> noDataServiceMock) |> ignore)
        |> ignore

[<Fact>]
let ``GET api/trades/day/{day} without data at the exchange should return 404`` () = async {
    use server = new WebAppFactoryWithoutTradingData<Startup>()
    use client = server.CreateClient()

    let! data = client.GetAsync "api/trades/day/2026-11-17" |> Async.AwaitTask
    let! response = data.Content.ReadAsStringAsync() |> Async.AwaitTask
    let responseJson = PseApiDefinition.Schemas.ProblemDetails.Parse(response)

    data.StatusCode |> should equal HttpStatusCode.NotFound
    responseJson.Status.Value |> should equal 404
}
