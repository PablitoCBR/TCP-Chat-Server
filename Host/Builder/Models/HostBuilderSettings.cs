using System.ComponentModel.DataAnnotations;

namespace Host.Builder.Models
{
    public class HostBuilderSettings
    {
        [Range(1, 65535, ErrorMessage = "Invalid port number!")]
        public int Port { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Invalid pending connection queue length")]
        public int PendingConnectionsQueue { get; set; }
    }
}
