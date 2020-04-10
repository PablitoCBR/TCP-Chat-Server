namespace Core.Handlers.Security.Interfaces
{
    using Core.Models;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAuthenticationHandler
    {
        Task RegisterAsync(Message message, CancellationToken cancellationToken);

        Task<ClientInfo> Authenticate(Message message, CancellationToken cancellationToken);
    }
}
