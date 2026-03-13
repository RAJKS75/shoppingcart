using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Data;
using ShoppingCart.Models;

namespace ShoppingCart.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly AppDbContext _db;
    public CartController(AppDbContext db) => _db = db;

    private string GetUserId() => User?.Identity?.Name ?? "anonymous";

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = GetUserId();
        var items = await _db.CartItems.Include(ci => ci.Product).Where(ci => ci.UserId == userId).ToListAsync();
        return Ok(items);
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] AddCartItemModel model)
    {
        var userId = GetUserId();
        var existing = await _db.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == model.ProductId);
        if (existing != null)
        {
            existing.Quantity += model.Quantity;
        }
        else
        {
            _db.CartItems.Add(new CartItem { ProductId = model.ProductId, Quantity = model.Quantity, UserId = userId });
        }
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("remove")]
    public async Task<IActionResult> Remove([FromBody] RemoveCartItemModel model)
    {
        var userId = GetUserId();
        var existing = await _db.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == model.ProductId);
        if (existing == null) return NotFound();
        _db.CartItems.Remove(existing);
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        var userId = GetUserId();
        var items = await _db.CartItems.Include(ci => ci.Product).Where(ci => ci.UserId == userId).ToListAsync();
        if (!items.Any()) return BadRequest("Cart is empty");

        var order = new Order { UserId = userId };
        decimal total = 0;
        foreach (var ci in items)
        {
            order.Items.Add(new OrderItem { ProductId = ci.ProductId, ProductName = ci.Product?.Name ?? "", Price = ci.Product?.Price ?? 0, Quantity = ci.Quantity });
            total += (ci.Product?.Price ?? 0) * ci.Quantity;
        }
        order.Total = total;
        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
        return Ok(order);
    }
}

public class AddCartItemModel { public int ProductId { get; set; } public int Quantity { get; set; } }
public class RemoveCartItemModel { public int ProductId { get; set; } }
