namespace Core.Models.Interfaces
{
    using Core.Models.Enums;

    public interface IFrameMetaData
    {
        MessageType Type { get; }

        uint SenderID { get; }

        uint HeadersDataLength { get; }

        uint MessageDataLength { get; }
    }
}
