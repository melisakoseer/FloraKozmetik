using FloraKozmetik.Data;
using FloraKozmetik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloraKozmetik.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Sepet sayısını döndür (navbar badge için)
        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            if (!User.Identity!.IsAuthenticated)
                return Json(new { count = 0 });

            var userId = _userManager.GetUserId(User);
            var count = await _context.CartItems
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity);//her ürünün adedini toplayıp tek sayı döndürüyor

            return Json(new { count });
        }

        // Sepet içeriğini döndür (drawer için)
        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            if (!User.Identity!.IsAuthenticated)
                return Json(new { items = new List<object>(), total = 0 });

            var userId = _userManager.GetUserId(User);
            var items = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var result = items.Select(c => new //nonim nesne listesi oluşturuyor, sadece lazım olan alanları JSON'a gönderiyor. 
            {
                id        = c.Id,
                productId = c.ProductId,
                name      = c.Product!.Name,
                price     = c.Product.Price,
                image     = c.Product.ImageUrl,
                quantity  = c.Quantity,
                subtotal  = c.Product.Price * c.Quantity//her satır için adet x fiyat
            });

            var total = items.Sum(c => c.Product!.Price * c.Quantity);

            return Json(new { items = result, total });
        }

        // Sepete ekle
        [HttpPost]
        public async Task<IActionResult> Add(int productId)
        {
            if (!User.Identity!.IsAuthenticated)
                return Json(new { success = false, message = "Sepete eklemek için giriş yapmalısınız.", requireLogin = true });

            var userId = _userManager.GetUserId(User);
            var product = await _context.Products.FindAsync(productId);

            if (product == null || !product.IsActive)
                return Json(new { success = false, message = "Ürün bulunamadı." });

            var existing = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (existing != null)
            {
                existing.Quantity++;//Aynı ürün tekrar eklenirse mevcut kaydın miktarını arttır.
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    UserId    = userId!,
                    ProductId = productId,
                    Quantity  = 1
                });
            }

            await _context.SaveChangesAsync();

            var count = await _context.CartItems
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity);//güncel sayı

            return Json(new { success = true, message = $"{product.Name} sepete eklendi.", count });
        }

        // Adet güncelle
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            if (!User.Identity!.IsAuthenticated)
                return Json(new { success = false });

            var userId = _userManager.GetUserId(User);
            var item = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);//kullanıcının sepetine ulaşmak için

            if (item == null)
                return Json(new { success = false });

            //− butonuna basılıp 0'a düşünce ürün sepetten kaldırılıyor.
            if (quantity <= 0)
            {
                _context.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }

            await _context.SaveChangesAsync();

            var count = await _context.CartItems
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity);

            var total = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Product!.Price * c.Quantity);

            return Json(new { success = true, count, total });
        }

        // Sepetten kaldır
        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            if (!User.Identity!.IsAuthenticated)
                return Json(new { success = false });

            var userId = _userManager.GetUserId(User);
            var item = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            var count = await _context.CartItems
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity);

            var total = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Product!.Price * c.Quantity);

            return Json(new { success = true, count, total });
        }

        // Sepeti temizle
        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            if (!User.Identity!.IsAuthenticated)
                return Json(new { success = false });

            var userId = _userManager.GetUserId(User);
            var items = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(items);//toplu silme
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
