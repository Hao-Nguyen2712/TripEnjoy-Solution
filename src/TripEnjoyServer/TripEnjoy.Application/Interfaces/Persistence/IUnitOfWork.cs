namespace TripEnjoy.Application.Interfaces.Persistence
{
        public interface IUnitOfWork : IAsyncDisposable
        {
                IAccountRepository AccountRepository { get; }
                IPropertyRepository Properties { get; }
                IAuditLogRepository AuditLogs { get; }
                IPartnerDocumentRepository PartnerDocuments { get; }
                /// <summary>
                /// Gets a repository for performing CRUD and query operations on entities of type <typeparamref name="T"/>.
                /// </summary>
                /// <typeparam name="T">The entity type managed by the repository.</typeparam>
                /// <returns>An <see cref="IGenericRepository{T}"/> instance for <typeparamref name="T"/>.</returns>
                IGenericRepository<T> Repository<T>() where T : class;
                /// <summary>
                /// Persists pending changes in the unit of work to the underlying store asynchronously.
                /// </summary>
                /// <param name="cancellationToken">Token to observe while waiting for the save operation to complete.</param>
                /// <returns>A task that resolves to the number of state entries written to the underlying store.</returns>
                Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        }
}
