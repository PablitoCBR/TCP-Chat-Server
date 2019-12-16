namespace Core.Security.Interfaces
{
    using System.Threading.Tasks;

    using Core.Models.Interfaces;

    public interface IAuthenticationHandler
    {
        Task RegisterAsync(IMessage message);

        Task<IClientInfo> Authenticate(IMessage message);
    }
}
