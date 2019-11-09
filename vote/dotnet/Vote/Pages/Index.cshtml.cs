using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vote.Messaging;
using Vote.Messaging.Messages;
using Vote.Workers;

namespace Vote.Pages
{
    public class IndexModel : PageModel
    {
        private string _optionA;
        private string _optionB;

        protected readonly IMessageQueue _messageQueue;
        protected readonly IConfiguration _configuration;
        protected readonly ILogger _logger;
        protected readonly IQueueWorker _queueWorker;

        public IndexModel(IMessageQueue messageQueue, IConfiguration configuration, ILogger<IndexModel> logger, IQueueWorker queueWorker)
        {
            _messageQueue = messageQueue;
            _configuration = configuration;
            _logger = logger;
            _queueWorker = queueWorker;

            _optionA = _configuration.GetValue<string>("Voting:OptionA");
            _optionB = _configuration.GetValue<string>("Voting:OptionB");
        }

        public string OptionA { get; private set; }

        public string OptionB { get; private set; }

        public string NextLink { get; private set; } = "unset";
        public string NextDescription { get; private set; } = "desc";

        [BindProperty]
        public string Vote { get; private set; }

        private string _voterId 
        {
            get { return TempData.Peek("VoterId") as string; }
            set { TempData["VoterId"] = value; }
        }

        public void OnGet()
        {
            OptionA = _optionA;
            OptionB = _optionB;
            NextLink = _queueWorker.Url;
            NextDescription = _queueWorker.Description;
        }

        public IActionResult OnPost(string vote)
        {
            Vote = vote;
            OptionA = _optionA;
            OptionB = _optionB;
            if (_configuration.GetValue<bool>("MessageQueue:Enabled"))
            {
                PublishVote(vote);
            }
            NextLink = _queueWorker.Url;
            NextDescription = _queueWorker.Description;
            return Page();
        }

        private void PublishVote(string vote)
        {
            if (string.IsNullOrEmpty(_voterId))
            {
                _voterId = Guid.NewGuid().ToString();
            }
            var message = new VoteCastEvent
            {
                VoterId = _voterId,
                Vote = vote
            };
           _messageQueue.Publish(message);
        }
    }
}
