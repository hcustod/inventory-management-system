using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [Required, Column("ProductName"), StringLength(150)]
        public string Name { get; set; }
        
        [Required, Column("ProductDescription")]
        public string Description { get; set; }
        
        [Required, Column("ProductPrice")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        
        [Required, Column("ProductStockAmount")]
        [Range(0, int.MaxValue)]
        public int ProductStockAmount { get; set; }
        
        [Required, Column("LowStockThreshold")]
        [Range(0, int.MaxValue)]
        public int LowStockThreshold { get; set; }
        
        
        // Category attributes
        [Column("CategoryId")]
        public int CategoryId { get; set; }
        
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
        
        
        // Update times
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    }
}

