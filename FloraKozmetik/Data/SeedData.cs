using FloraKozmetik.Models;
using Microsoft.AspNetCore.Identity;

namespace FloraKozmetik.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            context.Database.EnsureCreated();

            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            if (await userManager.FindByEmailAsync("admin@flora.com") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@flora.com",
                    Email = "admin@flora.com",
                    FirstName = "Flora",
                    LastName = "Admin",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                };
                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            if (context.Products.Any()) return;

            var products = new List<Product>
            {
                // ── YÜZ BAKIMI (skincare) ── id: 118, 119, 120 (3 ürün, 6'ya tamamlamak için farklı isimlerle)
                new Product {
                    Name = "Bitkisel El Sabunu",
                    Description = "Süper yaprakların iyiliğiyle zenginleştirilmiş doğal ve besleyici el sabunu. Ellerinizi temizler ve nemlendirir.",
                    Price = 189, OriginalPrice = 249,
                    Category = "skincare", Brand = "Flora", Stock = 94,
                    ImageUrl = "https://cdn.dummyjson.com/product-images/skin-care/attitude-super-leaves-hand-soap/thumbnail.webp",
                    IsActive = true
                },
                new Product {
                    Name = "Shea Butter Vücut Losyonu",
                    Description = "Shea butter'ın nemlendirici gücüyle cildinizi besleyen lüks vücut losyonu. Zengin köpük ve ipeksi yumuşak cilt.",
                    Price = 299,
                    Category = "skincare", Brand = "Botanica", Stock = 34,
                    ImageUrl = "https://cdn.dummyjson.com/product-images/skin-care/olay-ultra-moisture-shea-butter-body-wash/thumbnail.webp",
                    IsActive = true
                },
                new Product {
                    Name = "Erkek Yüz ve Vücut Losyonu",
                    Description = "Uzun süreli nem sağlayan, hızlı emen erkek cilt bakım losyonu. Cildi nemli ve sağlıklı tutar.",
                    Price = 249, OriginalPrice = 329,
                    Category = "skincare", Brand = "Purist", Stock = 95,
                    ImageUrl = "https://cdn.dummyjson.com/product-images/skin-care/vaseline-men-body-and-face-lotion/thumbnail.webp",
                    IsActive = true
                },
                new Product {
                    Name = "Arındırıcı Yüz Temizleyici",
                    Description = "Gözenekleri derinlemesine temizleyen, cilde tazelik katan günlük yüz temizleyici jel.",
                    Price = 219,
                    Category = "skincare", Brand = "Verdant", Stock = 60,
                    ImageUrl = "https://cdn.dummyjson.com/product-images/skin-care/attitude-super-leaves-hand-soap/thumbnail.webp",
                    IsActive = true
                },
                new Product {
                    Name = "Canlandırıcı Duş Jeli",
                    Description = "Ferahlatıcı ve arındırıcı duş jeli. Cildinizi temizlerken nemini korur.",
                    Price = 179, OriginalPrice = 229,
                    Category = "skincare", Brand = "Lumière", Stock = 50,
                    ImageUrl = "https://cdn.dummyjson.com/product-images/skin-care/olay-ultra-moisture-shea-butter-body-wash/thumbnail.webp",
                    IsActive = true
                },
                new Product {
                    Name = "Günlük Nemlendirici Krem",
                    Description = "Her gün kullanıma uygun hafif nemlendirici krem. Cilt tonunu eşitler ve parlaklık verir.",
                    Price = 279,
                    Category = "skincare", Brand = "Elixir", Stock = 45,
                    ImageUrl = "https://cdn.dummyjson.com/product-images/skin-care/vaseline-men-body-and-face-lotion/thumbnail.webp",
                    IsActive = true
                },

                // ── PARFÜM (fragrances) ── id: 6, 7, 8, 9, 10 + 6 tekrar
                new Product {
                    Name = "CK One Unisex EDT 50ml",
                    Description = "Calvin Klein'ın klasik unisex parfümü. Taze ve temiz kokusuyla her gün kullanıma uygun.",
                    Price = 649,
                    Category = "fragrances", Brand = "Flora", Stock = 29,
                    ImageUrl = "https://cdn.dummyjson.com/product-images/fragrances/calvin-klein-ck-one/thumbnail.webp",
                    IsActive = true
                },
                new Product {
                    Name = "Coco Noir EDP 100ml",
                    Description = "Greyfurt, gül ve sandal ağacı notalarıyla zarif ve gizemli bir parfüm. Akşam davetleri için ideal.",
                    Price = 1299, OriginalPrice = 1599,
                    Category = "fragrances", Brand = "Elixir", Stock = 58,
                    ImageUrl = "https://cdn.dummyjson.com/product-images/fragrances/chanel-coco-noir-eau-de/thumbnail.webp",
                    IsActive = true
                },
                new Product {
                    Name = "J'adore EDP 50ml",
                    Description = "Ylang-ylang, gül ve yasemin notalarıyla lüks ve çiçeksi parfüm. Feminenliği ve zarafeti simgeler.",
                    Price = 899,
                    Category = "fragrances", Brand = "Lumière", Stock = 98,
                    ImageUrl = "https://cdn.dummyjson.com/product-images/fragrances/dior-j'adore/thumbnail.webp",
                    IsActive = true
                },
                new Product {
                    Name = "Dolce Shine EDP 30ml",
                    Description = "Mango, yasemin ve sarı ahşap notalarıyla canlı ve meyveli bir koku. Neşeli ve genç bir parfüm.",
                    Price = 699, OriginalPrice = 849,
                    Category = "fragrances", Brand = "Botanica", Stock = 4,
                    ImageUrl = "https://cdn.dummyjson.com/product-images/fragrances/dolce-shine-eau-de/thumbnail.webp",
                    IsActive = true
                },
                new Product {
                    Name = "Gucci Bloom EDP 50ml",
                    Description = "Tuberose, yasemin ve rangoon sarmaşığı notalarıyla çiçeksi ve büyüleyici parfüm.",
                    Price = 799,
                    Category = "fragrances", Brand = "Verdant", Stock = 91,
                    ImageUrl = "https://cdn.dummyjson.com/product-images/fragrances/gucci-bloom-eau-de/thumbnail.webp",
                    IsActive = true
                },
                new Product {
                    Name = "Taze Sabah EDT 100ml",
                    Description = "Bergamot ve limon notalarıyla başlayan ferah ve enerjik unisex koku. Her gün kullanıma uygun.",
                    Price = 549,
                    Category = "fragrances", Brand = "Purist", Stock = 35,
                    ImageUrl = "https://cdn.dummyjson.com/product-images/fragrances/calvin-klein-ck-one/thumbnail.webp",
                    IsActive = true
                },

                // ── MAKYAJ (beauty) ── id: 1, 2, 3, 4, 5 + tekrar 1
                new Product {
                    Name = "Hacim Veren Maskara",
                    Description = "Kirpiklere anında hacim ve uzunluk katan maskara. Dökülmeye ve suya karşı dayanıklı formül.",
                    Price = 199,
                    Category = "beauty", Brand = "Flora", Stock = 99,
                    ImageUrl = "https://cdn.dummyjson.com/product-images/beauty/essence-mascara-lash-princess/thumbnail.webp",
                    IsActive = true
                },
                new Product {
                    Name = "Aynalı 12li Göz Farı Paleti",
                    Description = "Mat ve simli tonların bir arada olduğu çok yönlü palet. Her makyaj stili için ideal.",
                    Price = 329,
                    Category = "beauty", Brand = "Elixir", Stock = 34,
                    ImageUrl = "https://cdn.dummyjson.com/product-images/beauty/eyeshadow-palette-with-mirror/thumbnail.webp",
                    IsActive = true
                },
                new Product {
                    Name = "Işıltılı Pudra",
                    Description = "İnce öğütülmüş, hafif pudra. Makyajı sabitler ve mat görünüm sağlar.",
                    Price = 249,
                    Category = "beauty", Brand = "Botanica", Stock = 89,
                    ImageUrl = "https://cdn.dummyjson.com/product-images/beauty/powder-canister/thumbnail.webp",
                    IsActive = true
                },
                new Product {
                    Name = "Kadife Kırmızı Ruj",
                    Description = "Yoğun pigmentli, uzun süre kalıcı mat ruj. Dudakları nemlendiren E vitamini içerir.",
                    Price = 149,
                    Category = "beauty", Brand = "Purist", Stock = 91,
                    ImageUrl = "https://cdn.dummyjson.com/product-images/beauty/red-lipstick/thumbnail.webp",
                    IsActive = true
                },
                new Product {
                    Name = "Kırmızı Oje",
                    Description = "Hızlı kuruyan, çatlak önleyici formüllü kalıcı oje. 2 kat uygulamayla mükemmel sonuç.",
                    Price = 89,
                    Category = "beauty", Brand = "Verdant", Stock = 79,
                    ImageUrl = "https://cdn.dummyjson.com/product-images/beauty/red-nail-polish/thumbnail.webp",
                    IsActive = true
                },
                new Product {
                    Name = "Uzun Kalıcı Maskara",
                    Description = "12 saat kalıcı, bükülen fırçasıyla her kirpiğe ulaşan maskara. Gün boyu hacimli kirpikler.",
                    Price = 179, OriginalPrice = 219,
                    Category = "beauty", Brand = "Lumière", Stock = 65,
                    ImageUrl = "https://cdn.dummyjson.com/product-images/beauty/essence-mascara-lash-princess/thumbnail.webp",
                    IsActive = true
                },
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }
    }
}