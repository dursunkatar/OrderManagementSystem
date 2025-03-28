using Microsoft.EntityFrameworkCore.Metadata;
using OMS.Application.Interfaces;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OMS.Infrastructure.Messaging
{
    public class RabbitMQEventPublisher : IEventPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQEventPublisher(ConnectionFactory connectionFactory)
        {
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            // Event exchange tanımla
            _channel.ExchangeDeclare(
                exchange: "order_events",
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);
        }

        public async Task PublishAsync<T>(T @event) where T : class
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var eventName = @event.GetType().Name;
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: "order_events",
                routingKey: eventName,
                basicProperties: null,
                body: body);

            await Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
