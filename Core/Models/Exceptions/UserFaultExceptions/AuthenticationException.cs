﻿using Core.Models.Enums;

namespace Core.Models.Exceptions.UserFaultExceptions
{
    public class AuthenticationException : AbstractException
    {
        public AuthenticationException(MessageType responseMessageType, string message = "")
            : base(responseMessageType, message)
        {

        }
    }
}
