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
}
