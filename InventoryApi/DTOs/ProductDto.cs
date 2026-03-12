using System.ComponentModel.DataAnnotations;

namespace InventoryAPI.DTOs;

public class CreateProductDto
{
    [Required]
    [StringLength(255, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 1000000000)]
    public decimal Price { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Sku { get; set; } = string.Empty;
}

public class UpdateProductDto
{
    [Required]
    [StringLength(255, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 1000000000)]
    public decimal Price { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Sku { get; set; } = string.Empty;
    public InventoryDto? Inventory { get; set; }
}
