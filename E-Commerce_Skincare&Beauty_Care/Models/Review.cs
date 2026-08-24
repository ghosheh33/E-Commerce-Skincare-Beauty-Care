using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using E_Commerce_Skincare_Beauty_Care.Models;

public class Review 
{
    public int Id { get; set; }

    [Required, Range(1, 5)]
    public int Rating { get; set; } 

    public string Comment { get; set; } 

    public bool IsApproved { get; set; } = false; 

    public DateTime CreatedAt { get; set; } = DateTime.Now; 

    
    public int ProductId { get; set; } 
    [ForeignKey("ProductId")]
    [ValidateNever]
    public Product Product { get; set; }

    [Required]
    public string UserId { get; set; }
    [ForeignKey("UserId")]
    [ValidateNever]
    public ApplicationUser User { get; set; }
}