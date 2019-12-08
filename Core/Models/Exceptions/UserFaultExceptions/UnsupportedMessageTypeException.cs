using System;

namespace Core.Models.Exceptions
{
    public class UnsupportedMessageTypeException : Exception
    {
        public byte MessageTypeCode { get; }

        public UnsupportedMessageTypeException(byte messageTypeCode, string message = "") : base(message)
        {
            MessageTypeCode = messageTypeCode;
        }
    }
}