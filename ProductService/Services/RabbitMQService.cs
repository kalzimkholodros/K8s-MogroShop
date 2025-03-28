using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ProductService.Services;

public interface IRabbitMQService
{
    void PublishMessage<T>(string exchange, string routingKey, T message);
    void Subscribe<T>(string queue, Action<T> onMessage);
}

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private readonly IConnection _connection;
    private readonly RabbitMQ.Client.IModel _channel;

    public RabbitMQService(IConfiguration configuration)
    {
        var factory = new ConnectionFactory() 
        { 
            HostName = configuration["RabbitMQ:Host"] ?? "rabbitmq",
            Port = configuration["RabbitMQ:Port"] != null ? int.Parse(configuration["RabbitMQ:Port"]) : 5672,
            UserName = configuration["RabbitMQ:Username"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest"
        };
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void PublishMessage<T>(string exchange, string routingKey, T message)
    {
        _channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(
            exchange: exchange,
            routingKey: routingKey,
            basicProperties: null,
            body: body);
    }

    public void Subscribe<T>(string queue, Action<T> onMessage)
    {
        _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var deserializedMessage = JsonSerializer.Deserialize<T>(message);

            if (deserializedMessage != null)
            {
                onMessage(deserializedMessage);
            }
        };

        _channel.BasicConsume(queue: queue,
                            autoAck: true,
                            consumer: consumer);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
} 