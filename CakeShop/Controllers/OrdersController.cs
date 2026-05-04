using CakeShop.Data;
using CakeShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CakeShop.Controllers
{
    [Authorize] // 確保只有登入的會員可以存取
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public OrdersController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // 新增訂單
        [HttpPost]
        public async Task<IActionResult> Create(int cakeId, int quantity)
        {
            var cake = await _context.Cakes.FindAsync(cakeId);
            if (cake == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            var order = new Order
            {
                UserId = user.Id,
                CakeId = cakeId,
                Quantity = quantity,
                TotalPrice = cake.Price * quantity
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 發送訂單確認信
            string subject = "您的蛋糕訂單已確認！";
            string message = $"親愛的顧客您好，<br>您已成功訂購 {quantity} 份 {cake.Name}，總價為 {order.TotalPrice} 元。<br>感謝您的購買！";
            await _emailSender.SendEmailAsync(user.Email, subject, message);

            return RedirectToAction("Index"); // 假設有一個 Index 顯示訂單列表
        }

        // 刪除訂單
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            // 尋找訂單，並確保該訂單屬於當前登入的使用者
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}