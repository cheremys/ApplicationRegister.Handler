using ApplicationRegister.Handler.Entities;
using ApplicationRegister.Handler.Interfaces;
using ApplicationRegister.Handler.Models;
using ApplicationRegister.Handler.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ApplicationRegister.Handler.Services
{
    internal class Worker : IWorker
    {
        private const string createMessange = "create";
        private const string getMessange = "get";
        private readonly string queueName;
        private readonly string hostName;
        private readonly ILogger<Worker> logger;
        private readonly IApplicationRepository repository;
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly IConfigurationRoot config;

        public Worker(ILogger<Worker> logger, IApplicationRepository repository, IConfigurationRoot config)
        {
            this.logger = logger;
            this.repository = repository;
            this.config = config;
            this.queueName = config.GetSection("queueName").Get<string>();
            this.hostName = config.GetSection("hostName").Get<string>();
            var factory = new ConnectionFactory() { HostName = hostName };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
        }

        public void Run()
        {

            channel.QueueDeclare(queue: queueName, durable: false,
                  exclusive: false, autoDelete: false, arguments: null);
            channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue: queueName,
              autoAck: false, consumer: consumer);

            logger.LogInformation("Awaiting RPC requests");

            consumer.Received += (model, ea) =>
            {
                string response = null;

                var body = ea.Body.ToArray();
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                try
                {
                    var message = Encoding.UTF8.GetString(body.ToArray());

                    logger.LogInformation("Got message " + message);

                    if (message.Contains(createMessange))
                    {
                        message = message.Remove(0, createMessange.Length);
                        Application application = JsonSerializer.Deserialize<Application>(message);
                        response = repository.CreateApplication(application).ToString();
                    }
                    else if (message.Contains(getMessange))
                    {
                        message = message.Remove(0, getMessange.Length);
                        RequestModel requestModel = JsonSerializer.Deserialize<RequestModel>(message);
                        IEnumerable<Application> applications = repository.GetApplications(requestModel.Id, requestModel.Address);
                        response = JsonSerializer.Serialize<IEnumerable<Application>>(applications);
                    }
                }
                catch (Exception exception)
                {
                    response = "";
                    logger.LogError(exception.Message);
                }
                finally
                {
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                      basicProperties: replyProps, body: responseBytes);
                    channel.BasicAck(deliveryTag: ea.DeliveryTag,
                      multiple: false);

                    logger.LogInformation("Sent message " + response);
                }
            };
        }

        public void Stop()
        {
            this.connection.Dispose();
            this.channel.Dispose();
            logger.LogInformation("Worker stoped");
        }
    }
}
