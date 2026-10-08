using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

// Business logic for products. Controllers stay thin: they only map HTTP <-> DTOs.
public class ProductService
{
    private readonly IProductRepository _products;

    public ProductService(IProductRepository products)
    {
        _products = products;
    }

    public async Task<List<ProductDto>> GetAllAsync(CancellationToken ct = default)
    {
        var products = await _products.GetAllAsync(ct);
        return products.Select(ToDto).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(id, ct);
        return product is null ? null : ToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock
        };

        await _products.AddAsync(product, ct);
        await _products.SaveChangesAsync(ct);
        return ToDto(product);
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(id, ct);
        if (product is null) return null;

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.UpdatedAt = DateTime.UtcNow;

        _products.Update(product);
        await _products.SaveChangesAsync(ct);
        return ToDto(product);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(id, ct);
        if (product is null) return false;

        _products.Delete(product);
        await _products.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<ProductDto>> SearchAsync(string term, CancellationToken ct = default)
    {
        var products = await _products.SearchByNameAsync(term, ct);
        return products.Select(ToDto).ToList();
    }

    private static ProductDto ToDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        Stock = p.Stock,
        CreatedAt = p.CreatedAt
    };
}
