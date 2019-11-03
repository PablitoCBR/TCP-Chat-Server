using Host.Models.Enums;
using Host.Models.Interfaces;

namespace Host.Models
{
    public class Message : IMessage
    {
        public IClientInfo ClientInfo { get; internal set; }

        public MessageType Type { get; internal set; }

        public int HeaderLength { get; internal set; }

        public int MessageLength { get; internal set; }

        public byte[] HeaderData { get; internal set; }

        public byte[] MessageData { get; internal set; }
    }
}
