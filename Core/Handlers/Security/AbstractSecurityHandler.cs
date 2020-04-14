using Core.Models.Consts;
using Core.Models.Enums;
using Core.Models.Exceptions.UserFaultExceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Handlers.Security
{
    public abstract class AbstractSecurityHandler
    {
        protected (string username, string password) GetCredentials(IDictionary<string, string> headers)
        {
            if (!headers.ContainsKey(MessageHeaders.Authentication))
                throw new AuthenticationException(MessageType.Unauthenticated, "Authentication header was not found.");

            string credentialsBase64 = headers[MessageHeaders.Authentication];
            string credentailsString = Encoding.ASCII.GetString(Convert.FromBase64String(credentialsBase64));
            string[] credentials = credentailsString.Split(':');
            return (credentials[0], credentials[1]);
        }
    }
}
