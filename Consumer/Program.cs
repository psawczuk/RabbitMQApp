using Domain;
using Domain.Extensions;
using Domain.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace Consumer
{
    public class Program
    {
        private static readonly Lazy<ConnectionFactory> ConnectionFactory = new Lazy<ConnectionFactory>(CreateConnectionFactory);

        private static ConnectionFactory CreateConnectionFactory()
        {
            //// default port: 5672
            return new ConnectionFactory() { HostName = Constants.HostName };
        }

        /// <summary>
        /// Mains the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            var program = new Program();
            program.Run(args);

            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }

        /// <summary>
        /// Gets the factory.
        /// </summary>
        /// <value>
        /// The factory.
        /// </value>
        public ConnectionFactory Factory
        {
            get
            {
                return ConnectionFactory.Value;
            }
        }

        /// <summary>
        /// Runs the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void Run(string[] args)
        {
            using (var connection = this.Factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //// for Work Queues pattern
                //channel.QueueDeclare(queue: Constants.QueueName,
                //    durable: true,
                //    exclusive: false,
                //    autoDelete: false,
                //    arguments: null);

                //// for Publish/Subscribe pattern
                channel.ExchangeDeclare(exchange: Constants.ExchangeName, type: "fanout");
                ////non-durable, exclusive, autodelete queue with a generated name
                var queueName = channel.QueueDeclare().QueueName;
                ////relationship between exchange and a queue
                channel.QueueBind(queue: queueName,
                                  exchange: Constants.ExchangeName,
                                  routingKey: "");

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, basicDeliver) =>
                {
                    var data = basicDeliver.Body.FromByteArray<Message>();
                    Console.WriteLine($"Id = {data.Id}, Moment = {data.Moment.ToString("o")}, TextLength = {data.TextLength}, Text = {data.Text}");

                    //// for Work Queues pattern
                    //channel.BasicAck(deliveryTag: basicDeliver.DeliveryTag, multiple: false);
                };

                while (true)
                {
                    //// for Work Queues pattern
                    //channel.BasicConsume(queue: Constants.QueueName,
                    //    autoAck: false,
                    //    consumer: consumer);

                    //// for Publish/Subscribe pattern
                    channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);
                }
            }
        }
    }
}

