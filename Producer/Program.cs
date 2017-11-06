using Domain;
using Domain.Extensions;
using Domain.Models;
using RabbitMQ.Client;
using System;

namespace Producer
{
    public class Program
    {
        private static readonly Lazy<ConnectionFactory> ConnectionFactory = new Lazy<ConnectionFactory>(CreateConnectionFactory);

        private static ConnectionFactory CreateConnectionFactory()
        {
            //// default port: 5672
            return new ConnectionFactory() { HostName = "localhost" };
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

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                while (true)
                {
                    var message = GetMessage();
                    if (message.Text.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return;
                    }

                    //// for Work Queues pattern
                    //channel.BasicPublish(exchange: "",
                    //    routingKey: Constants.QueueName,
                    //    basicProperties: properties,
                    //    body: message.ToByteArray());

                    //// for Publish/Subscribe pattern
                    channel.BasicPublish(exchange: Constants.ExchangeName,
                                 routingKey: "",
                                 basicProperties: null,
                                 body: message.ToByteArray());
                }
            }
        }

        private static Message GetMessage()
        {
            Console.Write("Message> ");
            var message = Console.ReadLine();

            return new Message()
            {
                Id = Guid.NewGuid(),
                Moment = DateTime.UtcNow,
                Text = message
            };
        }
    }
}
