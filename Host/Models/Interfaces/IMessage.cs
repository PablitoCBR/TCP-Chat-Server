using Host.Models.Enums;

namespace Host.Models.Interfaces
{
    public interface IMessage
    {
        IClientInfo ClientInfo { get; }

        MessageType Type { get; }

        int HeaderLength { get; }

        int MessageLength { get; }

        byte[] HeaderData { get; }

        byte[] MessageData { get; }
    }
}
