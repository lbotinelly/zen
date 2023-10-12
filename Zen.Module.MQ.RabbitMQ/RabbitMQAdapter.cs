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

        public RabbitMQAdapter()
        {
            _options = new Configuration.Options().GetSettings<Configuration.IOptions, Configuration.Options>("MessageQueue:RabbitMQ");

            var factory = new ConnectionFactory { HostName = _options.HostName };
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();

            _categories = typeof(T).GetParentTypes().Select(i => i.Name).ToList();
            _queueName = typeof(T).FullName;


            _channel.QueueDeclare(_queueName, durable: _options.Durable, exclusive: _options.Exclusive, autoDelete: _options.AutoDelete);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var item = Encoding.UTF8.GetString(body).FromJson<T>();

                Receive?.Invoke(item);
            };

            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        }

        public override event MessageReceivedHandler<T> Receive;

        public override void Send(T item)
        {
            var payload = item.ToJson();
            var body = Encoding.UTF8.GetBytes(payload);

            _channel.BasicPublish(exchange: string.Empty, routingKey: _queueName, basicProperties: null, body: body);
        }
    }
}