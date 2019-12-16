using System;

using Microsoft.Extensions.DependencyInjection;

using Serilog;

using Host.Builder;
using Host.Builder.Interfaces;
using Host.Builder.Models;
using Host.Listeners;
using Host.Listeners.Interfaces;

using AutoMapper;

using Core.Models;
using Core.Services.Encoders.Interfaces;
using Core.Services.Encoders;
using Core.Services.Security;
using Core.Services.Security.Interfaces;
using DAL.Repositories.Interfaces;
using DAL.Repositories;
using DAL;
using Core.Pipeline.Interfaces;
using Core.Pipeline;
using Core.Services.Factories.Interfaces;
using Core.Services.Factories;
using Core.Security.Interfaces;
using Core.Security;

namespace Server
{
    public class Startup : AbstractStartup
    {
        public override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration).CreateLogger();
            services.AddLogging(cfg => cfg.AddSerilog());
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            ConfigureOptions(services);

            services.AddSingleton<IHostBuilder, HostBuilder>();

            services.AddTransient<IListenerFabric, ListenerFabric>();

            services.AddTransient<IFrameMetaEncoder, FrameMetaEncoder>();
            services.AddTransient<IHeadersEncoder, HeadersEncoder>();
            services.AddTransient<IMessageEncoder, MessageEncoder>();

            services.AddTransient<ISecurityService, SecurityService>();
            services.AddTransient<IMessageFactory, MessageFactory>();

            services.AddTransient<IUserRepository, UserRepository>();

            services.AddDbContext<ChattyDbContext>();

            this.ConfigureMessageHandlers(services);
            return services.BuildServiceProvider();
        }

        private void ConfigureOptions(IServiceCollection services)
        {
            services.AddOptions<SecuritySettings>().Bind(Configuration.GetSection(nameof(SecuritySettings))).ValidateDataAnnotations();
            services.AddOptions<HostBuilderSettings>().Bind(Configuration.GetSection(nameof(HostBuilderSettings))).ValidateDataAnnotations();
            services.AddOptions<FrameMetaDataConfiguration>().Bind(Configuration.GetSection(nameof(FrameMetaDataConfiguration)))
                .ValidateDataAnnotations()
                .PostConfigure(config => config.MetaDataLength = config.SenderIdLength + config.HeadersDataLength + config.MessageDataLength + 1);
        }

        private void ConfigureMessageHandlers(IServiceCollection services)
        {
            services.AddTransient<IMessageDispatcher, MessageDispatcher>();
            services.AddTransient<IAuthenticationHandler, AuthenticationHandler>();
        }
    }
}
    