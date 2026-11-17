using System.ComponentModel.DataAnnotations.Schema;

namespace Grocery_Management.Models;

public class Purchase
{
    public int Id { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string AssociateId { get; set; } = "Associate_Default";

    // Navigation property: One receipt contains many items
    public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
}