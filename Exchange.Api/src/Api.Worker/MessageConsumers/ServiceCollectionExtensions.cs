﻿using Microsoft.Extensions.DependencyInjection;

namespace Api.Worker.MessageConsumers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageConsumers(this IServiceCollection services)
        {
            // TODO: Just an example
            services.AddTransient<ExecuteSomethingConsumer>();

            return services;
        }
    }
}
