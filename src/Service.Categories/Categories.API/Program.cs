using System.Reflection;
using Categories.API.Extensions;
using Categories.Contracts.Interfaces;
using Lib.MessageBroker.Contracts;
using Lib.Service;

var assembly = Assembly.GetExecutingAssembly();
_ = bool.TryParse(Environment.GetEnvironmentVariable("IsLocalDevelopment"), out var isLocalDevelopment);

await ServiceBuilder.CreateNewService(assembly.GetName().Name!, isLocalDevelopment)
    .AddMigrations(assembly)
    .AddConsumers(assembly)
    .AddServices((services) => services.AddServices())
    .AddGrpcServices(typeof(ICategoriesApi).Assembly)
    .AddOptions<ConsumingSettings>()
    .ConfigureWebApp(app =>
    {
        app.UseAuthentication();
        app.UseAuthorization();
    })
    .StartAsync();