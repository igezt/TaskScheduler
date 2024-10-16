using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace TaskScheduler.Queue
{
    public class KafkaProducer
    {
        private readonly ProducerConfig _config;
        private readonly string _bootstrapServers;
        private readonly ILogger<KafkaProducer> _logger;

        private readonly string _topic;
        private readonly IProducer<Null, string> _producer;

        public KafkaProducer(ILogger<KafkaProducer> logger)
        {
            _bootstrapServers = "localhost:9092";
            _config = new ProducerConfig
            {
                BootstrapServers = _bootstrapServers,
                AllowAutoCreateTopics = true,
                ApiVersionRequest =
                    false // Disable automatic version request
                ,
                SecurityProtocol = SecurityProtocol.Plaintext,
            };
            _topic = "Tasks";
            _logger = logger;
            _producer = new ProducerBuilder<Null, string>(_config).Build();
        }

        public void ProduceMessage()
        {
            var message = "Konichiwa from " + new Random().Next().ToString();

            _producer.Produce(
                _topic,
                new Message<Null, string> { Value = message },
                (deliveryReport) =>
                {
                    if (deliveryReport.Error.Code != ErrorCode.NoError)
                    {
                        _logger.LogInformation(
                            "Failed to deliver message: {string}",
                            deliveryReport.Error.Reason
                        );
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Produced event to topic {string}: value = {string}",
                            _topic,
                            message
                        );
                    }
                }
            );
        }
    }
}
