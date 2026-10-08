namespace Application.Interfaces;

// Generic repository: one data-access contract for every entity.
// Entity-specific queries go in dedicated interfaces (e.g. IProductRepository).
public interface IGenericRepository<T> where T : class
{
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
