using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PaymentService.Services;

public interface IRabbitMQService
{
    void PublishMessage<T>(string queueName, T message);
    void ConsumeMessages(string queueName, Action<string> messageHandler);
}

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQService(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:Username"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest"
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to RabbitMQ: {ex.Message}");
            throw;
        }
    }

    public void PublishMessage<T>(string queueName, T message)
    {
        try
        {
            _channel.QueueDeclare(queue: queueName,
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(exchange: "",
                                routingKey: queueName,
                                basicProperties: null,
                                body: body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to publish message: {ex.Message}");
            throw;
        }
    }

    public void ConsumeMessages(string queueName, Action<string> messageHandler)
    {
        try
        {
            _channel.QueueDeclare(queue: queueName,
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    messageHandler(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to process message: {ex.Message}");
                }
            };

            _channel.BasicConsume(queue: queueName,
                                autoAck: true,
                                consumer: consumer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to set up consumer: {ex.Message}");
            throw;
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to dispose RabbitMQ resources: {ex.Message}");
        }
    }
} 