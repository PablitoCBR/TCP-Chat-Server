using System.Collections.Generic;

namespace Core.Models.Interfaces
{
    public interface IMessage
    {
        IClientInfo ClientInfo { get; }

        IFrameMetaData FrameMetaData { get; }

        IDictionary<string, string> Headers { get; }

        byte[] MessageData { get; }
    }
}
