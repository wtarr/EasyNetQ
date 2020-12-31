using System;

namespace EasyNetQ.Events
{
    public readonly struct AckEvent
    {
        public MessageReceivedInfo ReceivedInfo { get; }
        public MessageProperties Properties { get; }
        public ReadOnlyMemory<byte> Body { get; }
        public AckResult AckResult { get; }

        public AckEvent(MessageReceivedInfo info, MessageProperties properties, in ReadOnlyMemory<byte> body, AckResult ackResult)
        {
            ReceivedInfo = info;
            Properties = properties;
            Body = body;
            AckResult = ackResult;
        }
    }

    public enum AckResult
    {
        Ack,
        Nack,
        Exception
    }
}
