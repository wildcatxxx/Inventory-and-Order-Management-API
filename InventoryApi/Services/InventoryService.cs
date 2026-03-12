using InventoryAPI.Models;
using InventoryAPI.Repositories;
using InventoryAPI.DTOs;

namespace InventoryAPI.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductRepository _productRepository;

    public InventoryService(IInventoryRepository inventoryRepository, IProductRepository productRepository)
    {
        _inventoryRepository = inventoryRepository;
        _productRepository = productRepository;
    }

    public async Task<InventoryDto?> GetInventoryByProductIdAsync(int productId)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        return inventory == null ? null : MapToDto(inventory);
    }

    public async Task<InventoryDto> UpdateInventoryAsync(int productId, UpdateInventoryDto dto)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventory == null)
            throw new InvalidOperationException($"Inventory for product {productId} not found");

        inventory.QuantityOnHand = dto.QuantityOnHand;
        inventory.ReorderLevel = dto.ReorderLevel;
        inventory.ReorderQuantity = dto.ReorderQuantity;
        inventory.LastUpdated = DateTime.UtcNow;

        await _inventoryRepository.UpdateAsync(inventory);
        await _inventoryRepository.SaveChangesAsync();

        return MapToDto(inventory);
    }

    public async Task<bool> CheckInventoryAsync(int productId, int quantity)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        return inventory != null && inventory.QuantityOnHand >= quantity;
    }

    public async Task<bool> DeductInventoryAsync(int productId, int quantity)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventory == null || inventory.QuantityOnHand < quantity)
            return false;

        inventory.QuantityOnHand -= quantity;
        inventory.LastUpdated = DateTime.UtcNow;

        await _inventoryRepository.UpdateAsync(inventory);
        await _inventoryRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ReaddInventoryAsync(int productId, int quantity)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventory == null)
            return false;

        inventory.QuantityOnHand += quantity;
        inventory.LastUpdated = DateTime.UtcNow;

        await _inventoryRepository.UpdateAsync(inventory);
        await _inventoryRepository.SaveChangesAsync();

        return true;
    }

    private static InventoryDto MapToDto(Inventory inventory)
    {
        return new InventoryDto
        {
            Id = inventory.Id,
            ProductId = inventory.ProductId,
            QuantityOnHand = inventory.QuantityOnHand,
            ReorderLevel = inventory.ReorderLevel,
            ReorderQuantity = inventory.ReorderQuantity
        };
    }
}
