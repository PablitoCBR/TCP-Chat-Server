using Host.Abstractions;

namespace Host.Builder.Interfaces
{
    public interface IHostBuilder
    {
        IHost Build();
    }
}
