using System.Threading.Tasks;

namespace DAL.Repositories.Interfaces
{
    public interface IRepository<T>
    {
        Task<T> GetAsync(int id);

        Task AddAsync(T data);

        Task UpdateAsync(T data);
    }
}
