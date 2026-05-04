using CakeShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CakeShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 如果資料庫沒有蛋糕，自動塞入幾筆測試資料 (方便你直接看到畫面)
            if (!_context.Cakes.Any())
            {
                _context.Cakes.AddRange(
                    new Models.Cake { Name = "草莓千層", Price = 120, Description = "新鮮草莓搭配濃郁卡士達" },
                    new Models.Cake { Name = "重乳酪蛋糕", Price = 100, Description = "經典美式重乳酪" },
                    new Models.Cake { Name = "黑森林蛋糕", Price = 90, Description = "苦甜巧克力與櫻桃的絕妙搭配" }
                );
                await _context.SaveChangesAsync();
            }

            var cakes = await _context.Cakes.ToListAsync();
            return View(cakes);
        }
    }
}
