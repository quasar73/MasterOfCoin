using System.Reflection;
using Lib.Service;

var assembly = Assembly.GetExecutingAssembly();
_ = bool.TryParse(Environment.GetEnvironmentVariable("IsLocalDevelopment"), out var isLocalDevelopment);

await ServiceBuilder.CreateNewService(assembly.GetName().Name!, isLocalDevelopment)
    .AddMigrations(assembly)
    // .AddServices((services, configuration) => services.AddServices(configuration).AddValidators(assembly))
    .ConfigureWebApp(x => x.Lifetime.ApplicationStarted.Register(() =>
    {
    }))
    .StartAsync();