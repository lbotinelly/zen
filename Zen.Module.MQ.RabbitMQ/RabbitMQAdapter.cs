using RabbitMQ.Client;
using Zen.Base;
using Zen.MessageQueue.Shared;
using Zen.Base.Extension;
using System.Text;
using RabbitMQ.Client.Events;
using System.Linq;
using System.Collections.Generic;

namespace Zen.Module.MQ.RabbitMQ
{
    public class RabbitMQAdapter<T> : MessageQueuePrimitive<T>
    {
        private readonly Configuration.IOptions _options;
        private readonly IModel _channel;
        private readonly string _queueName;
        private readonly List<string> _categories;

        private static string _broadcastExchange = "broadcast"; // Changed to Topic exchange
        private static string _roundRobinExchange = "roundRobin";


        public RabbitMQAdapter()
        {
            _options = new Configuration.Options().GetSettings<Configuration.IOptions, Configuration.Options>("MessageQueue:RabbitMQ");

            var factory = new ConnectionFactory { HostName = _options.HostName };
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();

            _categories = typeof(T).GetParentTypes().Select(i => i.Name).ToList();
            _queueName = typeof(T).FullName;

            _channel.ExchangeDeclare(_roundRobinExchange, ExchangeType.Direct, true, false);
            _channel.ExchangeDeclare(_broadcastExchange, ExchangeType.Topic, true, false);

            _channel.QueueDeclare(_queueName, durable: _options.Durable, exclusive: _options.Exclusive, autoDelete: _options.AutoDelete);
            _channel.QueueBind(queue: _queueName, exchange: _roundRobinExchange, routingKey: _queueName);
        }

        public override event MessageReceivedHandler<T> Receive;

        public override void Send(T item, EDistributionStyle distributionStyle = EDistributionStyle.Broadcast)
        {
            var payload = item.ToJson();
            var body = Encoding.UTF8.GetBytes(payload);

            string exchangeName = string.Empty;
            string routingKey = _queueName; // Default routing key for round-robin

            if (distributionStyle == EDistributionStyle.RoundRobin)
            {
                exchangeName = _roundRobinExchange;
            }
            else if (distributionStyle == EDistributionStyle.Broadcast)
            {
                exchangeName = _broadcastExchange;
                routingKey = typeof(T).FullName; // Use the full type name as the routing key
            }

            _channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: null, body: body);
        }

        public override void Subscribe()
        {
            var roundRobinConsumer = new EventingBasicConsumer(_channel);
            roundRobinConsumer.Received += (model, ea) =>
            {
                if (ea.RoutingKey == _queueName)
                {
                    var body = ea.Body.ToArray();
                    var item = Encoding.UTF8.GetString(body).FromJson<T>();
                    Receive?.Invoke(item);
                }
            };
            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: roundRobinConsumer);

            var broadcastQueueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue: broadcastQueueName, exchange: _broadcastExchange, routingKey: typeof(T).FullName);

            var broadcastConsumer = new EventingBasicConsumer(_channel);
            broadcastConsumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var item = Encoding.UTF8.GetString(body).FromJson<T>();
                Receive?.Invoke(item);
            };
            _channel.BasicConsume(queue: broadcastQueueName, autoAck: true, consumer: broadcastConsumer);
        }
    }
}