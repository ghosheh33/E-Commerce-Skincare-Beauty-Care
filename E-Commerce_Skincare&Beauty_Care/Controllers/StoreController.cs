using E_Commerce_Skincare_Beauty_Care.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Skincare_Beauty_Care.Controllers
{
    [Route("shop")]
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StoreController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(
            string? search,
            int? catalogId,
            string? sort)
        {
            var products = _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Include(p => p.Images)
                .Include(p => p.Catalog)
                .Include(p => p.Reviews)
                .AsQueryable();


            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search) ||
                    p.Catalog.Name.Contains(search));
            }


            // Category Filter
            if (catalogId.HasValue)
            {
                products = products.Where(p =>
                    p.CatalogId == catalogId.Value);
            }


            // Sorting
            products = sort switch
            {
                "price-low" =>
                    products.OrderBy(p => p.Price),

                "price-high" =>
                    products.OrderByDescending(p => p.Price),

                _ =>
                    products.OrderByDescending(p => p.Id)
            };


            // Categories for sidebar
            ViewBag.Catalogs = await _context.Catalogs
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();


            // Keep selected values
            ViewBag.Search = search;
            ViewBag.CatalogId = catalogId;
            ViewBag.Sort = sort;


            return View(await products.ToListAsync());
        }
    }
}