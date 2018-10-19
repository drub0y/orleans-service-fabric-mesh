using System;
using System.Net;
using System.Threading.Tasks;
using Foo.Implementation;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace SiloHost
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var silo = new SiloHostBuilder()
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "sfmesh-cluster";
                        options.ServiceId = "sfmesh-service";
                    })
                    .UseAzureStorageClustering(options =>
                    {
                        options.ConnectionString = Environment.GetEnvironmentVariable("ORLEANS_AZURE_STORAGE_CLUSTER_CONNECTION_STRING");
                        options.TableName = Environment.GetEnvironmentVariable("ORLEANS_AZURE_STORAGE_CLUSTER_TABLE_NAME") ?? "OrleansClusterMembership";
                    })
                    .ConfigureEndpoints(
                        int.Parse(Environment.GetEnvironmentVariable("ORLEANS_SILO_PORT") ?? "11111"),
                        int.Parse(Environment.GetEnvironmentVariable("ORLEANS_GATEWAY_PORT") ?? "30000"))
                    .ConfigureApplicationParts(apm =>
                    {
                        apm.AddApplicationPart(typeof(FooGrain).Assembly).WithReferences();
                    })
                    .ConfigureLogging(loggingBuilder =>
                    {
                        loggingBuilder
                            .SetMinimumLevel(LogLevel.Information)
                            .AddConsole();
                    })
                    .AddAzureTableGrainStorage(
                        "Foos", 
                        options =>
                        {
                            options.ConnectionString = Environment.GetEnvironmentVariable("ORLEANS_AZURE_STORAGE_CLUSTER_CONNECTION_STRING");
                            options.UseJson = true;
                            options.IndentJson = true;
                            options.TableName = "Foos";
                        })
                    .Build();

            var loggerFactory = silo.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;

            var logger = loggerFactory.CreateLogger<Program>();

            logger.LogInformation("Starting SiloHost...");

            await silo.StartAsync();

            logger.LogInformation("SiloHost started...");

            var tcs = new TaskCompletionSource<object>();

            Console.CancelKeyPress += (s, a) => tcs.SetCanceled();

            logger.LogInformation("Waiting for cancellation key press...");

            await tcs.Task;

            logger.LogInformation("Cancellation received, shutting down.");
        }
    }
}
