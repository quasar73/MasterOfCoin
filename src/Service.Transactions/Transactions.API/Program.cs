using System.Reflection;
using Lib.Service;
using Transactions.API.Extensions;
using Transactions.Contracts.Interfaces;

var assembly = Assembly.GetExecutingAssembly();
_ = bool.TryParse(Environment.GetEnvironmentVariable("IsLocalDevelopment"), out var isLocalDevelopment);

await ServiceBuilder.CreateNewService(assembly.GetName().Name!, isLocalDevelopment)
    .AddMigrations(assembly)
    .AddServices((services) => services.AddServices())
    .AddGrpcServices(typeof(ITransactionsApi).Assembly)
    .ConfigureWebApp(app =>
    {
        app.UseAuthentication();
        app.UseAuthorization();
    })
    .StartAsync();