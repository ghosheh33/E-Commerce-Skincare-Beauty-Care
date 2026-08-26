

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using E_Commerce_Skincare_Beauty_Care.Models;
using Microsoft.AspNetCore.Authorization;

namespace E_Commerce_Skincare_Beauty_Care.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;


        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Settings()
        {
            return View();
        }

        public async Task<IActionResult> users()
        {
            var allUsers = await _userManager.Users.ToListAsync();

            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(user.Id, roles);
            }

            ViewBag.UserRoles = userRoles;

            return View(allUsers);
        }

        // GET: Admin/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser updatedUser)
        {
            if (id != updatedUser.Id)
            {
                return NotFound();
            }

            
            var userInDb = await _userManager.FindByIdAsync(id);
            if (userInDb == null)
            {
                return NotFound();
            }

            userInDb.Name = updatedUser.Name;
            userInDb.Address = updatedUser.Address;
            userInDb.IsActive = updatedUser.IsActive;

            // حفظ التعديلات
            var result = await _userManager.UpdateAsync(userInDb);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(users));
            }

            // في حال وجود أخطاء، قم بعرضها
            foreach (var error in result.Errors)
            {
                TempData["ُrror"] = error;
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(updatedUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = !user.IsActive;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["info"] =user.Name+(user.IsActive?" is Active": " is Inactive");
                return RedirectToAction(nameof(users));
            }

            return BadRequest("حدث خطأ أثناء تحديث حالة المستخدم.");
        }

    }
}