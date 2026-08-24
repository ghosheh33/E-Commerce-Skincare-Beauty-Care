using E_Commerce_Skincare_Beauty_Care.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_Skincare_Beauty_Care.Models
{
    public class CommunityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommunityController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTestimonial(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["TestimonialError"] =
                    "Please write your story before submitting.";

                return Redirect(
     Url.Action("About", "Home") + "#testimonial-message"
 );
            }


            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }


            var testimonial = new Testimonial
            {
                Content = content.Trim(),
                UserId = userId,
                IsApproved = false,
                CreatedAt = DateTime.Now
            };


            _context.Testimonials.Add(testimonial);

            await _context.SaveChangesAsync();


            TempData["TestimonialSuccess"] =
                "Thank you! Your story was submitted for review.";


            return Redirect(
                Url.Action("About", "Home") + "#testimonial-message"
            );
        }
    }
}