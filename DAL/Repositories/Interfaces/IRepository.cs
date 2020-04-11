using DAL.Models;
using System.Threading.Tasks;

namespace DAL.Repositories.Interfaces
{
    public interface IRepository<TEntity> where TEntity : Entity
    {
        Task<TEntity> GetAsync(int id);

        Task AddAsync(TEntity data);

        Task UpdateAsync(TEntity data);
    }
}
