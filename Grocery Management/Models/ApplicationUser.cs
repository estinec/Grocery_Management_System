using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Grocery_Management.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}