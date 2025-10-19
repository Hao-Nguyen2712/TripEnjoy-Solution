using System.Collections;
using TripEnjoy.Application.Interfaces.Persistence;

namespace TripEnjoy.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly TripEnjoyDbContext _dbContext;
        private Hashtable? _repositories;
        private IAccountRepository? _accountRepository;
        private IPropertyRepository? _propertyRepository;
        private IAuditLogRepository? _auditLogRepository;
        private IPartnerDocumentRepository? _partnerDocumentRepository;

        /// <summary>
        /// Initializes a new <see cref="UnitOfWork"/> using the provided <see cref="TripEnjoyDbContext"/> for repository coordination and persistence operations.
        /// </summary>
        public UnitOfWork(TripEnjoyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IAccountRepository AccountRepository => _accountRepository ??= new AccountRepository(_dbContext);

        public IPropertyRepository Properties => _propertyRepository ??= new PropertyRepository(_dbContext);

        public IAuditLogRepository AuditLogs => _auditLogRepository ??= new AuditLogRepository(_dbContext);

        public IPartnerDocumentRepository PartnerDocuments => _partnerDocumentRepository ??= new PartnerDocumentRepository(_dbContext);

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
