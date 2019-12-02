namespace Core.Models.Enums
{
    public enum MessageType : byte
    {
        None = 0x00,
        Message = 0xAA,

        Registration = 0x01,
        Authentication = 0x02,

        Unauthenticated = 0x03,
        InvalidAuthentication = 0x04,
        UsernameAlreadyTaken = 0x05,

        InternalServerError = 0xFF
    }
}
