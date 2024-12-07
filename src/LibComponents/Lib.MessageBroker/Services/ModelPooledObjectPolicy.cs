using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;

namespace Lib.MessageBroker.Services;

public class ModelPooledObjectPolicy(IConnection _connection) : IPooledObjectPolicy<IModel>
{
    public IModel Create()
    {
        return _connection.CreateModel();
    }

    public bool Return(IModel obj)
    {
        if (obj.IsOpen)
        {
            return true;
        }
        else
        {
            obj.Dispose();
            return false;
        }
    }
}