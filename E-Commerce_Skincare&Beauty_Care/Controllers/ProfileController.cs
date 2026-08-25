using E_Commerce_Skincare_Beauty_Care.Models;
using E_Commerce_Skincare_Beauty_Care.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_Skincare_Beauty_Care.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;


        public ProfileController(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        // =========================================
        // GET: /Profile
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user =
                await _userManager.GetUserAsync(User);


            if (user == null)
            {
                return Challenge();
            }


            var viewModel = new ProfileViewModel
            {
                Name = user.Name,

                Email = user.Email ?? string.Empty,

                PhoneNumber = user.PhoneNumber,

                Address = user.Address
            };


            return View(viewModel);
        }



        // =========================================
        // POST: /Profile
        // Save Profile Changes
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            ProfileViewModel model)
        {
            var user =
                await _userManager.GetUserAsync(User);


            if (user == null)
            {
                return Challenge();
            }


            // Email is displayed only.
            // Keep the real email from Identity.
            model.Email =
                user.Email ?? string.Empty;


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            user.Name =
                model.Name.Trim();

            user.Address =
                model.Address.Trim();

            user.PhoneNumber =
                string.IsNullOrWhiteSpace(model.PhoneNumber)
                    ? null
                    : model.PhoneNumber.Trim();


            var result =
                await _userManager.UpdateAsync(user);


            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }


            TempData["ProfileSuccess"] =
                "Your profile has been updated successfully.";


            return RedirectToAction(nameof(Index));
        }
    }
}