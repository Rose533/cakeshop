using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CakeShop.Models
{
    public class Cake
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}