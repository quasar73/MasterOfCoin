using System.Reflection;
using Lib.MessageBroker.Contracts;
using Lib.Service;
using Transactions.API.Extensions;
using Transactions.Contracts.Interfaces;

var assembly = Assembly.GetExecutingAssembly();
_ = bool.TryParse(Environment.GetEnvironmentVariable("IsLocalDevelopment"), out var isLocalDevelopment);

await ServiceBuilder.CreateNewService(assembly.GetName().Name!, isLocalDevelopment)
    .AddMigrations(assembly)
    .AddConsumers(assembly)
    .AddServices((services) => services.AddServices())
    .AddGrpcServices(typeof(ITransactionsApi).Assembly)
    .AddOptions<ConsumingSettings>()
    .ConfigureWebApp(app =>
    {
        app.UseAuthentication();
        app.UseAuthorization();
    })
    .StartAsync();