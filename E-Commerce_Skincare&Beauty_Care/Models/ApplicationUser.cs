using E_Commerce_Skincare_Beauty_Care.Models;
using Microsoft.AspNetCore.Identity;
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
    public ICollection<Order> Orders { get; set; }
    public ICollection<Review> Reviews { get; set; }
    public ICollection<Testimonial> Testimonials { get; set; }
    public ICollection<Wishlist> Wishlists { get; set; }
}