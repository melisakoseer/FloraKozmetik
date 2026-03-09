using FloraKozmetik.Data;
using FloraKozmetik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloraKozmetik.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //Admin
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalProducts  = await _context.Products.CountAsync();
            ViewBag.TotalOrders    = await _context.Orders.CountAsync();
            ViewBag.TotalUsers     = await _userManager.Users.CountAsync();
            //iptal edilmiş siparişler için toplam gelir

            ViewBag.TotalRevenue = await _context.Orders
                .Where(o => o.Status != "İptal")
                .SumAsync(o => o.TotalAmount);

            ViewBag.PendingOrders  = await _context.Orders.CountAsync(o => o.Status == "Bekliyor");
            ViewBag.RecentOrders   = await _context.Orders
                .Include(o => o.User)//müşteri adı
                .Include(o => o.OrderItems)//ürün sayısı
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)//dashboard son 5 sipariş gösterimi
                .ToListAsync();

            ViewBag.LowStockProducts = await _context.Products//dashboardaki stoğu 10 altındaki aktif ürünler
                .Where(p => p.Stock < 10 && p.IsActive)
                .Take(5)
                .ToListAsync();
            return View();
        }

        //ÜRÜNLER
        public async Task<IActionResult> Products(string? category, string? search)
        {
            var query = _context.Products.AsQueryable();//sorgu hemen çalışmaz filreleri eklendikçe biriktirir sonra tek sorguda db ye gönderir
            if (!string.IsNullOrEmpty(category)) query = query.Where(p => p.Category == category);
            if (!string.IsNullOrEmpty(search))   query = query.Where(p => p.Name.Contains(search));

            ViewBag.Products = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            ViewBag.Category = category;
            ViewBag.Search   = search;
            return View();
        }

        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.Brands = await _context.Products
                .Where(p => p.Brand != null)
                .Select(p => p.Brand)
                .Distinct()//tekrar eden markaları ele
                .OrderBy(b => b)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateProduct(
        string name, string description, decimal price, decimal? originalPrice,
        string category, string brand, int stock, IFormFile? imageFile)
            {
            string imageUrl = "";
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsDir = Path.Combine("wwwroot", "uploads");
                Directory.CreateDirectory(uploadsDir);
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName); //rastgele unique dosya adı üretir,uzantı korunur
                var filePath = Path.Combine(uploadsDir, fileName);
                using var stream = System.IO.File.Create(filePath);
                await imageFile.CopyToAsync(stream);
                imageUrl = "/uploads/" + fileName;
            }

            var product = new Product
            {
                Name = name,
                Description = description,
                Price = price,
                OriginalPrice = originalPrice,
                Category = category,
                Brand = brand,
                Stock = stock,
                ImageUrl = imageUrl,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("Products");
        }

        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            ViewBag.Product = product;
            ViewBag.Brands = await _context.Products
                .Where(p => p.Brand != null)
                .Select(p => p.Brand)
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> EditProduct(
            int id,
            string name,
            string description,
            decimal price,
            int discountRate,
            string category,
            string brand,
            int stock,
            string imageUrl,
            bool isActive,
            IFormFile? imageFile)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Name = name;
            product.Description = description;

            if (discountRate > 0)
            {
                product.OriginalPrice = price;

                product.Price = price - (price * discountRate / 100m);
            }
            else
            {
                product.Price = price;//düşük fiyat
                product.OriginalPrice = null;
            }

            product.Category = category;
            product.Brand = brand;
            product.Stock = stock;
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsDir = Path.Combine("wwwroot", "uploads");
                Directory.CreateDirectory(uploadsDir);
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsDir, fileName);
                using var stream = System.IO.File.Create(filePath);
                await imageFile.CopyToAsync(stream);
                product.ImageUrl = "/uploads/" + fileName;
            }
            else
            {
                product.ImageUrl = imageUrl;
            }
            product.IsActive = isActive;
            await _context.SaveChangesAsync();
            return RedirectToAction("Products");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)//pasife almak için
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsActive = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Products");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> HardDeleteProduct(int id)//silmek için
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Products");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RemoveDiscount(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.Price = product.OriginalPrice ?? product.Price; //OriginalPrice null ise mevcut Price korunuyor. İndirim kaldırılınca fiyat orijinal fiyata dönüyor.
                product.OriginalPrice = null;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Products");
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateStock(int id, int stock)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.Stock = stock;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Products");
        }

        //SİPARİŞLER
        public async Task<IActionResult> Orders(string? status)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .AsQueryable();
            if (!string.IsNullOrEmpty(status)) query = query.Where(o => o.Status == status);
            ViewBag.Orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
            ViewBag.Status = status;
            return View();
        }
        public async Task<IActionResult> OrderDetail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            ViewBag.Order = order;
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status, string? returnTo = null)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
            }

            if (returnTo == "detail")//nereden çağrıldığını anlıyor (sipariş listesi/detay sayfasından)Detaydan güncellenince tekrar detaya dönüyor.
                return RedirectToAction("OrderDetail", "Admin", new { id = orderId });

            return RedirectToAction("Orders");
        }

        //KULLANICILAR
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
            ViewBag.Users = users;
            return View();
        }
    }
}
