using Microsoft.EntityFrameworkCore.Metadata;
using OMS.Application.Interfaces;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Infrastructure.Messaging
{
    public class RabbitMQEventPublisher : IEventPublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public Task PublishAsync<T>(T @event) where T : class
        {
            throw new NotImplementedException();
        }

        // Implementasyon...
    }
}
