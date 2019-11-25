using System;

namespace DAL.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public byte[] PasswordHash { get; set; }

        public byte[] PasswordSalt { get; set; }

        public DateTime LastConnected { get; set; }

        public string LastKnownIPAddress { get; set; }

        public User()
        {

        }

        public User(string username, byte[] passwordHash, byte[] salt, DateTime lastConnected, string lastKnownIp)
        {
            this.Username = username;
            this.PasswordHash = passwordHash;
            this.PasswordSalt = salt;
            this.LastConnected = lastConnected;
            this.LastKnownIPAddress = lastKnownIp;
        }
    }
}
