using Microsoft.EntityFrameworkCore;
using InventoryAPI.Models;
using InventoryAPI.Data;
using InventoryAPI.DTOs;
using System.Linq;

namespace InventoryAPI.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<PaginatedResult<Order>> GetAllAsync(FilterParams filters)
    {
        IQueryable<Order> query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product);

        if (!string.IsNullOrEmpty(filters.SortBy))
        {
            query = filters.SortBy.ToLower() switch
            {
                "date" => filters.Ascending ? query.OrderBy(o => o.OrderDate) : query.OrderByDescending(o => o.OrderDate),
                "amount" => filters.Ascending ? query.OrderBy(o => o.TotalAmount) : query.OrderByDescending(o => o.TotalAmount),
                "status" => filters.Ascending ? query.OrderBy(o => o.Status) : query.OrderByDescending(o => o.Status),
                _ => query.OrderBy(o => o.OrderDate)
            };
        }
        else
        {
            query = query.OrderByDescending(o => o.OrderDate);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        return new PaginatedResult<Order>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }

    public async Task<PaginatedResult<Order>> GetByUserIdAsync(int userId, FilterParams filters)
    {
        IQueryable<Order> query = _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product);

        if (!string.IsNullOrEmpty(filters.SortBy) && filters.SortBy.ToLower() == "date")
        {
            query = filters.Ascending ? query.OrderBy(o => o.OrderDate) : query.OrderByDescending(o => o.OrderDate);
        }
        else
        {
            query = query.OrderByDescending(o => o.OrderDate);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        return new PaginatedResult<Order>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
