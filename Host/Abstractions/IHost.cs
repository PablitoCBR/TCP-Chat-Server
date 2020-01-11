namespace Host.Abstractions
{
    public interface IHost
    {
        bool IsActive { get; }
        void Run();
        void Reset();
        void Stop();
    }
}
