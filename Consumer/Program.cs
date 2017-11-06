using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Domain;
using Domain.Models;
using RabbitMQ.Client.Events;
using Domain.Extensions;

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

        public ConnectionFactory Factory
        {
            get
            {
                return ConnectionFactory.Value;
            }
        }

        public void Run(string[] args)
        {
            using (var connection = this.Factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: Constants.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, basicDeliver) =>
                {
                    var data = basicDeliver.Body.FromByteArray<Message>();
                    Console.WriteLine($"Id = {data.Id}, Moment = {data.Moment.ToString("o")}, TextLength = {data.TextLength}, Text = {data.Text}");

                    channel.BasicAck(deliveryTag: basicDeliver.DeliveryTag, multiple: false);
                };

                while (true)
                {
                    channel.BasicConsume(queue: Constants.QueueName,
                        autoAck: false,
                        consumer: consumer);
                }
            }
        }
    }
}

