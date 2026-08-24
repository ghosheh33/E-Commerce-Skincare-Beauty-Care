// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using E_Commerce_Skincare_Beauty_Care.Models;
using E_Commerce_Skincare_Beauty_Care.ViewModels;
using E_Commerce_Skincare_Beauty_Care.Data; // تأكدي أن هذا هو هيدر الـ DbContext الخاص بمشروعك
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using E_Commerce_Skincare_Beauty_Care.Extensions;

namespace E_Commerce_Skincare_Beauty_Care.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    // 1. معرفة المستخدم الحالي
                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    if (user != null)
                    {
                        // 2. نقل السلة من الـ Session إلى جدول Order بالداتابيس
                        await MigrateSessionCartToDatabaseAsync(user.Id);
                    }

                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            return Page();
        }

        private async Task MigrateSessionCartToDatabaseAsync(string userId)
        {
            // 1. استخدام نفس الـ Key المعتمد في CartController ("UserCart")
            var sessionCart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("UserCart");

            if (sessionCart != null && sessionCart.Items != null && sessionCart.Items.Any())
            {
                // 2. البحث عن سلة مفتوحة سابقة للمستخدم بحالة "Cart"
                var cartOrder = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.UserId == userId && o.State == "Cart");

                if (cartOrder == null)
                {
                    cartOrder = new Order
                    {
                        UserId = userId,
                        State = "Cart",
                        PaymentMethod = "Pending",
                        OrderDate = DateTime.Now,
                        TotalAmount = 0,
                        OrderItems = new List<OrderItem>()
                    };
                    _context.Orders.Add(cartOrder);
                    await _context.SaveChangesAsync(); // للحصول على Order.Id قبل إضافة الأغراض
                }

                // 3. دمج عناصر الـ Session داخل الـ Order
                foreach (var item in sessionCart.Items)
                {
                    var existingOrderItem = cartOrder.OrderItems
                        .FirstOrDefault(oi => oi.ProductId == item.ProductId);

                    if (existingOrderItem != null)
                    {
                        existingOrderItem.Quantity += item.Quantity;
                    }
                    else
                    {
                        cartOrder.OrderItems.Add(new OrderItem
                        {
                            OrderId = cartOrder.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            PriceAtPurchase = item.Price
                        });
                    }
                }

                // 4. احتساب المبلغ والتأكيد على حفظ كل التغييرات
                cartOrder.TotalAmount = cartOrder.OrderItems.Sum(x => x.Quantity * x.PriceAtPurchase);
                await _context.SaveChangesAsync();

                // 5. مسح السيشن باستخدام نفس الـ Key
                HttpContext.Session.Remove("UserCart");
            }
        }
    }
}