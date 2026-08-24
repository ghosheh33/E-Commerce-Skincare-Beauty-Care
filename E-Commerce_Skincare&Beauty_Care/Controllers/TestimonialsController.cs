using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using E_Commerce_Skincare_Beauty_Care.Data;
using Microsoft.AspNetCore.Authorization;

namespace E_Commerce_Skincare_Beauty_Care.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TestimonialsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestimonialsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Testimonials
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Testimonials.Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.Testimonials == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Testimonials'  is null.");
            }
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial != null)
            {
                _context.Testimonials.Remove(testimonial);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial == null)
            {
                return NotFound();
            }

            testimonial.IsApproved = true;
            _context.Update(testimonial);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial == null)
            {
                return NotFound();
            }

            testimonial.IsApproved = false;
            _context.Update(testimonial);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }


}
