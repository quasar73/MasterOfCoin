using System.Reflection;
using Lib.Service;
using Microsoft.Extensions.Options;

var assembly = Assembly.GetExecutingAssembly();
_ = bool.TryParse(Environment.GetEnvironmentVariable("IsLocalDevelopment"), out var isLocalDevelopment);

await ServiceBuilder.CreateNewService(assembly.GetName().Name!, isLocalDevelopment)
    .AddMigrations(assembly)
    // .AddServices((services, configuration) => services.AddServices(configuration).AddValidators(assembly))
    .ConfigureWebApp(x => x.Lifetime.ApplicationStarted.Register(() =>
    {
        // var options = x.Services.GetRequiredService<IOptions<MigrationJobOptions>>();

        // if (!options.Value.Enabled)
        // {
        //     return;
        // }

        // var scheduler = x.Services.GetRequiredService<IScheduler>();
        // scheduler.ScheduleOrUpdate<IMigrationService>(nameof(IMigrationService.Migrate), service => service.Migrate(x.Lifetime.ApplicationStopping), options.Value.Cron);
    }))
    .StartAsync();