using E_Commerce_Skincare_Beauty_Care.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class ApplicationUser : IdentityUser
{

    [Required, MaxLength(150)]
    public string Name { get; set; } 

    [MaxLength(250)]
    public string Address { get; set; } 

    public DateTime CreatedAt { get; set; } = DateTime.Now; 

    public DateTime? LastLogin { get; set; } 

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    [ValidateNever]
    public ICollection<Order> Orders { get; set; }
    [ValidateNever]
    public ICollection<Review> Reviews { get; set; }
    [ValidateNever]
    public ICollection<Testimonial> Testimonials { get; set; }
    [ValidateNever]
    public ICollection<Wishlist> Wishlists { get; set; }
}