using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace TaskScheduler.Queue
{
    public class KafkaConsumer
    {
        private readonly ConsumerConfig _config;
        private readonly string _bootstrapServers;
        private readonly ILogger<KafkaConsumer> _logger;

        private readonly IConsumer<Ignore, string> _consumer;

        private readonly string _topic;

        public KafkaConsumer(ILogger<KafkaConsumer> logger)
        {
            _bootstrapServers = "localhost:9092";
            _config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                AllowAutoCreateTopics = true,
                GroupId = "Tasks",
                AutoOffsetReset =
                    AutoOffsetReset.Earliest // Start reading from earliest message
                ,
                SecurityProtocol = SecurityProtocol.Plaintext,
                ApiVersionRequest =
                    false // Disable automatic version request
                ,
            };
            _topic = "Tasks";
            _logger = logger;
            _consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
            _consumer.Subscribe(_topic);
        }

        public void ConsumeMessage()
        {
            CancellationTokenSource cts = new();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };
            try
            {
                var consumeResult = _consumer.Consume(cts.Token);
                _logger.LogInformation(
                    "Consumed message '{string}' at: '{string}'.",
                    consumeResult.Message.Value,
                    consumeResult.TopicPartitionOffset
                );
            }
            catch (OperationCanceledException)
            {
                // Ensure the consumer leaves the group cleanly and commits final offsets
                _consumer.Close();
            }
        }
    }
}
