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

        private static string _broadcastExchange = "broadcast";
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
            _channel.ExchangeDeclare(_broadcastExchange, ExchangeType.Fanout, true, false);

            _channel.QueueDeclare(_queueName, durable: _options.Durable, exclusive: _options.Exclusive, autoDelete: _options.AutoDelete);

            _channel.QueueBind(queue: _queueName, exchange: _broadcastExchange, routingKey: _queueName);
            _channel.QueueBind(queue: _queueName, exchange: _roundRobinExchange, routingKey: _queueName);

        }

        public override event MessageReceivedHandler<T> Receive;

        public override void Send(T item, EDistributionStyle distributionStyle = EDistributionStyle.Broadcast)
        {
            var payload = item.ToJson();
            var body = Encoding.UTF8.GetBytes(payload);

            var dist = distributionStyle == EDistributionStyle.RoundRobin ? _roundRobinExchange : _broadcastExchange;

            _channel.BasicPublish(exchange: dist, routingKey: _queueName, body: body);
            //_channel.BasicPublish(exchange: string.Empty, routingKey: _queueName, body: body);
        }

        public override void Subscribe()
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                if (ea.RoutingKey != _queueName) return;

                var body = ea.Body.ToArray();
                var item = Encoding.UTF8.GetString(body).FromJson<T>();

                Receive?.Invoke(item);
            };

            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        }
    }
}