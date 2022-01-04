using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PseApi.Configuration;
using PseApi.Data;
using Serilog;
using System;
using System.IO;
using System.Reflection;
using Hellang.Middleware.ProblemDetails;

namespace PseApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var connectionString = Configuration.GetConnectionString("Default");
            var serverVersion = ServerVersion.AutoDetect(connectionString);
            services.AddDbContext<PseContext>(options => options.UseMySql(connectionString, serverVersion));

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder => 
                {
                    builder.AllowAnyOrigin()
                        .WithMethods("GET");
                });
            });

            services.AddHealthChecks().AddMySql(connectionString);

            services.ConfigureDI(Configuration);
            services.AddQuartz();

            services.AddSwaggerGen(options => 
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "PSE API", Version = "v1" });
                options.DescribeAllParametersInCamelCase();

                var fileName = this.GetType().GetTypeInfo().Module.Name
                    .Replace(".dll", ".xml")
                    .Replace(".exe", ".xml");
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, fileName));

                // options.ExampleFilters();
            });
            // services.AddSwaggerExamplesFromAssemblyOf<StockExamples>();

            services.AddProblemDetails(options =>
            {
                options.ShouldLogUnhandledException = (context, exception, details) => true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseProblemDetails();
            app.UseSerilogRequestLogging();

            app.UseRouting();
            app.UseCors();

            if (Configuration.GetSection("HttpConfiguration")?.GetValue<bool>("UseHttpsRedirection") ?? false)
            {
                app.UseHttpsRedirection();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthz");
                endpoints.MapControllers();
            });

            app.UseScheduler(lifetime);

            app.UseSwagger();
            app.UseSwaggerUI(options => 
            {
                options.RoutePrefix = string.Empty;
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "PSE (BCPP) API v1");
            });
        }
    }
}
