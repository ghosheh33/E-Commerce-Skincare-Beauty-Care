using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using E_Commerce_Skincare_Beauty_Care.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;

namespace E_Commerce_Skincare_Beauty_Care.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CatalogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CatalogsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Catalogs
        public async Task<IActionResult> Index(string searchString)
        {
            if (_context.Catalogs == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Catalogs' is null.");
            }
            ViewData["CurrentFilter"] = searchString;
            var catalogs = from Catalog in _context.Catalogs
                           select Catalog;

            if (!string.IsNullOrEmpty(searchString))
            {
                catalogs = catalogs.Where(s => s.Name!.Contains(searchString) || s.Description!.Contains(searchString));
            }

            return View(await catalogs.ToListAsync());

           
        }

       

        // GET: Catalogs/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Catalog catalog)
        {
            if (ModelState.IsValid)
            {
                if (catalog.ImageUrl != null && catalog.ImageUrl.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "catalogs");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(catalog.ImageUrl.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await catalog.ImageUrl.CopyToAsync(fileStream);
                    }

                    catalog.CatalogImage = "/images/catalogs/" + uniqueFileName;
                }

                _context.Add(catalog);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(catalog);
        }

        
        // GET: Catalogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Catalogs == null)
            {
                return NotFound();
            }

            var catalog = await _context.Catalogs.FindAsync(id);
            if (catalog == null)
            {
                return NotFound();
            }
            return View(catalog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Catalog catalog)
        {
            if (id != catalog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                   
                    var existingCatalog = await _context.Catalogs.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

                    if (existingCatalog == null)
                    {
                        return NotFound();
                    }

                    if (catalog.ImageUrl != null && catalog.ImageUrl.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "catalogs");

                        if (!string.IsNullOrEmpty(existingCatalog.CatalogImage))
                        {
                            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingCatalog.CatalogImage.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(catalog.ImageUrl.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await catalog.ImageUrl.CopyToAsync(fileStream);
                        }

                        catalog.CatalogImage = "/images/catalogs/" + uniqueFileName;
                    }
                    else
                    {
                        catalog.CatalogImage = existingCatalog.CatalogImage;
                    }

                    _context.Update(catalog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CatalogExists(catalog.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(catalog);
        }

        // GET: Catalogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Catalogs == null)
            {
                return NotFound();
            }

            var catalog = await _context.Catalogs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (catalog == null)
            {
                return NotFound();
            }

            return View(catalog);
        }

        // POST: Catalogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Catalogs == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Catalogs'  is null.");
            }
            var catalog = await _context.Catalogs.FindAsync(id);
            if (catalog != null)
            {
                _context.Catalogs.Remove(catalog);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CatalogExists(int id)
        {
          return (_context.Catalogs?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
