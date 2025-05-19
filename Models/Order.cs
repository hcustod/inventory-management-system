using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Column("OrderDate")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required, Column("OrderTotalPrice")]
        [Range(0, double.MaxValue)]
        public decimal TotalPrice { get; set; }

        [Required, Column("OrderUsername"), StringLength(100)]
        public string userName { get; set; }

        [Required, Column("OrderEmail"), EmailAddress, StringLength(150)]
        public string userEmail { get; set; }

        public ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    }
}