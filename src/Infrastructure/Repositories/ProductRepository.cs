using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext db) : base(db) { }

    public async Task<List<Product>> SearchByNameAsync(string term, CancellationToken ct = default)
        => await _db.Products
            .AsNoTracking()
            .Where(p => p.Name.Contains(term))
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

    public async Task<(List<Product> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Products.AsNoTracking().OrderBy(p => p.Id);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
