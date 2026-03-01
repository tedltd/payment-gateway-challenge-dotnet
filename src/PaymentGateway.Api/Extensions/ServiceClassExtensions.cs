using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Services;
using PaymentGateway.Infrastructure.Clients;
using PaymentGateway.Infrastructure.Options;
using PaymentGateway.Infrastructure.Repositories;

using Polly;


namespace PaymentGateway.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PaymentGatewayOptions>(configuration.GetSection(PaymentGatewayOptions.SectionName));
            services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));
            return services;
        }
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IPaymentGatewayClient, PaymentGatewayClient>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<ICurrencyRepository, CurrencyRepository>();
            return services;
        }
        public static IServiceCollection AddTelemetry(this IServiceCollection services)
        {
            // removed for now, just included to show how we should add telemetry
            //services.AddOpenTelemetry()
            //    .WithMetrics(metrics =>
            //    {
            //        metrics
            //            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("PaymentApi"))
            //            .AddAspNetCoreInstrumentation()
            //            .AddMeter(PaymentMetrics.MeterName);
                    
            //    });
            services.AddSingleton<PaymentMetricsService>();
            return services;
        }
        public static IServiceCollection AddHttpClient(this IServiceCollection services, IConfiguration configuration)
        {
            var paymentGatewayOptions = configuration
                .GetSection(PaymentGatewayOptions.SectionName)
                .Get<PaymentGatewayOptions>() ?? new PaymentGatewayOptions();

            services.AddHttpClient("PaymentGateway", client =>
            {
                var baseUrl = configuration["PaymentGateway:BaseUrl"] ?? "http://localhost:8080";
                client.BaseAddress = new Uri(paymentGatewayOptions.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "PaymentApi/1.0");

                if (!string.IsNullOrEmpty(paymentGatewayOptions.ApiKey))
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", paymentGatewayOptions.ApiKey);
                }
            })
            .AddStandardResilienceHandler(options =>
            {
               
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(1);
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.Retry.UseJitter = true;

               
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                options.CircuitBreaker.FailureRatio = 0.5;
                options.CircuitBreaker.MinimumThroughput = 10;
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);

                
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
            });

            return services;
        }
    }
}
