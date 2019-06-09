using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");

            // create service collection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // create service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // entry to run app
            serviceProvider.GetService<AppStart>().Run().Wait();

            Console.WriteLine("End");
            Console.ReadLine();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            var defaultsettingFile = "appsettings.json";

            // build system configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(defaultsettingFile, false)
                .Build();
            serviceCollection.Configure<AppSettings>(configuration.GetSection("Configuration"));

            //add services
            serviceCollection.AddTransient<IApiTest, ApiTest>();

            // add app
            serviceCollection.AddTransient<AppStart>();
        }
    }
}