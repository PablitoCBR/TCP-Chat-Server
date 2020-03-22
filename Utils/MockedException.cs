using System;

namespace Utils
{
    public class MockedException : Exception
    {
        public MockedException()
        {
        }

        public MockedException(string message = "")
            : base(message)
        {

        }

        public MockedException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
