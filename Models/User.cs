using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Identity;

public class User : IdentityUser
{
    [Required]
    public string FullName { get; set; }
    
    [Required]
    public string ContactInformation { get; set; }
    
    public ICollection<Order> Orders { get; set; }
}