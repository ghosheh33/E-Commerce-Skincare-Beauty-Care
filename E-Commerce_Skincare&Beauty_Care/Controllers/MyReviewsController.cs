using E_Commerce_Skincare_Beauty_Care.Data;
using E_Commerce_Skincare_Beauty_Care.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace E_Commerce_Skincare_Beauty_Care.Controllers
{
    [Authorize] // حماية الكنترولر كاملاً لضمان عدم دخوله إلا بعد تسجيل الدخول
    public class MyReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyReviewsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /MyReviews
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            ViewData["Products"] = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .SelectMany(o => o.OrderItems)
                .Select(oi => oi.Product)
                .Distinct()
                .ToListAsync();

            var userReviews = await _context.Reviews
                .Include(r => r.Product)
                    .ThenInclude(p => p.Images) // جلب الصور المرتبطة بالمنتج
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(userReviews);
        }

        // POST: /MyReviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int ProductId, int Rating, string Comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (Rating > 0 && !string.IsNullOrEmpty(Comment) && ProductId > 0)
            {
                var review = new Review
                {
                    ProductId = ProductId,
                    Rating = Rating,
                    Comment = Comment,
                    UserId = user.Id,
                    IsApproved = false, // ينتظر موافقة الأدمن
                    CreatedAt = DateTime.Now
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thank you! Your review has been submitted for approval.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}