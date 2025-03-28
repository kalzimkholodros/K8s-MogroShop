using System.ComponentModel.DataAnnotations;

namespace CartService.Models;

public class Cart
{
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation property
    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
} 