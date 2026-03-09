using FloraKozmetik.Data;
using FloraKozmetik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloraKozmetik.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AppDbContext context)
        {
            _userManager = userManager; // kullanıcı oluşturma, güncelleme, şifre işlemleri
            _signInManager = signInManager; //giriş/çıkış işlemleri
            _context = context;//DB sorguları 
        }

        //PROFİL
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == user.Id) //kullanıcının siparişleri
                .OrderByDescending(o => o.CreatedAt)//son siparişe göre sırala
                .ToListAsync();

            var addresses = await _context.Addresses
                .Where(a => a.UserId == user.Id)
                .ToListAsync();

            ViewBag.User      = user;
            ViewBag.Orders    = orders;
            ViewBag.Addresses = addresses;
            return View();
        }

        //Kişisel bilgileri güncelle
        [Authorize]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateProfile(string firstName, string lastName, string phoneNumber, string gender)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false });

            user.FirstName   = firstName;
            user.LastName    = lastName;
            user.PhoneNumber = phoneNumber;
            user.Gender      = gender;

            await _userManager.UpdateAsync(user); // identity update metodu, güvenlik stamp gibi ekstra işlemler 
            return Json(new { success = true, message = "Bilgileriniz güncellendi." });
        }

        //Adres ekle
        [Authorize]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddAddress(string title, string fullName, string phone, string city, string district, string fullAddress, bool isDefault)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false });

            if (isDefault)
            {
                var existing = await _context.Addresses
                    .Where(a => a.UserId == user.Id)
                    .ToListAsync();
                existing
                    .ForEach(a => a.IsDefault = false);//Yeni adres varsayılan yapılacaksa önce mevcut tüm adreslerin IsDefault'unu false yapıyor
            }

            _context.Addresses.Add(new Address
            {
                UserId      = user.Id,
                Title       = title,
                FullName    = fullName,
                Phone       = phone,
                City        = city,
                District    = district,
                FullAddress = fullAddress,
                IsDefault   = isDefault
            });

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Adres eklendi." });
        }

        //Adres sil
        [Authorize]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false });

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (address == null) return Json(new { success = false });

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // KAYIT / GİRİŞ / ÇIKIŞ 
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            DateTime? birthDate = null;
            try { birthDate = new DateTime(model.BirthYear, model.BirthMonth, model.BirthDay); }
            catch
            {
                ModelState.AddModelError("", "Geçersiz doğum tarihi.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName    = model.Email,
                Email       = model.Email,
                PhoneNumber = model.PhoneNumber,
                FirstName   = model.FirstName,
                LastName    = model.LastName,
                BirthDate   = birthDate,
                Gender      = model.Gender,
                CreatedAt   = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, model.Password); // şifreyi hashleyip kaydet
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false); // kayıt olunca oto giriş, tarayıcı kapanınca oturmu sonlandır
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)

                ModelState.AddModelError("", error.Description);// kayıt sırasında hata olursa identitynin hata mesajlarını modalstate'e ekleyip göster

            return View(model);
        }

        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);//4. parametre lockoutOnFailure fazla hatalı giriş olursa true'ya döner hesabı kitler
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (await _userManager.IsInRoleAsync(user!, "Admin"))
                    return RedirectToAction("Index", "Admin");

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string newPassword2)
        {
            if (newPassword != newPassword2)
                return Json(new { success = false, message = "Yeni şifreler eşleşmiyor." });

            if (newPassword.Length < 6)
                return Json(new { success = false, message = "Şifre en az 6 karakter olmalı." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (result.Succeeded)
                return Json(new { success = true, message = "Şifreniz güncellendi." });

            return Json(new { success = false, message = "Mevcut şifre hatalı." });
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ForgotPassword(string email, string newPassword, string newPassword2)
        {
            if (newPassword != newPassword2)
                return Json(new { success = false, message = "Şifreler eşleşmiyor." });

            if (newPassword.Length < 6)
                return Json(new { success = false, message = "Şifre en az 6 karakter olmalı." });

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Json(new { success = false, message = "Bu e-posta ile kayıtlı hesap bulunamadı." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);//identity'nin şifre sıfırlama tokenı üretiyor normalde maile gönderilir.
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
                return Json(new { success = true, message = "Şifreniz güncellendi. Giriş yapabilirsiniz." });

            return Json(new { success = false, message = "Şifre güncellenemedi. Şifre en az 1 büyük harf ve 1 rakam içermeli." });
        }


        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false });

            // İlişkili verileri sil
            var cartItems = await _context.CartItems.Where(c => c.UserId == user.Id).ToListAsync();
            var favorites = await _context.Favorites.Where(f => f.UserId == user.Id).ToListAsync();
            var addresses = await _context.Addresses.Where(a => a.UserId == user.Id).ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            _context.Favorites.RemoveRange(favorites);
            _context.Addresses.RemoveRange(addresses);
            await _context.SaveChangesAsync();

            await _signInManager.SignOutAsync();
            await _userManager.DeleteAsync(user);
            return Json(new { success = true });
        }
    }
}
