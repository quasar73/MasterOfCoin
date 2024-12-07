using System.Reflection;
using Lib.MessageBroker.Contracts;
using Lib.Service;
using Wallets.API.Extensions;
using Wallets.Contracts.Interfaces;

var assembly = Assembly.GetExecutingAssembly();
_ = bool.TryParse(Environment.GetEnvironmentVariable("IsLocalDevelopment"), out var isLocalDevelopment);

await ServiceBuilder.CreateNewService(assembly.GetName().Name!, isLocalDevelopment)
    .AddMigrations(assembly)
    .AddConsumers(assembly)
    .AddServices((services) => services.AddServices())
    .AddGrpcServices(typeof(IWalletsApi).Assembly)
    .AddOptions<ConsumingSettings>()
    .ConfigureWebApp(app =>
    {
        app.UseAuthentication();
        app.UseAuthorization();
    })
    .StartAsync();