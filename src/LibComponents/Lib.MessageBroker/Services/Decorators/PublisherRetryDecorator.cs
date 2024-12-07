using Lib.MessageBroker.Contracts;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Lib.MessageBroker.Services.Decorators;

public class PublisherRetryDecorator : IPublisher
{
    private readonly IPublisher _inner;
    private readonly ILogger<PublisherRetryDecorator> _logger;

    private readonly AsyncRetryPolicy _retryPolicy;
    private const int _retryAttempts = 3;

    public PublisherRetryDecorator(IPublisher inner, ILogger<PublisherRetryDecorator> logger)
    {
        _inner = inner;
        _logger = logger;

        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(retryCount: _retryAttempts, retryCount => TimeSpan.FromSeconds(Math.Pow(2, retryCount)),
                (exc, interval, retryAttempt, _) => { _logger.LogError("Operation failed with error: {message}. Retry attempt: {retryAttempt}", exc.Message, retryAttempt); });
    }

    public Task Publish<T>(T message, string? rountingKey = null) where T : class
        => _retryPolicy.ExecuteAsync(() => _inner.Publish(message, rountingKey));

    public Task FireAndForget<T>(T message, string? rountingKey = null) where T : class
        => _retryPolicy.ExecuteAsync(() => _inner.FireAndForget(message, rountingKey));
}
