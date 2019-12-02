using Core.Models.Enums;
using System;

namespace Core.Models.Exceptions.UserFaultExceptions
{
    public abstract class AbstractUserFaultException : Exception
    {
        public MessageType ResponseMessageType { get; }

        public AbstractUserFaultException(MessageType responseMessageType, string message = "") : base(message)
        {
            this.ResponseMessageType = responseMessageType;
        }
    }
}
