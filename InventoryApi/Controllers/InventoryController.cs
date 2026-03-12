using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryAPI.Services;
using InventoryAPI.DTOs;

namespace InventoryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<InventoryDto>> GetInventoryByProductId(int productId)
    {
        var inventory = await _inventoryService.GetInventoryByProductIdAsync(productId);
        if (inventory == null)
            return NotFound();

        return Ok(inventory);
    }

    [HttpPut("product/{productId}")]
    public async Task<ActionResult<InventoryDto>> UpdateInventory(int productId, [FromBody] UpdateInventoryDto dto)
    {
        try
        {
            var result = await _inventoryService.UpdateInventoryAsync(productId, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("check/{productId}/{quantity}")]
    public async Task<ActionResult<bool>> CheckInventory(int productId, int quantity)
    {
        var available = await _inventoryService.CheckInventoryAsync(productId, quantity);
        return Ok(new { available });
    }
}
