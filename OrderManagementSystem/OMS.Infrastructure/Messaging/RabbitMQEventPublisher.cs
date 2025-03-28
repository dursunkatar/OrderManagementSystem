using Microsoft.Extensions.Logging;
using OMS.Application.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OMS.Infrastructure.Messaging
{
    public class RabbitMQEventPublisher : IEventPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQEventPublisher> _logger;

        public RabbitMQEventPublisher(ConnectionFactory connectionFactory, ILogger<RabbitMQEventPublisher> logger)
        {
            _logger = logger;

            try
            {
                _connection = connectionFactory.CreateConnection();
                _channel = _connection.CreateModel();

                // Event exchange tanımla
                _channel.ExchangeDeclare(
                    exchange: "order_events",
                    type: "topic",
                    durable: true,
                    autoDelete: false);

                _logger.LogInformation("RabbitMQ bağlantısı başarıyla oluşturuldu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ bağlantısı oluşturulurken hata oluştu");
                throw;
            }
        }

        public async Task PublishAsync<T>(T @event) where T : class
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            try
            {
                string eventName = @event.GetType().Name;
                string message = JsonSerializer.Serialize(@event);
                byte[] body = Encoding.UTF8.GetBytes(message);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.MessageId = Guid.NewGuid().ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                properties.ContentType = "application/json";

                _channel.BasicPublish(
                    exchange: "order_events",
                    routingKey: eventName,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Event başarıyla RabbitMQ'ya gönderildi: {EventName}", eventName);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Event RabbitMQ'ya gönderilirken hata oluştu");
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();

                _logger.LogInformation("RabbitMQ bağlantısı kapatıldı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ bağlantısı kapatılırken hata oluştu");
            }
        }
    }
}
