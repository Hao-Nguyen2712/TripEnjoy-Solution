using Microsoft.EntityFrameworkCore;
using TripEnjoy.Application.Interfaces.Persistence;

namespace TripEnjoy.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly TripEnjoyDbContext _dbContext;

        public GenericRepository(TripEnjoyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            return entity;
        }

        public Task<T> UpdateAsync(T entity)
        {
            _dbContext.Set<T>().Update(entity);
            return Task.FromResult(entity);
        }

        public Task<T> DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            return Task.FromResult(entity);
        }
    }
}
