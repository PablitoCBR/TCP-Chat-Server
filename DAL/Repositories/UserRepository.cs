using System;
using System.Threading.Tasks;
using DAL.Models;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ChattyDbContext _context;

        public UserRepository(ChattyDbContext dbContext)
        {
            this._context = dbContext;
        }

        public async Task AddAsync(User data)
        {
            await this._context.Users.AddAsync(data);
            await this._context.SaveChangesAsync();
        }

        public async Task<bool> AnyWithNameAsync(string name)
            => await this._context.Users.AnyAsync(x => String.Equals(x.Username, name));


        public async Task<User> GetAsync(int id)
            => await this._context.Users.SingleOrDefaultAsync(x => x.Id == id);

        public async Task<User> GetByNameAsync(string name)
            => await this._context.Users.SingleOrDefaultAsync(x => String.Equals(x.Username, name));


        public async Task UpdateAsync(User data)
        {
            this._context.Users.Update(data);
            await this._context.SaveChangesAsync();
        }
    }
}
