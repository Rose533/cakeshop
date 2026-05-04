using CakeShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CakeShop.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        // 👇 這邊的 DbContextOptions 裡面必須包著 <ApplicationDbContext>，如果漏掉或寫錯，UseSqlite 就會找不到！
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cake> Cakes { get; set; }
        public DbSet<Order> Orders { get; set; }
    }
}