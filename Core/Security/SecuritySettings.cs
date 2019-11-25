
namespace Core.Security
{
    public class SecuritySettings
    {
        public int SaltByteSize { get; set; }

        public int HashByteSize { get; set; }

        public int HashingIterationsCount { get; set; }
    }
}
