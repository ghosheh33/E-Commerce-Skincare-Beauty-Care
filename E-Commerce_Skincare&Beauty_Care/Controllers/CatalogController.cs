using E_Commerce_Skincare_Beauty_Care.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Skincare_Beauty_Care.Controllers
{
    [Route("categories")]
    public class CatalogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /categories
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var catalogs = await _context.Catalogs
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(catalogs);
        }

        // GET: /categories/3
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var catalog = await _context.Catalogs
                .AsNoTracking()
                .Include(c => c.Products.Where(p => p.IsActive))
                    .ThenInclude(p => p.Images)
                .Include(c => c.Products.Where(p => p.IsActive))
                    .ThenInclude(p => p.Reviews)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (catalog == null)
            {
                return NotFound();
            }

            return View(catalog);
        }
    }
}