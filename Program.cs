using Microsoft.Extensions.DependencyInjection;
using AdminSERMAC.Core.Configuration;
using AdminSERMAC.Services;
using AdminSERMAC.Repositories;
using AdminSERMAC.Core.Theme;
using AdminSERMAC.Core.Interfaces;

namespace AdminSERMAC;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Inicializar el tema
        ThemeManager.LoadThemePreference();

        // Configurar servicios
        var services = new ServiceCollection();
        var connectionString = "Data Source=AdminSERMAC.db;Version=3;";

        // Registrar dependencias manualmente
        RegisterServices(services, connectionString);

        // Crear el proveedor de servicios
        var serviceProvider = services.BuildServiceProvider();

        // Obtener el servicio de cliente
        var clienteService = serviceProvider.GetRequiredService<IClienteService>();

        // Iniciar la aplicación con la ventana principal
        Application.Run(new MainForm(clienteService, connectionString));
    }

    private static void RegisterServices(IServiceCollection services, string connectionString)
    {
        // Registro de repositorios
        services.AddScoped<IClienteRepository>(provider =>
        {
            var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ClienteRepository>>();
            return new ClienteRepository(connectionString, logger);
        });

        // Registro de servicios
        services.AddScoped<IClienteService, ClienteService>();

        // Configurar logging
        services.AddLogging(configure => configure.AddConsole());
    }
}
