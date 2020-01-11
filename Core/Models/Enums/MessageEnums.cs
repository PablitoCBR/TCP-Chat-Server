namespace Core.Models.Enums
{
    public enum MessageType : byte // range: 0 - 256
    {
        /// <summary>
        /// Unspecified message type (default value)
        /// </summary>
        None = 0x00,

        // ================================ Server successful response codes (1 - 50) =================================== //

        /// <summary>
        /// Data contains ecrypted message sent between users.
        /// </summary>
        Message = 0x01,

        /// <summary>
        /// Registration request completed successfully.
        /// </summary>
        Registered = 0x02,

        /// <summary>
        /// Authentication request completed successfully.
        /// </summary>
        Authenticated = 0x03,

        /// <summary>
        /// Message sent to recipient confirmation.
        /// </summary>
        MessageSent = 0x04,

        /// <summary>
        /// Response containing coma separated names of active users.
        /// </summary>
        ActiveUsers = 0x05,

        /// <summary>
        /// DH key exchange initialization message containing P and G public keys.
        /// </summary>
        DHKeyExchangeInit = 0x06,

        /// <summary>
        /// DH key exchange containing public key computed by second user.
        /// </summary>
        DHKeyExchange = 0x07,

        // ================================ Message user requests codes (51 - 100) ========================================= //

        /// <summary>
        /// Data contains message addressed to user specified in header.
        /// </summary>
        MessageSendRequest = 0x33,

        /// <summary>
        /// Message contains data required to perform DH key exchange.
        /// </summary>
        DHKeyExchangeRequest = 0x34,

        /// <summary>
        /// Step of DH key exchange.
        /// </summary>
        DHKeyExchangeStepRequest = 0x35,

        /// <summary>
        /// Message contains data required to registration.
        /// </summary>
        RegistrationRequest = 0x36,

        /// <summary>
        /// Message contains user credentials to perform authentication.
        /// </summary>
        AuthenticationRequest = 0x37,
        
        /// <summary>
        /// Message with request of all connected users.
        /// </summary>
        ActiveUsersUpdataRequest = 0x38,

        // ================================ User fault error codes (101 - 200) ================================= //

        /// <summary>
        /// User credentials are not valid. More information in message data.
        /// </summary>
        Unauthenticated = 0x65,

        /// <summary>
        /// Failed to register. More information in message data.
        /// </summary>
        RegistrationFailed = 0x66,

        /// <summary>
        /// Message was missing required header.
        /// </summary>
        MissingHeader = 0x67,

        // ================================ Server fault error codes (201 - 256) =============================== //

        /// <summary>
        /// Client was unreachable by server.
        /// </summary>
        ClientUnreachable = 0xC9,

        /// <summary>
        /// Message type code was unrecognized by server.
        /// </summary>
        UnrecognizedMessageType = 0xCA,

        /// <summary>
        /// Internal server error. Unhandled exception occured.
        /// </summary>
        InternalServerError = 0xFF
    }
}
