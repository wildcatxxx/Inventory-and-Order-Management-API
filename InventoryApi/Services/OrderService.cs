using InventoryAPI.Models;
using InventoryAPI.Repositories;
using InventoryAPI.DTOs;

namespace InventoryAPI.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IInventoryService inventoryService,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _inventoryService = inventoryService;
        _logger = logger;
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        return order == null ? null : MapToDto(order);
    }

    public async Task<PaginatedResult<OrderDto>> GetAllOrdersAsync(FilterParams filters)
    {
        var result = await _orderRepository.GetAllAsync(filters);
        return new PaginatedResult<OrderDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }

    public async Task<PaginatedResult<OrderDto>> GetUserOrdersAsync(int userId, FilterParams filters)
    {
        var result = await _orderRepository.GetByUserIdAsync(userId, filters);
        return new PaginatedResult<OrderDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        _logger.LogInformation("Creating order for userId {UserId} with {ItemCount} items", dto.UserId, dto.Items.Count);

        // Validate inventory for all items first
        foreach (var item in dto.Items)
        {
            if (!await _inventoryService.CheckInventoryAsync(item.ProductId, item.Quantity))
            {
                _logger.LogWarning("Order creation failed due to insufficient inventory for productId {ProductId}", item.ProductId);
                throw new InvalidOperationException($"Insufficient inventory for product {item.ProductId}");
            }
        }

        var order = new Order
        {
            UserId = dto.UserId,
            Status = OrderStatus.Pending,
            OrderDate = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OrderItems = new List<OrderItem>()
        };

        decimal totalAmount = 0;

        foreach (var item in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Order creation failed because productId {ProductId} was not found", item.ProductId);
                throw new InvalidOperationException($"Product {item.ProductId} not found");
            }

            var orderItem = new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                Product = product
            };

            order.OrderItems.Add(orderItem);
            totalAmount += orderItem.LineTotal;

            // Deduct from inventory
            await _inventoryService.DeductInventoryAsync(item.ProductId, item.Quantity);
        }

        order.TotalAmount = totalAmount;

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} created for userId {UserId} with total {TotalAmount}", order.Id, order.UserId, order.TotalAmount);

        return MapToDto(order);
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(int id, UpdateOrderStatusDto dto)
    {
        _logger.LogInformation("Updating order {OrderId} status to {Status}", id, dto.Status);

        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            throw new InvalidOperationException($"Order {id} not found");

        if (Enum.TryParse<OrderStatus>(dto.Status, true, out var status))
        {
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            if (status == OrderStatus.Shipped)
                order.ShippedDate = DateTime.UtcNow;
            else if (status == OrderStatus.Delivered)
                order.DeliveredDate = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} status updated to {Status}", order.Id, order.Status);
        }
        else
        {
            throw new InvalidOperationException($"Invalid order status: {dto.Status}");
        }

        return MapToDto(order);
    }

    public async Task CancelOrderAsync(int id)
    {
        _logger.LogInformation("Cancelling order {OrderId}", id);

        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            throw new InvalidOperationException($"Order {id} not found");

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
            throw new InvalidOperationException("Can only cancel pending or processing orders");

        // Restock inventory
        foreach (var item in order.OrderItems)
        {
            await _inventoryService.ReaddInventoryAsync(item.ProductId, item.Quantity);
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} cancelled and inventory restored", order.Id);
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            OrderDate = order.OrderDate,
            ShippedDate = order.ShippedDate,
            DeliveredDate = order.DeliveredDate,
            Items = order.OrderItems.Select(oi => new OrderItemDetailsDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product.Name,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                LineTotal = oi.LineTotal
            }).ToList()
        };
    }
}
