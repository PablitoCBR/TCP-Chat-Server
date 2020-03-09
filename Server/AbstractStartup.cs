using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Server
{
    public abstract class AbstractStartup
    {
        public IConfiguration Configuration { get; }

        protected AbstractStartup()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            Configuration = configurationBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();
        }

        public abstract IServiceProvider ConfigureServices(IServiceCollection services);
    }
}
