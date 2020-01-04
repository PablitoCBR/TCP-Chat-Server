using Core.Models.Enums;

namespace Core.Models.Exceptions.UserFaultExceptions
{
    public class BadMessageFormatException : AbstractException
    {
        public BadMessageFormatException(MessageType responseMessageType, string message = "") 
            : base(responseMessageType, message)
        {

        }
    }
}
