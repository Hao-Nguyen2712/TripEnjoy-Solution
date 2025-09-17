using System.Linq.Expressions;
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

        /// <summary>
        /// Marks the given entity as modified in the DbContext so it will be updated on the next SaveChanges call.
        /// </summary>
        /// <param name="entity">The entity to mark as modified. Must be tracked or will become tracked by the context.</param>
        /// <returns>A completed task returning the same entity instance.</returns>
        public Task<T> UpdateAsync(T entity)
        {
            
            _dbContext.Entry(entity).State = EntityState.Modified;
            return Task.FromResult(entity);
        }

        public Task<T> DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            return Task.FromResult(entity);
        }

        public Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return _dbContext.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public IQueryable<T> GetQueryable()
        {
            return _dbContext.Set<T>().AsQueryable();
        }
    }
}
