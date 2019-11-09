using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NATS.Client;
using Vote.Messaging;
using Vote.Messaging.Messages;

namespace Vote.Workers
{
    public class QueueWorker : IQueueWorker
    {
        private static ManualResetEvent _ResetEvent = new ManualResetEvent(false);

        public string Url { get; private set; } = "https://unset";
        public string Description { get; private set; } = "unset";

        private readonly IMessageQueue _messageQueue;
        private readonly IConfiguration _config;
        protected readonly ILogger _logger;

        public QueueWorker(IMessageQueue messageQueue, IConfiguration config, ILogger<QueueWorker> logger)
        {
            _messageQueue = messageQueue;
            _config = config;
            _logger = logger;
        }

        public void Start()
        {
            _logger.LogInformation($"Connecting to message queue url: {_config.GetValue<string>("MessageQueue:Url")}");
            using (var connection = _messageQueue.CreateConnection())
            {
                var subscription = connection.SubscribeAsync(UrlChangeEvent.MessageSubject);
                subscription.MessageHandler += ReceiveUrl;
                subscription.Start();
                _logger.LogInformation($"Listening on subject: {UrlChangeEvent.MessageSubject}");

                _ResetEvent.WaitOne();
                connection.Close();
            }
        }

        private void ReceiveUrl(object sender, MsgHandlerEventArgs e)
        {
            _logger.LogDebug($"Received message, subject: {e.Message.Subject}");
            var urlMessage = MessageHelper.FromData<UrlChangeEvent>(e.Message.Data);
            _logger.LogInformation($"Processing url change to '{urlMessage.Url}', description change to '{urlMessage.Description}'");
            Url = urlMessage.Url;
            Description = urlMessage.Description;
        }

    }
}