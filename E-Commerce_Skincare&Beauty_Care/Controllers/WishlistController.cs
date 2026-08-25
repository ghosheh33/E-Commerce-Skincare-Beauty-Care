using E_Commerce_Skincare_Beauty_Care.Data;
using E_Commerce_Skincare_Beauty_Care.Extensions;
using E_Commerce_Skincare_Beauty_Care.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace E_Commerce_Skincare_Beauty_Care.Controllers
{
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string WISHLIST_SESSION_KEY = "WishlistSession";

        public WishlistController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. عرض صفحة المفضلة (من الداتابيز إذا مسجل دخول، ومن السيشن إذا زائر)
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                // مسجل دخول -> جلب من الداتابيز
                var wishlistItems = await _context.Wishlists
                    .Include(w => w.Product)
                    .ThenInclude(p => p.Images)
                    .Where(w => w.UserId == userId)
                    .ToListAsync();

                return View(wishlistItems);
            }
            else
            {
                // زائر -> جلب من السيشن
                var sessionWishlist = HttpContext.Session.GetObjectFromJson<List<WishlistSessionItem>>(WISHLIST_SESSION_KEY)
                                     ?? new List<WishlistSessionItem>();

                var productIds = sessionWishlist.Select(x => x.ProductId).ToList();

                var products = await _context.Products
                    .Include(p => p.Images)
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                // تحويل المنتجات لنفس نموذج Wishlist ليتطابق مع الـ View بدون أخطاء
                var wishlistViewModel = products.Select(p => new Wishlist
                {
                    ProductId = p.Id,
                    Product = p,
                    UserId = string.Empty
                }).ToList();

                return View(wishlistViewModel);
            }
        }

        // 2. إضافة أو إزالة المنتج من المفضلة (Toggle) عبر AJAX
        [HttpPost]
        public async Task<IActionResult> ToggleWishlist(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdded;

            if (!string.IsNullOrEmpty(userId))
            {
                // --- معالجة الداتابيز (للمستخدم المسجل) ---
                var existingItem = await _context.Wishlists
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

                if (existingItem != null)
                {
                    _context.Wishlists.Remove(existingItem);
                    isAdded = false;
                }
                else
                {
                    _context.Wishlists.Add(new Wishlist
                    {
                        UserId = userId,
                        ProductId = productId
                    });
                    isAdded = true;
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                // --- معالجة السيشن (للزائر) ---
                var sessionWishlist = HttpContext.Session.GetObjectFromJson<List<WishlistSessionItem>>(WISHLIST_SESSION_KEY)
                                     ?? new List<WishlistSessionItem>();

                var existingItem = sessionWishlist.FirstOrDefault(x => x.ProductId == productId);

                if (existingItem != null)
                {
                    sessionWishlist.Remove(existingItem);
                    isAdded = false;
                }
                else
                {
                    sessionWishlist.Add(new WishlistSessionItem { ProductId = productId });
                    isAdded = true;
                }

                HttpContext.Session.SetObjectAsJson(WISHLIST_SESSION_KEY, sessionWishlist);
            }

            return Json(new { success = true, isAdded = isAdded });
        }

        // 3. حذف عنصر مباشرة من داخل صفحة المفضلة
        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                // مسجل دخول -> حذف من الداتابيز
                var item = await _context.Wishlists
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

                if (item != null)
                {
                    _context.Wishlists.Remove(item);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
            }
            else
            {
                // زائر -> حذف من السيشن
                var sessionWishlist = HttpContext.Session.GetObjectFromJson<List<WishlistSessionItem>>(WISHLIST_SESSION_KEY);
                if (sessionWishlist != null)
                {
                    var item = sessionWishlist.FirstOrDefault(x => x.ProductId == productId);
                    if (item != null)
                    {
                        sessionWishlist.Remove(item);
                        HttpContext.Session.SetObjectAsJson(WISHLIST_SESSION_KEY, sessionWishlist);
                        return Json(new { success = true });
                    }
                }
            }

            return Json(new { success = false });
        }
    }
}