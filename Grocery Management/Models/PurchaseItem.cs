using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grocery_Management.Models;

public class PurchaseItem
{
    public int Id { get; set; }

    [Required]
    public int PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }

    [Required]
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [NotMapped]
    public decimal Subtotal => UnitPrice * Quantity;
}