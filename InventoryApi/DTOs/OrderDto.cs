using System.ComponentModel.DataAnnotations;

namespace InventoryAPI.DTOs;

public class CreateOrderDto
{
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Required]
    [MinLength(1)]
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public List<OrderItemDetailsDto> Items { get; set; } = new();
}

public class OrderItemDetailsDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class UpdateOrderStatusDto
{
    [Required]
    [RegularExpression("^(Pending|Processing|Shipped|Delivered|Cancelled|Failed)$", ErrorMessage = "Status must be one of: Pending, Processing, Shipped, Delivered, Cancelled, Failed.")]
    public string Status { get; set; } = string.Empty;
}
