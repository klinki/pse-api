module PseApi.IntegrationTests
open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.TestHost
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open PseApi
open PseApi.Data
open Xunit
open System.Net.Http

// module Fixtures =
// type WebApplicationFixture() =
//     do
//     
//         sprintf "" |> ignore
//     
//     interface IDisposable with
//         
//         member __.Dispose () =
//             ()

let configureApp (app : IApplicationBuilder) = 
    let env = app.ApplicationServices.GetService<IHostEnvironment>()
    match env.IsDevelopment() with
     | _ -> app.UseDeveloperExceptionPage()
     // | false -> app.UseExceptionHandler()
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

let createHost() =
    WebHostBuilder()
        .UseContentRoot(Directory.GetCurrentDirectory()) 
        .UseEnvironment("Test")
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(Action<IServiceCollection> configureServices)

[<Fact>]
let ``GET api/about/version`` () =
    let testData = async {
        use server = new TestServer(createHost())
        use client = server.CreateClient()
        
        let! data = (client.GetAsync "api/about/version" |> Async.AwaitTask)
        return Assert.True data.IsSuccessStatusCode
    }
    testData |> Async.StartAsTask

let createTestServer =
    let server = new CustomWebApplicationFactory<Startup>()
    server

[<Fact>]
let ``GET api/about/version Native`` () = async {
        use server = createTestServer
        use client = server.CreateClient()
        
        try
            let! data = (client.GetAsync "api/about/version" |> Async.AwaitTask)
            let code = data.StatusCode
            printfn "Code: %s" (string code)
            return Assert.True data.IsSuccessStatusCode
        with
            | ex -> printfn "Exception! %s " (ex.Message)
    }

