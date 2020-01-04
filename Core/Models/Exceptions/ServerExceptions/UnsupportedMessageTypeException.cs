using Core.Models.Enums;

namespace Core.Models.Exceptions.ServerExceptions
{
    public class UnsupportedMessageTypeException : AbstractException
    {
        public byte UsedMessageTypeCode { get; }

        public UnsupportedMessageTypeException(byte usedMessageTypeCode, MessageType responseMessageType, string message = "") : base(responseMessageType, message)
        {
            this.UsedMessageTypeCode = usedMessageTypeCode;
        }
    }
}