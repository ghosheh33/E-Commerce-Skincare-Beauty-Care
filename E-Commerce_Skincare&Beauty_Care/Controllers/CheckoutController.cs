using E_Commerce_Skincare_Beauty_Care.Data;
using E_Commerce_Skincare_Beauty_Care.Models;
using E_Commerce_Skincare_Beauty_Care.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace E_Commerce_Skincare_Beauty_Care.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. عرض صفحة الـ Checkout
        // GET: Checkout
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "Identity" });

            // البحث عن طلب يحمل حالة Cart ومحتوي على عناصر
            var cartOrder = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Images)
                .Where(o => o.UserId == user.Id && o.State.ToLower() == "cart")
                .FirstOrDefaultAsync(o => o.OrderItems.Any());

            if (cartOrder == null || cartOrder.OrderItems == null || !cartOrder.OrderItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var shoppingCartVM = new ShoppingCart
            {
                Items = cartOrder.OrderItems.Select(item => new CartItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? "",
                    Price = item.PriceAtPurchase,
                    Quantity = item.Quantity,
                    ImageUrl = item.Product?.Images?.FirstOrDefault()?.ImageUrl ?? "/images/default.jpg"
                }).ToList()
            };

            ViewBag.TotalAmount = cartOrder.OrderItems.Sum(x => x.Quantity * x.PriceAtPurchase);

            return View(shoppingCartVM);
        }

        // 2. إتمام عملية الشراء والتحويل لصفحة التاكيد الجديدة
        // POST: Checkout/ProcessOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOrder(string paymentMethod)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "Identity" });

            var cartOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == user.Id && o.State.ToLower() == "cart")
                .FirstOrDefaultAsync(o => o.OrderItems.Any());

            if (cartOrder == null || cartOrder.OrderItems == null || !cartOrder.OrderItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // تحديث بيانات الطلب
            cartOrder.State = "Processing";
            cartOrder.PaymentMethod = string.IsNullOrEmpty(paymentMethod) ? "Credit Card" : paymentMethod;
            cartOrder.OrderDate = DateTime.Now;
            cartOrder.TotalAmount = cartOrder.OrderItems.Sum(x => x.Quantity * x.PriceAtPurchase);

            await _context.SaveChangesAsync();

            // التحويل المباشر لصفحة Confirmation الجديدة مع تمرير رقم الطلب
            return RedirectToAction("Confirmation", new { id = cartOrder.Id });
        }

        // 3. صفحة تأكيد إتمام الطلب بنجاح (Order Success Page)
        // GET: Checkout/Confirmation/5
        public async Task<IActionResult> Confirmation(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "Identity" });

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        // GET: Checkout/Invoice/5
        public async Task<IActionResult> Invoice(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "Identity" });

            // جلب بيانات الطلب مع التفاصيل والمنتجات والعناوين
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "Identity" });

            // جلب كافة طلبات المستخدم الحالية مع المنتجات والصور، مع استثناء حالة السلة (Cart)
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Images)
                .Where(o => o.UserId == user.Id && o.State.ToLower() != "cart")
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // استخراج الاسم الكامل للمستخدم، وفي حال لم يقم بإدخاله يعود للـ UserName أو الجزء الأول من الإيميل
            string displayName = $"{user.Name}".Trim();

            if (string.IsNullOrEmpty(displayName))
            {
                displayName = !string.IsNullOrEmpty(user.UserName) && !user.UserName.Contains("@")
                    ? user.UserName
                    : user.Email?.Split('@')[0];
            }

            ViewBag.UserName = displayName;

            return View(orders);
        }
    }
}