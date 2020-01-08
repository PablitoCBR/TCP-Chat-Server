using Core.Models.Enums;
using System;
using System.Collections.Generic;

namespace Core.Models.Exceptions
{
    public abstract class AbstractException : Exception
    {
        public MessageType ResponseMessageType { get; }

        public IDictionary<string, string> ResponseHeaders { get; }

        public AbstractException(MessageType responseMessageType, string message = "", IDictionary<string, string> responseHeaders = null)
            : base(message)
        {
            this.ResponseMessageType = responseMessageType;
            this.ResponseHeaders = responseHeaders ?? new Dictionary<string, string>();
        }
    }
}
