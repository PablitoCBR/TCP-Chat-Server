using System;

using Microsoft.Extensions.DependencyInjection;

using Serilog;

using Host.Abstractions;
using Host.Builder;
using Host.Builder.Interfaces;
using Host.Builder.Models;
using Host.Listeners;
using Host.Listeners.Interfaces;

namespace Server
{
    public class Startup : AbstractStartup
    {
        public override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration).CreateLogger();
            services.AddLogging(cfg => cfg.AddSerilog());

            ConfigureOptions(services);

            services.AddSingleton<IHostBuilder, HostBuilder>();

            services.AddTransient<IListenerFabric, ListenerFabric>();
            
            return services.BuildServiceProvider();
        }

        private void ConfigureOptions(IServiceCollection services)
        {
            services.AddOptions<HostBuilderSettings>().Bind(Configuration.GetSection(nameof(HostBuilderSettings))).ValidateDataAnnotations();
        }
    }
}
    