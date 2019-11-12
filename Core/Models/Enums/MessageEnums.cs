namespace Core.Models.Enums
{
    public enum MessageType : byte
    {
        Message = 0x00,

        Registration = 0x01,
        Authentication = 0x02,

        Error = 0xFF
    }
}
