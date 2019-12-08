using Core.Models.Interfaces;

namespace Core.Services.Encoders.Interfaces
{
    public interface IMessageEncoder 
    {
        byte[] Encode(IMessage message);

        IMessage Decode(byte[] message, IFrameMetaData frameMetaData, IClientInfo clientInfo);
    }
}
