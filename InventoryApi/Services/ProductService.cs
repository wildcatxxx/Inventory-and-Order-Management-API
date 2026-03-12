using InventoryAPI.Models;
using InventoryAPI.Repositories;
using InventoryAPI.DTOs;

namespace InventoryAPI.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IInventoryRepository _inventoryRepository;

    public ProductService(IProductRepository repository, IInventoryRepository inventoryRepository)
    {
        _repository = repository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product == null ? null : MapToDto(product);
    }

    public async Task<PaginatedResult<ProductDto>> GetAllProductsAsync(FilterParams filters)
    {
        var result = await _repository.GetAllAsync(filters);
        return new PaginatedResult<ProductDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Sku = dto.Sku,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(product);
        await _repository.SaveChangesAsync();

        // Create inventory for the product
        var inventory = new Inventory
        {
            ProductId = product.Id,
            QuantityOnHand = 0,
            ReorderLevel = 10,
            ReorderQuantity = 50,
            LastUpdated = DateTime.UtcNow
        };

        await _inventoryRepository.AddAsync(inventory);
        await _inventoryRepository.SaveChangesAsync();
        return MapToDto(product);
    }

    public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            throw new InvalidOperationException($"Product with id {id} not found");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(product);
        await _repository.SaveChangesAsync();

        return MapToDto(product);
    }

    public async Task DeleteProductAsync(int id)
    {
        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Sku = product.Sku,
            Inventory = product.Inventory == null ? null : new InventoryDto
            {
                Id = product.Inventory.Id,
                ProductId = product.Inventory.ProductId,
                QuantityOnHand = product.Inventory.QuantityOnHand,
                ReorderLevel = product.Inventory.ReorderLevel,
                ReorderQuantity = product.Inventory.ReorderQuantity
            }
        };
    }
}
