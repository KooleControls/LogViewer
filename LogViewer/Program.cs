using FormsLib;
using Microsoft.Extensions.DependencyInjection;

namespace LogViewer
{
    internal static class Program
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            // 1. Create a service collection
            var services = new ServiceCollection();

            // 2. Configure the services
            ConfigureServices(services);

            // 3. Build the service provider
            ServiceProvider = services.BuildServiceProvider();

            // 4. Run the application with the main form
            Application.Run(ServiceProvider.GetRequiredService<Form1>());

        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Add your forms to the DI container
            services.AddTransient<Form1>();
            services.AddMemoryCache();
            services.AddHybridCache();

        }
    }
}
