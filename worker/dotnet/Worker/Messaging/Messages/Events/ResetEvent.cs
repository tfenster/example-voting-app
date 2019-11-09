using System;

namespace Worker.Messaging.Messages
{
    public class ResetEvent : Message
    {
        public override string Subject { get { return MessageSubject; } }

        public static string MessageSubject = "events.vote.reset";
    }
}