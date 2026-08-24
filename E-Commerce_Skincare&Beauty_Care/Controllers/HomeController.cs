using E_Commerce_Skincare_Beauty_Care.Data;
using E_Commerce_Skincare_Beauty_Care.Models;
using E_Commerce_Skincare_Beauty_Care.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace E_Commerce_Skincare_Beauty_Care.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredProducts = await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .OrderByDescending(p => p.Id)
                .Take(3)
                .ToListAsync();

            var testimonials = await _context.Testimonials
                .AsNoTracking()
                .Where(t => t.IsApproved)
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .Take(2)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                FeaturedProducts = featuredProducts,
                Testimonials = testimonials
            };

            return View(viewModel);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                            ?? HttpContext.TraceIdentifier
            });
        }
    }
}