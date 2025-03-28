using System.ComponentModel.DataAnnotations;

namespace CartService.Models;

public class CartItem
{
    public Guid Id { get; set; }

    [Required]
    public Guid CartId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation property
    public virtual Cart Cart { get; set; } = null!;
} 