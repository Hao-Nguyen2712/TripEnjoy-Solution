namespace TripEnjoy.Application.Interfaces.Persistence
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IAccountRepository AccountRepository { get; }
        IGenericRepository<T> Repository<T>() where T : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
