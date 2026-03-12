using InventoryAPI.Models;
using InventoryAPI.DTOs;

namespace InventoryAPI.Services;

public interface IProductService
{
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<PaginatedResult<ProductDto>> GetAllProductsAsync(FilterParams filters);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto);
    Task DeleteProductAsync(int id);
}

public interface IInventoryService
{
    Task<InventoryDto?> GetInventoryByProductIdAsync(int productId);
    Task<InventoryDto> UpdateInventoryAsync(int productId, UpdateInventoryDto dto);
    Task<bool> CheckInventoryAsync(int productId, int quantity);
    Task<bool> DeductInventoryAsync(int productId, int quantity);
    Task<bool> ReaddInventoryAsync(int productId, int quantity);
}

public interface IOrderService
{
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<PaginatedResult<OrderDto>> GetAllOrdersAsync(FilterParams filters);
    Task<PaginatedResult<OrderDto>> GetUserOrdersAsync(int userId, FilterParams filters);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
    Task<OrderDto> UpdateOrderStatusAsync(int id, UpdateOrderStatusDto dto);
    Task CancelOrderAsync(int id);
}

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<UserDto?> GetUserByIdAsync(int id);
    string GenerateJwtToken(User user);
}
