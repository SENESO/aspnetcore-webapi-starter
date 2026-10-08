using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

// Single EF Core implementation behind IGenericRepository<T>.
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext _db;

    public GenericRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<T>> GetAllAsync(CancellationToken ct = default)
        => await _db.Set<T>().AsNoTracking().ToListAsync(ct);

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Set<T>().FindAsync(new object[] { id }, ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _db.Set<T>().AddAsync(entity, ct);

    public void Update(T entity)
        => _db.Set<T>().Update(entity);

    public void Delete(T entity)
        => _db.Set<T>().Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
