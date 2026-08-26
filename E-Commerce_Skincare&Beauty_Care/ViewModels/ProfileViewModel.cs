using System.ComponentModel.DataAnnotations;

namespace E_Commerce_Skincare_Beauty_Care.ViewModels
{
    public class ProfileViewModel
    {
        [Required]
        [StringLength(150)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;


        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;


        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }


        [Required]
        [StringLength(250)]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;
    }
}