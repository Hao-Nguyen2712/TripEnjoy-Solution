using System.Collections;
using TripEnjoy.Application.Interfaces.Persistence;

namespace TripEnjoy.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly TripEnjoyDbContext _dbContext;
        private Hashtable? _repositories;
        private IAccountRepository? _accountRepository;

        public UnitOfWork(TripEnjoyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IAccountRepository AccountRepository => _accountRepository ??= new AccountRepository(_dbContext);

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

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            return _dbContext.DisposeAsync();
        }
    }
}
