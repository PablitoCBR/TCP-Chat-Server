using Host.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Server
{
    public static class Server
    {
        public static IHostBuilder CreateDeafaultBuilder<TStartup>(string[] args) where TStartup : AbstractStartup, new()
        {
            TStartup startup = Activator.CreateInstance<TStartup>();
            IServiceProvider serviceProvider = startup.ConfigureServices(new ServiceCollection());
            IHostBuilder hostBuilder = serviceProvider.GetService<IHostBuilder>();
            return hostBuilder;
        }
    }
}