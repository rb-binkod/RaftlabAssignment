using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using RaftlabAssignment.Infrastructure.Configuration;
using RaftlabAssignment.Infrastructure.Interfaces;
using RaftlabAssignment.Infrastructure.Services;
using System;
using System.IO;
using System.Net.Http;

namespace RaftlabApiAssignment.Api
{
    public static class Startup
    {
        public static IServiceProvider ConfigureServices()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection();

            services.AddLogging(config =>
            {
                config.AddConsole();
            });

            services.AddMemoryCache();
            services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));
            services.AddSingleton(resolver =>
                resolver.GetRequiredService<IOptions<ApiSettings>>().Value);

            services.AddHttpClient<IExternalUserService, ExternalUserService>()
                .AddPolicyHandler(GetRetryPolicy());

            return services.BuildServiceProvider();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
