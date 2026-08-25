using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using E_Commerce_Skincare_Beauty_Care.Models;


public class Testimonial
{
    public int Id { get; set; } 

    [Required]
    public string Content { get; set; } 

    public bool IsApproved { get; set; } = false; 

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Foreign Key
    [Required]
    public string UserId { get; set; } 
    [ForeignKey("UserId")]
    [ValidateNever]
    public ApplicationUser User { get; set; }
}