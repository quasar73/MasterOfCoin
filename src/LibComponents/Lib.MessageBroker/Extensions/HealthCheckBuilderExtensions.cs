using Lib.MessageBroker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Lib.MessageBroker.Extensions;

public static class HealthChecksBuilderExtensions
{
    public const string Tag = "MessageBrokerHealthCheck";

    public static IHealthChecksBuilder AddMessageBrokerHealthCheck(this IHealthChecksBuilder builder)
    {
        builder.Services.TryAddSingleton<MessageBrokerHealthCheck>();

        return builder.Add(new HealthCheckRegistration(
            name: Tag,
            factory: sp => sp.GetRequiredService<MessageBrokerHealthCheck>(),
            failureStatus: default,
            tags: [Tag]));
    }
}
