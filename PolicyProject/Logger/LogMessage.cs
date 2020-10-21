using System;

namespace Logger
{
    public class LogMessage
    {
        public LogMessage(DateTime timeStamp, string message, MessageTypeEnum messageType)
        {
            TimeStamp = timeStamp;
            Message = message;
            MessageType = messageType;
        }

        public DateTime TimeStamp { get; set; }

        public string Message { get; set; }

        public MessageTypeEnum MessageType { get; set; }
    }
}