using FloraKozmetik.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloraKozmetik.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string category = "skincare", string sort = "",
            decimal? minPrice = null, decimal? maxPrice = null, int page = 1)
        {
            const int pageSize = 6;
            var gecerliKategoriler = new[] { "skincare", "fragrances", "beauty" };
            if (!gecerliKategoriler.Contains(category))
                category = "skincare";

            //fiyat filtresi sadece değer geldiğinde ekleniyor
            var query = _context.Products.Where(p => p.IsActive && p.Category == category);

            if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);

            query = sort switch
            {
                "price-asc" => query.OrderBy(p => p.Price),
                "price-desc" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };//hiçbiri eşleşmezse yeni ürünler önce

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);//kategoride 6 üründen fazla ürün varsa sayfalama yap
            var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();//sayfa 2 ise ilk 6 ürünü atla

            ViewBag.Category = category;
            ViewBag.Sort = sort;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Products = products;
            ViewBag.Count = totalCount;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            ViewBag.Label = category switch
            {
                "skincare" => "Yüz Bakımı",
                "fragrances" => "Parfümler",
                "beauty" => "Makyaj",
                _ => "Ürünler"
            };
            ViewBag.HeroDesc = category switch
            {
                "skincare" => "Bitkisel özlerden elde edilen serum, nemlendirici ve göz bakım ürünleriyle cildinizi sabah akşam besleyin.",
                "fragrances" => "Doğadan ilham alan özgün kokular. Her biri bir his, her biri bir hikaye.",
                "beauty" => "Ruj, maskara, göz farı ve fondöten — doğal pigmentlerle üretilmiş kalıcı makyaj koleksiyonu.",
                _ => ""
            };
            ViewBag.HeroBg = category switch
            {
                "skincare" => "slide-bg-0",
                "fragrances" => "slide-bg-1",
                "beauty" => "slide-bg-2",
                _ => "slide-bg-0"
            };

            var heroImg = category switch
            {
                "beauty" => await _context.Products
                    .Where(p => p.IsActive && p.Name.Contains("Göz Farı"))
                    .Select(p => p.ImageUrl).FirstOrDefaultAsync(),
                "fragrances" => await _context.Products
                    .Where(p => p.IsActive && p.Name.Contains("Coco"))
                    .Select(p => p.ImageUrl).FirstOrDefaultAsync(),
                _ => await _context.Products
                    .Where(p => p.IsActive && p.Category == category)
                    .Select(p => p.ImageUrl).FirstOrDefaultAsync()
            };
            ViewBag.HeroImg = heroImg;
            return View();
        }
        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0)
                return RedirectToAction("Index");

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
                return NotFound();
            //pasife alınmış ürünün detay sayfası açılmaz

            var related = await _context.Products
                .Where(p => p.Category == product.Category && p.Id != id && p.IsActive) // görüntülenen ürünü ilgili ürünlere dahil etme
                .Take(4)//aynı kategoriden 4 ürün al
                .ToListAsync();

            ViewBag.Product = product;
            ViewBag.Related = related;

            return View();
        }
    }
}
