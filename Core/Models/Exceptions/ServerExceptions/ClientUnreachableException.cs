using Core.Models.Enums;

namespace Core.Models.Exceptions.ServerExceptions
{
    public class ClientUnreachableException : AbstractException
    {
        public string ClientName { get; }

        public ClientUnreachableException(ClientInfo clientInfo, MessageType responseMessageType, string message = "") 
            : this(clientInfo.Name, responseMessageType, message)
        {

        }

        public ClientUnreachableException(string clientName, MessageType responseMessageType, string message = "") 
            : base(responseMessageType, message)
        {
            this.ClientName = clientName;
        }
    }
}
