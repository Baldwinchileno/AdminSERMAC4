using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AdminSERMAC.Core.Interfaces;
using AdminSERMAC.Core.Infrastructure;
using AdminSERMAC.Services;
using AdminSERMAC.Core.Configuration;

namespace AdminSERMAC.Core.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            // Logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Unit of Work y Repositorios
            services.AddScoped<IUnitOfWork>(provider =>
                new UnitOfWork(connectionString, provider.GetRequiredService<ILogger<UnitOfWork>>()));

            // Servicios
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<SQLiteService>();

            // Servicios adicionales
            services.AddSingleton<ConfigurationService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<FileDataManager>();

            return services;
        }
    }
}