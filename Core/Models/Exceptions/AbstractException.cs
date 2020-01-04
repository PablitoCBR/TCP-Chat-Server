using Core.Models.Enums;
using System;

namespace Core.Models.Exceptions
{
    public abstract class AbstractException : Exception
    {
        public MessageType ResponseMessageType { get; }

        public AbstractException(MessageType responseMessageType, string message = "")
            : base(message)
        {
            this.ResponseMessageType = responseMessageType;
        }
    }
}
