using ApplicationRegister.Handler.Interfaces;
using ApplicationRegister.Handler.Repositories;
using ApplicationRegister.Handler.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace ApplicationRegister.Handler
{
    class Program
    {
        public static IConfigurationRoot configuration;

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<IWorker, Worker>()
                    .AddSingleton<IApplicationRepository, ApplicationRepository>();

            configuration = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                            .AddJsonFile("appsettings.json", false)
                            .Build();

            services.AddSingleton<IConfigurationRoot>(configuration);

            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddFile();
            });
        }

        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IWorker worker = serviceProvider.GetService<IWorker>();
            try
            {
                worker.Run();
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
            catch (System.Exception exception)
            {
                Console.Error.WriteLine(exception.Message);
            }
            finally
            {
                worker.Stop();
                serviceProvider.Dispose();
            }
        }
    }
}
