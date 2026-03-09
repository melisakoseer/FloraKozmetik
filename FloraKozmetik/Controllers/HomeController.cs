using FloraKozmetik.Data;
using FloraKozmetik.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloraKozmetik.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var skincare = await _context.Products
                .Where(p => p.IsActive && p.Category == "skincare")
                .OrderByDescending(p => p.CreatedAt)
                .Take(2)
                .ToListAsync();

            var fragrances = await _context.Products
                .Where(p => p.IsActive && p.Category == "fragrances")
                .OrderByDescending(p => p.CreatedAt)
                .Take(3)
                .ToListAsync();

            var beauty = await _context.Products
                .Where(p => p.IsActive && p.Category == "beauty")
                .OrderByDescending(p => p.CreatedAt)
                .Take(1)
                .ToListAsync();

            var products = skincare.Concat(fragrances).Concat(beauty).ToList();
            ViewBag.Products = products;

            //anasayfadaki fýrsatlar bölümü için indirimli ürünler
            var discounted = await _context.Products
                .Where(p => p.IsActive && p.OriginalPrice != null)
                .OrderByDescending(p => (1 - p.Price / p.OriginalPrice!.Value))
                .Take(4)
                .ToListAsync();

            //slider her kategoriden ilk aktif ürünü çek
            var hero0 = await _context.Products
                .Where(p => p.IsActive && p.Category == "skincare")
                .FirstOrDefaultAsync();

            var hero1 = await _context.Products
                .Where(p => p.IsActive && p.Category == "fragrances")
                .FirstOrDefaultAsync();

            var hero2 = await _context.Products
                .Where(p => p.IsActive && p.Category == "beauty")
                .FirstOrDefaultAsync();

            var featProduct = await _context.Products//öne çýkan, sliderla ayný olmamasý için skip
            .Where(p => p.IsActive && p.Category == "skincare")
            .Skip(1)
            .FirstOrDefaultAsync();

            ViewBag.FeatProduct = featProduct;
            ViewBag.Products = products;
            ViewBag.Discounted = discounted;
            ViewBag.Hero0 = hero0;
            ViewBag.Hero1 = hero1;
            ViewBag.Hero2 = hero2;

            return View();
        }
    }
}