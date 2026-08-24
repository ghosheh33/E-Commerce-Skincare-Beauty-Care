using E_Commerce_Skincare_Beauty_Care.Data;
using E_Commerce_Skincare_Beauty_Care.Extensions;
using E_Commerce_Skincare_Beauty_Care.Models;
using E_Commerce_Skincare_Beauty_Care.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace E_Commerce_Skincare_Beauty_Care.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "UserCart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. عرض صفحة السلة
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey) ?? new ShoppingCart();
            return View(cart);
        }

        // 2. إضافة منتج إلى السلة
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return NotFound();
            }

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey) ?? new ShoppingCart();
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                // التأكد من عدم تجاوز المخزون عند الإضافة مجدداً
                if (existingItem.Quantity + quantity <= product.StockQuantity)
                {
                    existingItem.Quantity += quantity;
                    existingItem.StockQuantity = product.StockQuantity; // تحديث قيمة الـ Stock
                }
                else
                {
                    TempData["ErrorMessage"] = $"Cannot add more. Maximum available stock is {product.StockQuantity}.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                var imageUrl = product.Images?.FirstOrDefault()?.ImageUrl ?? "/images/default-product.jpg";

                // تحديد الكمية المضافة بحيث لا تتجاوز المخزون
                int initialQuantity = quantity <= product.StockQuantity ? quantity : product.StockQuantity;

                cart.Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = initialQuantity,
                    StockQuantity = product.StockQuantity, // <-- تم ربط المخزون هنا
                    ImageUrl = imageUrl
                });
            }

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
            TempData["SuccessMessage"] = $"{product.Name} added to your cart!";

            return RedirectToAction("Index");
        }

        // 3. تحديث الكمية (زيادة / نقصان) مع فحص الـ Stock
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, string actionType)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey);
            if (cart == null) return RedirectToAction("Index");

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                var product = await _context.Products.FindAsync(productId);
                if (product != null)
                {
                    item.StockQuantity = product.StockQuantity; // تحديث المخزون من الداتابيز

                    if (actionType == "increase")
                    {
                        if (item.Quantity < product.StockQuantity)
                        {
                            item.Quantity++;
                        }
                        else
                        {
                            TempData["ErrorMessage"] = $"Maximum available quantity for {product.Name} is {product.StockQuantity}.";
                        }
                    }
                    else if (actionType == "decrease")
                    {
                        item.Quantity--;
                        if (item.Quantity <= 0)
                        {
                            cart.Items.Remove(item);
                        }
                    }

                    HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
                }
            }

            return RedirectToAction("Index");
        }

        // 4. حذف عنصر من السلة عبر AJAX
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey);
            if (cart != null)
            {
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    cart.Items.Remove(item);
                    HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

                    return Json(new
                    {
                        success = true,
                        grandTotal = cart.GrandTotal.ToString("F2"),
                        isCartEmpty = !cart.Items.Any()
                    });
                }
            }
            return Json(new { success = false, message = "Item not found in cart" });
        }

        // 5. تحويل السلة إلى Order و OrderItem في الداتابيس (تأكيد الشراء)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(string paymentMethod)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey);
            if (cart == null || !cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // إنشاء الطلب الرئيسي Order
            var order = new Order
            {
                UserId = userId!,
                TotalAmount = cart.GrandTotal,
                PaymentMethod = string.IsNullOrEmpty(paymentMethod) ? "Cash On Delivery" : paymentMethod,
                State = "Processing",
                OrderDate = DateTime.Now,
                OrderItems = new List<OrderItem>()
            };

            // تحويل عناصر السلة لـ OrderItems في الداتابيس
            foreach (var item in cart.Items)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    PriceAtPurchase = item.Price
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // تفريغ السلة من الـ Session بعد حفظ الطلب
            HttpContext.Session.Remove(CartSessionKey);

            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }

        // 6. صفحة تأكيد الطلب
        [Authorize]
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}