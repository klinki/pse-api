module PseApi.IntegrationTests
open System
open System.Net.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open PseApi
open PseApi.Data
open Xunit
open OpenAPITypeProvider

let configureApp (app : IApplicationBuilder) = 
    let env = app.ApplicationServices.GetService<IHostEnvironment>()
    match env.IsDevelopment() with
     | _ -> app.UseDeveloperExceptionPage()
     |> ignore

let configureServices (services : IServiceCollection) = 
    let env = services.BuildServiceProvider().GetService<IHostEnvironment>();

    services.AddDbContext<PseContext>
        (fun (options : DbContextOptionsBuilder) -> 
        match env.IsEnvironment("Test") with
        | true -> options.UseInMemoryDatabase("client_tests") |> ignore
        | false -> options.UseMySql(@"Data Source=(localdb)\v11.0;Initial Catalog=ContentDataDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False") |> ignore)
    |> ignore
    services.AddCors() |> ignore

type CustomWebApplicationFactory<'TStartup when 'TStartup : not struct>() = 
    inherit Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<'TStartup>()
    override _this.ConfigureWebHost builder =
        builder.UseEnvironment("Test")
            // .Configure(configureApp)
            .ConfigureServices(Action<IServiceCollection> configureServices)
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
