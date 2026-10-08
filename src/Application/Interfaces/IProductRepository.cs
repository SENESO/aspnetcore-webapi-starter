using Domain.Entities;

namespace Application.Interfaces;

// Product-specific queries live here so the generic contract stays clean.
public interface IProductRepository : IGenericRepository<Product>
{
    Task<List<Product>> SearchByNameAsync(string term, CancellationToken ct = default);
}
