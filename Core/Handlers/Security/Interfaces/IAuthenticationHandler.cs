namespace Core.Handlers.Security.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;

    using Core.Models.Interfaces;

    public interface IAuthenticationHandler
    {
        Task RegisterAsync(IMessage message, CancellationToken cancellationToken);

        Task<IClientInfo> Authenticate(IMessage message, CancellationToken cancellationToken);
    }
}
