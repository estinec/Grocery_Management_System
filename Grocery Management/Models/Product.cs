using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grocery_Management.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(10)]
    public string Emoji { get; set; } = "🛒";

    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 10000.00)]
    public decimal UnitPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int CurrentStock { get; set; }

    // Navigation properties
    public Category? Category { get; set; }
    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
}