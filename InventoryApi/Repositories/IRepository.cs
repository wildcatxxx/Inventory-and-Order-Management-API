using InventoryAPI.Models;
using InventoryAPI.DTOs;

namespace InventoryAPI.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<PaginatedResult<Product>> GetAllAsync(FilterParams filters);
    Task<Product?> GetBySkuAsync(string sku);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task SaveChangesAsync();
}

public interface IInventoryRepository
{
    Task<Inventory?> GetByIdAsync(int id);
    Task<Inventory?> GetByProductIdAsync(int productId);
    Task AddAsync(Inventory inventory);
    Task UpdateAsync(Inventory inventory);
    Task SaveChangesAsync();
}

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<PaginatedResult<Order>> GetAllAsync(FilterParams filters);
    Task<PaginatedResult<Order>> GetByUserIdAsync(int userId, FilterParams filters);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task SaveChangesAsync();
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task SaveChangesAsync();
}
