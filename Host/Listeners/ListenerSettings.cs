using System.ComponentModel.DataAnnotations;

namespace Host.Listeners
{
    public class ListenerSettings
    {
        [Range(1, int.MaxValue, ErrorMessage = "Invalid pending connection queue length")]
        public int PendingConnectionsQueue { get; set; }
    }
}
