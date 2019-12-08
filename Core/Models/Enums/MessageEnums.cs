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
        EncryptedMessage = 0x01,

        /// <summary>
        /// Registration request completed successfully.
        /// </summary>
        Registered = 0x02,

        /// <summary>
        /// Authentication request completed successfully.
        /// </summary>
        Authenticated = 0x03,

        // ================================ Message user requests codes (51 - 100) ========================================= //

        /// <summary>
        /// Data contains message addressed to user specified in header.
        /// </summary>
        MessageSendRequest = 0x32,

        /// <summary>
        /// Message contains data required to perform DH key exchange.
        /// </summary>
        DHKeyExchangeRequest = 0x33,

        /// <summary>
        /// Step of DH key exchange.
        /// </summary>
        DHKeyExchangeStep = 0x34,

        /// <summary>
        /// Message contains data required to registration.
        /// </summary>
        RegistrationRequest = 0x35,

        /// <summary>
        /// Message contains user credentials to perform authentication.
        /// </summary>
        AuthenticationRequest = 0x36,
        
        /// <summary>
        /// Message with request of all connected users.
        /// </summary>
        ActiveUsersUpdataRequest = 0x37,

        // ================================ User fault error codes (101 - 200) ================================= //

        /// <summary>
        /// User credentials are not valid. More information in message data.
        /// </summary>
        Unauthenticated = 0x10,

        /// <summary>
        /// Failed to register. More information in message data.
        /// </summary>
        RegistrationFailed = 0x11,

        // ================================ Server fault error codes (201 - 256) =============================== //

        /// <summary>
        /// Internal server error. Unhandled exception occured.
        /// </summary>
        InternalServerError = 0xFF
    }
}
