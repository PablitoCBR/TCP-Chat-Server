namespace Host.Abstractions
{
    public interface IHost
    {
        void Run();
        void Reset();
        void Stop();
    }
}
