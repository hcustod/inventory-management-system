using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        
        [Required, Column("CategoryName"), StringLength(100)]
        public string CategoryName { get; set; }
        
        [Column("CategoryDescription")]
        public string CategoryDescription { get; set; }
        
        public ICollection<Product> CategoryProducts { get; set; } = new List<Product>();
    }
}



