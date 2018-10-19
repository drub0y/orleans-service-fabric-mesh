using System;
using System.Threading.Tasks;
using Foo.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace WebClient
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var orleansClient = new ClientBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "sfmesh-cluster";
                    options.ServiceId = "sfmesh-service";
                })
                .UseAzureStorageClustering(options =>
                {
                    options.ConnectionString = Environment.GetEnvironmentVariable("ORLEANS_AZURE_STORAGE_CLUSTER_CONNECTION_STRING");
                    options.TableName = Environment.GetEnvironmentVariable("ORLEANS_AZURE_STORAGE_CLUSTER_TABLE_NAME");
                })
                .ConfigureApplicationParts(apm =>
                {
                    apm.AddApplicationPart(typeof(IFoo).Assembly);
                })
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder
                        .SetMinimumLevel(LogLevel.Information)
                        .AddEventSourceLogger();
                })
                .Build();

            orleansClient.Connect(async ex =>
            {
                await Task.Delay(500);

                return true;
            }).Wait();

            services.AddSingleton<IClusterClient>(orleansClient);
        }

        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                var clusterClient = context.RequestServices.GetService<IClusterClient>();

                var nullFoo = clusterClient.GetGrain<IFoo>(Guid.Empty);
                var result = await nullFoo.JustFooIt();

                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync($"Hello World! {result}");
            });
        }
    }
}
