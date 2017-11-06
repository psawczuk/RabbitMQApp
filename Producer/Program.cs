using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Domain.Models;
using Newtonsoft.Json;
using Domain;
using Domain.Extensions;

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

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                while (true)
                {
                    var message = GetMessage();
                    if (message.Text.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return;
                    }

                    channel.BasicPublish(exchange: "",
                        routingKey: Constants.QueueName,
                        basicProperties: properties,
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
