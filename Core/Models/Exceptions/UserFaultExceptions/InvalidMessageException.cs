using Core.Models.Enums;

namespace Core.Models.Exceptions.UserFaultExceptions
{
    public class InvalidMessageException : AbstractException
    {
        public InvalidMessageException(MessageType responseMessageType, string message = "") 
            : base(responseMessageType, message)
        {
        }
    }
}
