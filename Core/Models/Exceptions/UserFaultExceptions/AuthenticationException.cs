using Core.Models.Enums;

namespace Core.Models.Exceptions.UserFaultExceptions
{
    public class AuthenticationException : AbstractUserFaultException
    {
        public AuthenticationException(MessageType responseMessageType, string message = "")
            : base(responseMessageType, message)
        {

        }
    }
}
