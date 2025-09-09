using System.Collections;
using TripEnjoy.Application.Interfaces.Persistence;

namespace TripEnjoy.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly TripEnjoyDbContext _dbContext;
        private Hashtable? _repositories;
        private IAccountRepository? _accountRepository;

        /// <summary>
        /// Initializes a new <see cref="UnitOfWork"/> using the provided <see cref="TripEnjoyDbContext"/> for repository coordination and persistence operations.
        /// </summary>
        public UnitOfWork(TripEnjoyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IAccountRepository AccountRepository => _accountRepository ??= new AccountRepository(_dbContext);

        /// <summary>
        /// Gets a cached generic repository for the entity type <typeparamref name="T"/>.
        — Returns a lazily created <see cref="IGenericRepository{T}"/> instance and caches it for reuse.
        /// </summary>
        /// <typeparam name="T">The entity type the repository will manage.</typeparam>
        /// <returns>An <see cref="IGenericRepository{T}"/> for <typeparamref name="T"/>. The instance is created on first request and then reused.</returns>
        public IGenericRepository<T> Repository<T>() where T : class
        {
            _repositories ??= new Hashtable();

            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(GenericRepository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _dbContext);
                _repositories.Add(type, repositoryInstance);
            }

            return (IGenericRepository<T>)_repositories[type]!;
        }

        /// <summary>
        /// Asynchronously saves all changes made in the current DbContext to the database.
        /// </summary>
        /// <param name="cancellationToken">Token to observe while waiting for the operation to complete.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result is the number of state entries written to the database.</returns>
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously disposes the unit of work by disposing the underlying <c>TripEnjoyDbContext</c>.
        /// </summary>
        /// <remarks>
        /// After this completes, the unit of work and any repositories obtained from it must not be used.
        /// </remarks>
        /// <returns>A <see cref="ValueTask"/> that represents the asynchronous dispose operation.</returns>
        public ValueTask DisposeAsync()
        {
            return _dbContext.DisposeAsync();
        }
    }
}
