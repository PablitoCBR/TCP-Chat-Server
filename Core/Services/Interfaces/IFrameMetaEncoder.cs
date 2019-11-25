using Core.Models.Enums;
using Core.Models.Interfaces;

namespace Core.Services.Interfaces
{
    public interface IFrameMetaEncoder
    {
        IFrameMetaData Decode(byte[] frameMetaData);

        byte[] Encode(IFrameMetaData frameMetaData);

        int GetSenderId(byte[] frameMetaData);

        MessageType GetMessageType(byte[] frameMetaData);

        int GetHeadersDataLength(byte[] frameMetaData);

        int GetMessageDataLength(byte[] frameMetaData);
    }
}
