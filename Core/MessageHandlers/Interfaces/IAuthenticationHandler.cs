using Core.Models.Interfaces;
using System.Threading.Tasks;

namespace Core.MessageHandlers.Interfaces
{
    public interface IAuthenticationHandler
    {
        Task RegisterAsync(IMessage message);

        Task<IClientInfo> Authenticate(IMessage message);
    }
}
