using E_Commerce_Skincare_Beauty_Care.Data;
using E_Commerce_Skincare_Beauty_Care.Extensions;
using E_Commerce_Skincare_Beauty_Care.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace E_Commerce_Skincare_Beauty_Care.Controllers
{
    public class DetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // حساب متوسط التقييمات وعدد التقييمات المعتمدة
            var approvedReviews = product.Reviews?.Where(r => r.IsApproved).ToList() ?? new List<Review>();
            double averageRating = approvedReviews.Any() ? approvedReviews.Average(r => r.Rating) : 0.0;
            int reviewCount = approvedReviews.Count;

            ViewBag.AverageRating = Math.Round(averageRating, 1);
            ViewBag.ReviewCount = reviewCount;

            // التحقق هل المنتج في المفضلة أم لا
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isInWishlist = false;

            if (!string.IsNullOrEmpty(userId))
            {
                isInWishlist = await _context.Wishlists.AnyAsync(w => w.UserId == userId && w.ProductId == id);
            }
            else
            {
                var sessionWishlist = HttpContext.Session.GetObjectFromJson<List<WishlistSessionItem>>("WishlistSession");
                isInWishlist = sessionWishlist != null && sessionWishlist.Any(x => x.ProductId == id);
            }

            ViewBag.IsInWishlist = isInWishlist;

            return View(product);
        }
    }
}