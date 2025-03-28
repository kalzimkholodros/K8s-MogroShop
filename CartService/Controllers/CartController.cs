using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CartService.Data;
using CartService.Models;
using CartService.Services;
using Microsoft.AspNetCore.Authorization;

namespace CartService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly CartDbContext _context;
    private readonly RedisService _redisService;
    private readonly ILogger<CartController> _logger;

    public CartController(
        CartDbContext context,
        RedisService redisService,
        ILogger<CartController> logger)
    {
        _context = context;
        _redisService = redisService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<Cart>> GetCart()
    {
        try
        {
            var userId = GetUserIdFromToken();
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return Ok(cart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart for user");
            return StatusCode(500, "An error occurred while retrieving the cart");
        }
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartItem>> AddItem([FromBody] CartItem item)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
            }

            item.CartId = cart.Id;
            item.CreatedAt = DateTime.UtcNow;
            _context.CartItems.Add(item);
            await _context.SaveChangesAsync();

            // Invalidate Redis cache
            await _redisService.RemoveAsync($"cart:{userId}");

            return CreatedAtAction(nameof(GetCart), new { id = item.Id }, item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to cart");
            return StatusCode(500, "An error occurred while adding the item to cart");
        }
    }

    [HttpPut("items/{id}")]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] CartItem item)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var existingItem = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.Id == id && i.Cart.UserId == userId && i.IsActive);

            if (existingItem == null)
                return NotFound();

            existingItem.Quantity = item.Quantity;
            existingItem.UnitPrice = item.UnitPrice;
            existingItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Invalidate Redis cache
            await _redisService.RemoveAsync($"cart:{userId}");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cart item");
            return StatusCode(500, "An error occurred while updating the cart item");
        }
    }

    [HttpDelete("items/{id}")]
    public async Task<IActionResult> RemoveItem(Guid id)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var item = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.Id == id && i.Cart.UserId == userId && i.IsActive);

            if (item == null)
                return NotFound();

            item.IsActive = false;
            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Invalidate Redis cache
            await _redisService.RemoveAsync($"cart:{userId}");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from cart");
            return StatusCode(500, "An error occurred while removing the item from cart");
        }
    }

    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst("userId");
        if (userIdClaim == null)
            throw new InvalidOperationException("User ID not found in token");

        return Guid.Parse(userIdClaim.Value);
    }
} 