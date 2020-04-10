﻿using Core.Handlers.ExceptionHandlers;
using Core.Handlers.ExceptionHandlers.Interfaces;
using Core.Handlers.MessageHandlers;
using Core.Handlers.MessageHandlers.Interfaces;
using Core.Handlers.Security;
using Core.Handlers.Security.Interfaces;
using Core.Models;
using Core.Models.Exceptions;
using Core.Models.Exceptions.ServerExceptions;
using Core.Pipeline;
using Core.Services.Encoders;
using Core.Services.Factories;
using Core.Services.Security;
using DAL;
using DAL.Repositories;
using DAL.Repositories.Interfaces;
using Host.Builder;
using Host.Listeners;
using Host.Listeners.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Net.Sockets;

namespace Server
{
    public class Startup : AbstractStartup
    {
        public override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom
                .Configuration(Configuration)
                .CreateLogger();

            services.AddLogging(cfg => cfg.AddSerilog());
            
            this.ConfigureOptions(services);
            this.ConfigureEncoders(services);
            this.ConfigureMessageHandlers(services);
            this.ConfigureExceptionHandlers(services);

            services.AddSingleton<IHostBuilder, HostBuilder>();

            services.AddTransient<IListenerFabric, ListenerFabric>();
            services.AddTransient<IMessageFactory, MessageFactory>();

            services.AddTransient<ISecurityService, SecurityService>();
            services.AddSingleton<IPrimeNumberGenerator, PrimeNumberGenerator>();

            services.AddDbContext<ChattyDbContext>();
            services.AddTransient<IUserRepository, UserRepository>();

            return services.BuildServiceProvider();
        }

        private void ConfigureOptions(IServiceCollection services)
        {
            services.AddOptions<SecuritySettings>().Bind(Configuration.GetSection(nameof(SecuritySettings))).ValidateDataAnnotations();
            services.AddOptions<ListenerSettings>().Bind(Configuration.GetSection(nameof(ListenerSettings))).ValidateDataAnnotations();
            services.AddOptions<FrameMetaDataConfiguration>().Bind(Configuration.GetSection(nameof(FrameMetaDataConfiguration)))
                .ValidateDataAnnotations()
                .PostConfigure(config => config.MetaDataFieldsTotalSize = 1 + config.HeadersLengthFieldSize + config.MessageLengthFieldSize);
        }

        private void ConfigureMessageHandlers(IServiceCollection services)
        {
            services.AddTransient<IMessageDispatcher, MessageDispatcher>();
        
            services.AddTransient<IAuthenticationHandler, AuthenticationHandler>();

            services.AddTransient<IMessageHandler, SendRequestMessageHandler>();
            services.AddTransient<IMessageHandler, DHKeyExchangeRequestMessageHandler>();
            services.AddTransient<IMessageHandler, DHKeyExchangeStepMessageHandler>();
            services.AddTransient<IMessageHandler, ActiveUsersUpdateRequestMessageHandler>();
        }

        private void ConfigureExceptionHandlers(IServiceCollection services)
        {
            services.AddTransient<IExceptionHandler<AbstractException>, DefaultExceptionHandler>();

            services.AddTransient<IExceptionHandler<ClientUnreachableException>, ClientUnreachableExceptionHandler>();
            services.AddTransient<IExceptionHandler<UnsupportedMessageTypeException>, UnsupportedMessageTypeExceptionHandler>();
            services.AddTransient<IExceptionHandler<SocketException>, SocketExceptionHandler>();
        }

        private void ConfigureEncoders(IServiceCollection services)
        {
            services.AddTransient<IFrameMetaEncoder, FrameMetaEncoder>();
            services.AddTransient<IHeadersEncoder, HeadersEncoder>();
            services.AddTransient<IMessageEncoder, MessageEncoder>();
        }
    }
}
    