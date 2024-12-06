using System.Reflection;
using Lib.Service;
using MasterOfCoin.API.Extensions;
using MasterOfCoin.API.Options;
using Transactions.Contracts.Interfaces;

var assembly = Assembly.GetExecutingAssembly();
_ = bool.TryParse(Environment.GetEnvironmentVariable("IsLocalDevelopment"), out var isLocalDevelopment);

await ServiceBuilder.CreateNewService(assembly.GetName().Name!, isLocalDevelopment)
    .AddMigrations(assembly)
    .AddOptions<AuthenticationOptions>()
    // .AddGrpcClients(typeof(ITransactionsApi).Assembly)
    .AddServices((services, configuration) => services.AddServices(configuration, isLocalDevelopment))
    .ConfigureWebApp(app =>
    {
        app.UseAuthentication();
        app.UseAuthorization();
    })
    .StartAsync();