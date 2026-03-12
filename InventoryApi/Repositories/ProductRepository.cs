using Microsoft.EntityFrameworkCore;
using InventoryAPI.Models;
using InventoryAPI.Data;
using InventoryAPI.DTOs;
using System.Linq;

namespace InventoryAPI.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PaginatedResult<Product>> GetAllAsync(FilterParams filters)
    {
        IQueryable<Product> query = _context.Products.Include(p => p.Inventory);

        if (!string.IsNullOrEmpty(filters.SortBy))
        {
            query = filters.SortBy.ToLower() switch
            {
                "name" => filters.Ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "price" => filters.Ascending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Id)
            };
        }
        else
        {
            query = query.OrderBy(p => p.Id);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        return new PaginatedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _context.Products
            .Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Sku == sku);
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var product = await GetByIdAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
