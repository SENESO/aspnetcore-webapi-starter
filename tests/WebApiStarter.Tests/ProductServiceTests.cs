using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Moq;
using Xunit;

namespace WebApiStarter.Tests;

// ProductService is tested against a mocked IProductRepository: no database
// needed. Each test covers one service operation including its null/not-found path.
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _products = new();
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _service = new ProductService(_products.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProductsAsDtos()
    {
        _products.Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Product>
        {
            new() { Id = 1, Name = "Keyboard", Price = 49.99m, Stock = 10 },
            new() { Id = 2, Name = "Mouse", Price = 19.99m, Stock = 25 }
        });

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Keyboard", result[0].Name);
        Assert.Equal(19.99m, result[1].Price);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProduct_ReturnsDto()
    {
        _products.Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { Id = 1, Name = "Keyboard", Price = 49.99m, Stock = 10 });

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Keyboard", result.Name);
        Assert.Equal(49.99m, result.Price);
    }

    [Fact]
    public async Task GetByIdAsync_MissingProduct_ReturnsNull()
    {
        _products.Setup(p => p.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        Assert.Null(await _service.GetByIdAsync(99));
    }

    [Fact]
    public async Task CreateAsync_SavesProductAndReturnsDto()
    {
        Product? saved = null;
        _products.Setup(p => p.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => saved = p);

        var result = await _service.CreateAsync(new CreateProductDto
        {
            Name = "Monitor",
            Description = "27 inch",
            Price = 299.99m,
            Stock = 5
        });

        Assert.NotNull(saved);
        Assert.Equal("Monitor", saved.Name);
        Assert.Equal(299.99m, saved.Price);
        Assert.Equal(5, saved.Stock);
        Assert.Equal("Monitor", result.Name);
        _products.Verify(p => p.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingProduct_UpdatesFields()
    {
        var product = new Product { Id = 1, Name = "Old", Price = 10m, Stock = 1 };
        _products.Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await _service.UpdateAsync(1, new UpdateProductDto
        {
            Name = "New",
            Price = 20m,
            Stock = 2
        });

        Assert.NotNull(result);
        Assert.Equal("New", result.Name);
        Assert.Equal(20m, result.Price);
        Assert.Equal(2, result.Stock);
        Assert.NotNull(product.UpdatedAt);
        _products.Verify(p => p.Update(product), Times.Once);
        _products.Verify(p => p.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_MissingProduct_ReturnsNull()
    {
        _products.Setup(p => p.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await _service.UpdateAsync(99, new UpdateProductDto { Name = "X", Price = 1m, Stock = 0 });

        Assert.Null(result);
        _products.Verify(p => p.Update(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ExistingProduct_DeletesAndReturnsTrue()
    {
        var product = new Product { Id = 1, Name = "Keyboard" };
        _products.Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        Assert.True(await _service.DeleteAsync(1));

        _products.Verify(p => p.Delete(product), Times.Once);
        _products.Verify(p => p.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_MissingProduct_ReturnsFalse()
    {
        _products.Setup(p => p.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        Assert.False(await _service.DeleteAsync(99));

        _products.Verify(p => p.Delete(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_ReturnsMatchingProducts()
    {
        _products.Setup(p => p.SearchByNameAsync("key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { new() { Id = 1, Name = "Keyboard" } });

        var result = await _service.SearchAsync("key");

        Assert.Single(result);
        Assert.Equal("Keyboard", result[0].Name);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsItemsWithCorrectMetadata()
    {
        var items = new List<Product> { new() { Id = 3, Name = "Monitor", Price = 199.99m, Stock = 5 } };
        _products.Setup(p => p.GetPagedAsync(2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((items, 23));

        var result = await _service.GetPagedAsync(new PaginationParams(2, 10));

        Assert.Single(result.Items);
        Assert.Equal("Monitor", result.Items[0].Name);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(23, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetPagedAsync_ClampsInvalidPageAndPageSize()
    {
        _products.Setup(p => p.GetPagedAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Product>(), 0));

        var result = await _service.GetPagedAsync(new PaginationParams(-5, 500));

        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(0, result.TotalPages);
        Assert.False(result.HasNextPage);
        _products.Verify(p => p.GetPagedAsync(1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }
}
