using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;
using ShoppingCart.Data;
using Microsoft.EntityFrameworkCore;

namespace ShoppingCart.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    public HomeController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var products = await _db.Products.ToListAsync();
        return View(products);
    }

    public async Task<IActionResult> Cart()
    {
        var userId = User?.Identity?.Name ?? "anonymous";
        var items = await _db.CartItems.Include(ci => ci.Product).Where(ci => ci.UserId == userId).ToListAsync();
        return View(items);
    }

    public async Task<IActionResult> Checkout()
    {
        var userId = User?.Identity?.Name ?? "anonymous";
        var items = await _db.CartItems.Include(ci => ci.Product).Where(ci => ci.UserId == userId).ToListAsync();
        if (items.Any())
        {
            var orderItems = items.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                Price = ci.Product.Price,
                Quantity = ci.Quantity
            }).ToList();
            decimal total = 0;
            foreach (var oi in orderItems) total += oi.Price * oi.Quantity;
            var order = new Order { UserId = userId, CreatedAt = DateTime.Now, Total = total, Items = orderItems };
            _db.Orders.Add(order);
            _db.CartItems.RemoveRange(items);
            await _db.SaveChangesAsync();
            return View("CheckoutSuccess");
        }
        return RedirectToAction("Cart");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
