using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CakeShop.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public IdentityUser User { get; set; } = null!;

        public int CakeId { get; set; }
        public Cake Cake { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
    }
}