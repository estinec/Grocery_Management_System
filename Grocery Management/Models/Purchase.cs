using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grocery_Management.Models;

public class Purchase
{
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string AssociateId { get; set; } = "Associate_Default";

    // Navigation property
    public Product? Product { get; set; }
}