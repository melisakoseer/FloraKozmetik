using FloraKozmetik.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloraKozmetik.Controllers
{
    public class DealsController : Controller
    {
        private readonly AppDbContext _context;

        public DealsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var discounted = await _context.Products
                .Where(p => p.IsActive && p.OriginalPrice != null)//aktif ve fiyat null olmayan
                .OrderByDescending(p => (1 - p.Price / p.OriginalPrice!.Value))//yüksek indirim baþa gelir
                .ToListAsync();

            ViewBag.Discounted = discounted;
            return View();
        }
    }
}
