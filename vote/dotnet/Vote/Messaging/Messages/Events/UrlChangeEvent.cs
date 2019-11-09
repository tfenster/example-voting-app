using System;

namespace Vote.Messaging.Messages
{
    public class UrlChangeEvent : Message
    {
        public override string Subject { get { return MessageSubject; } }

        public string Url {get; set;}
        
        public string Description { get; set; }

        public static string MessageSubject = "events.url.urlchange";
    }
}