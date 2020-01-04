namespace Core.Models.Interfaces
{
    using Core.Models.Enums;

    public interface IFrameMetaData
    {
        MessageType Type { get; }

        int HeadersDataLength { get; }

        int MessageDataLength { get; }
    }
}
