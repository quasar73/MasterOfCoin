using System.Reflection;
using Lib.Service;
using MasterOfCoin.API.Extensions;
using MasterOfCoin.API.Options;

var assembly = Assembly.GetExecutingAssembly();
_ = bool.TryParse(Environment.GetEnvironmentVariable("IsLocalDevelopment"), out var isLocalDevelopment);

await ServiceBuilder.CreateNewService(assembly.GetName().Name!, isLocalDevelopment)
    .AddMigrations(assembly)
    .AddOptions<AuthenticationOptions>()
    .AddServices((services, configuration) => services.AddServices(configuration))
    .ConfigureWebApp(app =>
    {
        app.UseAuthentication();
        app.UseAuthorization();
    })
    .StartAsync();