﻿using System;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Api.Common.Configuration;
using Api.Common.Domain.AppFeatureExample;
using Api.Common.HostedServices;
using Api.Common.Persistence;
using Api.Worker.MessageConsumers;
using Swisschain.Sdk.Server.Common;

namespace Api.Worker
{
    public sealed class Startup : SwisschainStartup<AppConfig>
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void ConfigureServicesExt(IServiceCollection services)
        {
            base.ConfigureServicesExt(services);

            services.AddHttpClient();
            services.AddPersistence(Config.Db.ConnectionString);
            services.AddAppFeatureExample();
            services.AddMessageConsumers();

            services.AddMassTransit(x =>
            {
                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(Config.RabbitMq.HostUrl, host =>
                    {
                        host.Username(Config.RabbitMq.Username);
                        host.Password(Config.RabbitMq.Password);
                    });

                    cfg.UseMessageRetry(y =>
                        y.Exponential(5,
                            TimeSpan.FromMilliseconds(100),
                            TimeSpan.FromMilliseconds(10_000),
                            TimeSpan.FromMilliseconds(100)));

                    cfg.SetLoggerFactory(provider.GetRequiredService<ILoggerFactory>());

                    // TODO: Define your receive endpoints. It's just an example:
                    cfg.ReceiveEndpoint("api-exchange-something-execution", e =>
                    {
                        e.Consumer(provider.GetRequiredService<ExecuteSomethingConsumer>);
                    });
                }));

                services.AddHostedService<BusHost>();
            });
        }
    }
}
