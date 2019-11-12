using System;

namespace Core.Models.Exceptions
{
    public class UnsupportedMessageType : Exception
    {
        public byte MessageTypeCode { get; }

        public UnsupportedMessageType(byte messageTypeCode, string message = "") : base(message)
        {
            MessageTypeCode = messageTypeCode;
        }
    }
}
