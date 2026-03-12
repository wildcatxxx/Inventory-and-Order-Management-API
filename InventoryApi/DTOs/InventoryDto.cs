namespace InventoryAPI.DTOs;

public class InventoryDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int QuantityOnHand { get; set; }
    public int ReorderLevel { get; set; }
    public int ReorderQuantity { get; set; }
}

public class UpdateInventoryDto
{
    public int QuantityOnHand { get; set; }
    public int ReorderLevel { get; set; }
    public int ReorderQuantity { get; set; }
}
