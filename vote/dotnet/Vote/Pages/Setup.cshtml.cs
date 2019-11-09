using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Vote.Messaging;
using Vote.Messaging.Messages;
using Vote.Workers;

namespace Vote.Pages
{
    public class SetupModel : PageModel
    {
        protected readonly IMessageQueue _messageQueue;
        protected readonly ILogger _logger;
        protected readonly IQueueWorker _queueWorker;

        [BindProperty]
        public string LinkUrl { get; set; }
        [BindProperty]
        public string Description { get; set; }

        public SetupModel(IMessageQueue messageQueue, ILogger<SetupModel> logger, IQueueWorker queueWorker)
        {
            _messageQueue = messageQueue;
            _logger = logger;
            _queueWorker = queueWorker;
        }

        public void OnGet() {
            LinkUrl = _queueWorker.Url;
            Description = _queueWorker.Description;
        }

        public void OnPost() 
        {
            var message = new UrlChangeEvent
            {
                Url = LinkUrl,
                Description = Description
            };
            _logger.LogInformation($"Publishing URL '{message.Url}', description '{message.Description}' with subject '{message.Subject}'");
           _messageQueue.Publish(message);

           var reset = new ResetEvent();
           _logger.LogInformation($"Publishing reset message with subject '{message.Subject}'");
           _messageQueue.Publish(reset);
        }
    }
}