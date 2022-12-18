using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BlazorSozluk.Common.Infastructure
{
    public static class QueueFactory
    {
        public static void SendMessageToExchange(string exchangeName,
                                       string exchangeType,
                                       string queuName,
                                       object obj)
        {
            var channel = CreateBasicConsumer()
                            .EnsureExchange(exchangeName, exchangeType)
                            .EnsureQueue(queuName, exchangeName)
                            .Model;

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj));

            channel.BasicPublish(exchange: exchangeName,
                                 routingKey: queuName,
                                 basicProperties: null,
                                 body: body);
        }

        public static EventingBasicConsumer CreateBasicConsumer()
        {
            var factory = new ConnectionFactory() { HostName = SozlukConstants.RabbitMQHost };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            return new EventingBasicConsumer(channel);
        }

        public static EventingBasicConsumer EnsureExchange(this EventingBasicConsumer consumer,
                                                           string exchangeName,
                                                           string exchangeType = SozlukConstants.DefaultExcangeType)
        {
            consumer.Model.ExchangeDeclare(exchange: exchangeName,
                                           type: exchangeType,
                                           durable: false,
                                           autoDelete: false);

            return consumer;
        }

        public static EventingBasicConsumer EnsureQueue(this EventingBasicConsumer consumer,
                                                           string queueName,
                                                           string exchangeName)
        {
            consumer.Model.QueueDeclare(queue: queueName,
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);
            // routing key tekrardan queueName vermemizin sebebi, direct key kullanmamız,
            // direct tipinde route keyler, queue ile aynı oluyor
            consumer.Model.QueueBind(queueName, exchangeName, queueName);

            return consumer;
        }
    }
}
