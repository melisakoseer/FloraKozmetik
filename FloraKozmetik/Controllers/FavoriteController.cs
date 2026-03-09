using FloraKozmetik.Data;
using FloraKozmetik.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloraKozmetik.Controllers
{
    public class FavoriteController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavoriteController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Favori sayısını döndür
        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            if (!User.Identity!.IsAuthenticated)
                return Json(new { count = 0 });

            var userId = _userManager.GetUserId(User);
            var count = await _context.Favorites
                .Where(f => f.UserId == userId)
                .CountAsync();

            return Json(new { count });
        }

        // Favori listesini döndür
        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            if (!User.Identity!.IsAuthenticated)
                return Json(new { items = new List<object>() });

            var userId = _userManager.GetUserId(User);
            var items = await _context.Favorites
                .Include(f => f.Product)
                .Where(f => f.UserId == userId)
                .ToListAsync();

            var result = items.Select(f => new
            {
                id        = f.Id,
                productId = f.ProductId,
                name      = f.Product!.Name,
                price     = f.Product.Price,
                image     = f.Product.ImageUrl,
                brand     = f.Product.Brand
            });

            return Json(new { items = result });
        }

        // Favori ekle/kaldır (toggle)
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Toggle(int productId)
        {
            if (!User.Identity!.IsAuthenticated)
                return Json(new { success = false, message = "Favorilere eklemek için giriş yapmalısınız.", requireLogin = true });

            var userId = _userManager.GetUserId(User);
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
                return Json(new { success = false, message = "Ürün bulunamadı." });

            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            bool isFavorited;

            if (existing != null)
            {
                _context.Favorites.Remove(existing);
                isFavorited = false;
            }
            else
            {
                _context.Favorites.Add(new Favorite
                {
                    UserId    = userId!,
                    ProductId = productId
                });
                isFavorited = true;
            }

            await _context.SaveChangesAsync();

            var count = await _context.Favorites
                .Where(f => f.UserId == userId)
                .CountAsync();

            var message = isFavorited
                ? $"{product.Name} favorilere eklendi."
                : $"{product.Name} favorilerden kaldırıldı.";

            return Json(new { success = true, isFavorited, message, count });
        }

        // Kullanıcının favori ID listesini döndür (sayfa yüklenince butonları renklendirmek için)
        [HttpGet]
        public async Task<IActionResult> GetFavoriteIds()
        {
            if (!User.Identity!.IsAuthenticated)
                return Json(new { ids = new List<int>() });

            var userId = _userManager.GetUserId(User);
            var ids = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.ProductId)
                .ToListAsync();

            return Json(new { ids });
        }

        // Favorilerim sayfası
        public async Task<IActionResult> Index()
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userId = _userManager.GetUserId(User);
            var favorites = await _context.Favorites
                .Include(f => f.Product)
                .Where(f => f.UserId == userId)
                .ToListAsync();

            ViewBag.Favorites = favorites;
            return View();
        }
    }
}
