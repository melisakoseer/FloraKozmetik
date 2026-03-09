using FloraKozmetik.Data;
using FloraKozmetik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloraKozmetik.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Order/Checkout
        public async Task<IActionResult> Checkout()
        { 
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            //sepet boşsa ödemeye geçme
            if (!cartItems.Any())
                return RedirectToAction("Index", "Home");

            var addresses = await _context.Addresses
                .Where(a => a.UserId == user.Id)
                .ToListAsync();

            ViewBag.CartItems = cartItems;
            ViewBag.Addresses = addresses;
            ViewBag.User = user;

            return View();
        }

        // POST: /Order/PlaceOrder
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PlaceOrder(
            [FromForm] int? addressId,
            [FromForm] string? newAddressTitle,
            [FromForm] string? newAddressFullName,
            [FromForm] string? newAddressPhone,
            [FromForm] string? newAddressCity,
            [FromForm] string? newAddressDistrict,
            [FromForm] string? newAddressFullAddress,
            [FromForm] string? couponCode,
            [FromForm] string? paymentMethod)
        {
            //parametreler form body'den alınıyor. checkout.js'te FormData ile gönderiliyor.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
                return Json(new { success = false, message = "Sepetiniz boş." });

            // Adres belirle
            string shippingAddress = "";
            if (addressId.HasValue && addressId > 0)
            {
                var addr = await _context.Addresses.FindAsync(addressId.Value);
                if (addr != null)
                    shippingAddress = $"{addr.FullName}, {addr.FullAddress}, {addr.District}/{addr.City} - {addr.Phone}";
            }
            else if (!string.IsNullOrEmpty(newAddressFullAddress))
            {
                shippingAddress = $"{newAddressFullName}, {newAddressFullAddress}, {newAddressDistrict}/{newAddressCity} - {newAddressPhone}";

                // Yeni adresi kaydet
                if (!string.IsNullOrEmpty(newAddressTitle))
                {
                    var newAddr = new Address
                    {
                        UserId = user.Id,
                        Title = newAddressTitle,
                        FullName = newAddressFullName ?? "",
                        Phone = newAddressPhone ?? "",
                        City = newAddressCity ?? "",
                        District = newAddressDistrict ?? "",
                        FullAddress = newAddressFullAddress,
                        IsDefault = false
                    };
                    _context.Addresses.Add(newAddr);
                }
            }

            if (string.IsNullOrEmpty(shippingAddress))
                return Json(new { success = false, message = "Lütfen bir teslimat adresi seçin." });

            // Tutar hesapla
            decimal subtotal = cartItems.Sum(c => c.Product!.Price * c.Quantity);
            decimal discount = 0;
            string appliedCoupon = "";

            if (couponCode?.ToUpper() == "FLORA15")
            {
                var alreadyUsed = await _context.Orders
                 .AnyAsync(o => o.UserId == user.Id && o.CouponCode == "FLORA15" && o.Status != "İptal");//kupon kullanılmış mı kontrolu

                if (alreadyUsed)
                    return Json(new { success = false, message = "Bu kuponu daha önce kullandınız." });

                discount = Math.Round(subtotal * 0.15m, 2);
                appliedCoupon = "FLORA15";
            }

            decimal total = subtotal - discount;

            // Sipariş oluştur
            var order = new Order
            {
                UserId = user.Id,
                ShippingAddress = shippingAddress,
                TotalAmount = total,
                DiscountAmount = discount,
                CouponCode = appliedCoupon,
                Status = "Bekliyor",
                PaymentMethod = paymentMethod ?? "Kredi Kartı",
                CreatedAt = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Sipariş kalemleri
            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product!.Name,
                    ProductImage = item.Product.ImageUrl,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                };
                _context.OrderItems.Add(orderItem);
            }

            // Sepeti temizle
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return Json(new { success = true, orderId = order.Id });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CheckCoupon(string couponCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { valid = false, message = "Giriş yapmanız gerekiyor." });

            if (couponCode?.ToUpper() == "FLORA15")
            {
                var alreadyUsed = await _context.Orders
                    .AnyAsync(o => o.UserId == user.Id && o.CouponCode == "FLORA15" && o.Status != "İptal");

                if (alreadyUsed)
                    return Json(new { valid = false, message = "Bu kuponu daha önce kullandınız." });

                return Json(new { valid = true, discountRate = 0.15 });
            }

            return Json(new { valid = false, message = "Geçersiz kupon kodu." });
        }
        // Order/Success
        public async Task<IActionResult> Success(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user!.Id);

            if (order == null) return RedirectToAction("Index", "Home");

            ViewBag.Order = order;
            return View();
        }
        //Order/Detail/
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user!.Id);

            if (order == null) return RedirectToAction("Index", "Account");

            ViewBag.Order = order;
            return View();
        }

        //Order/Cancel
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user!.Id);

            if (order == null)
                return Json(new { success = false, message = "Sipariş bulunamadı." });

            if (order.Status != "Bekliyor")
                return Json(new { success = false, message = "Sadece 'Bekliyor' durumundaki siparişler iptal edilebilir." });

            order.Status = "İptal";
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
