using FormsLib;
using LogViewer.Config.Helpers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LogViewer
{
    internal static class Program
    {
#if DEBUG
        private static readonly string _sqLiteCachePath = PathHelper.NormalizePath("%LOCALAPPDATA%\\LogViewer\\cache.sqlite");
#else
        private static readonly string _sqLiteCachePath = PathHelper.NormalizePath("%LOCALAPPDATA%\\LogViewer\\cache.sqlite");
#endif


        public static IServiceProvider ServiceProvider { get; private set; }

        [STAThread]
        static void Main()
        {
            // Initialize SQLitePCL provider
            SQLitePCL.Batteries.Init();

            ApplicationConfiguration.Initialize();
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
            Application.Run(ServiceProvider.GetRequiredService<Form1>());

        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<Form1>();
            services.AddMemoryCache();
            services.AddSingleton<IDistributedCache>(sp => new SqliteDistributedCache(_sqLiteCachePath));
            services.AddHybridCache();
        }

    }
}
