using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NATS.Client;
using Worker.Data;
using Worker.Messaging;
using Worker.Messaging.Messages;

namespace Worker.Workers
{
    public class QueueWorker
    {
        private static ManualResetEvent _ResetEvent = new ManualResetEvent(false);
        private const string QUEUE_GROUP = "save-and-reset-handler";

        private readonly IMessageQueue _messageQueue;
        private readonly IConfiguration _config;
        private readonly IVoteData _data;
        protected readonly ILogger _logger;

        public QueueWorker(IMessageQueue messageQueue, IVoteData data, IConfiguration config, ILogger<QueueWorker> logger)
        {
            _messageQueue = messageQueue;
            _data = data;
            _config = config;
            _logger = logger;
        }

        public void Start()
        {       
            
            Task.Run(() => {
                RegisterListener(VoteCastEvent.MessageSubject, SaveVote);
            });
            Task.Run(() => {
                RegisterListener(ResetEvent.MessageSubject, ResetVotes);
            });
            
            _ResetEvent.WaitOne();
        }

        private void RegisterListener(string subject, EventHandler<MsgHandlerEventArgs> action) {
            _logger.LogInformation($"Connecting to message queue url: {_config.GetValue<string>("MessageQueue:Url")} for subject {subject}");
            using (var connection = _messageQueue.CreateConnection())
            {
            var subscription = connection.SubscribeAsync(subject, QUEUE_GROUP);
            subscription.MessageHandler += action;
            subscription.Start();
            _logger.LogInformation($"Listening on subject: {subject}, queue: {QUEUE_GROUP}");

            _ResetEvent.WaitOne();

            _logger.LogInformation($"No longer listening on subject: {subject}, queue: {QUEUE_GROUP}");
            connection.Close();
            }
    }

        private void SaveVote(object sender, MsgHandlerEventArgs e)
        {
            _logger.LogDebug($"Received message, subject: {e.Message.Subject}");
            var voteMessage = MessageHelper.FromData<VoteCastEvent>(e.Message.Data);
            _logger.LogInformation($"Processing vote for '{voteMessage.Vote}' by '{voteMessage.VoterId}'");
            try
            {
                _data.Set(voteMessage.VoterId, voteMessage.Vote);
                _logger.LogInformation($"Succesfully processed vote by '{voteMessage.VoterId}'");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Vote processing FAILED for '{voteMessage.VoterId}', exception: {ex}");
            }
            
        }

        private void ResetVotes(object sender, MsgHandlerEventArgs e)
        {
            _logger.LogDebug($"Received message, subject: {e.Message.Subject}");
            var resetMessage = MessageHelper.FromData<ResetEvent>(e.Message.Data);
            _logger.LogInformation($"Removing all votes");
            try
            {
                _data.Reset();
                _logger.LogDebug($"Succesfully reset votes");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Vote resetting FAILED, exception: {ex}");
            }
            
        }
    }
}