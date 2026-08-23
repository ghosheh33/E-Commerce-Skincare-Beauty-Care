using E_Commerce_Skincare_Beauty_Care.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Product 
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    public string Description { get; set; }

    [Required]
    public int StockQuantity { get; set; } 

    //[MaxLength(50)]
    //public string Barcode { get; set; } 

    public bool IsActive { get; set; } = true; 

    
    public int CatalogId { get; set; } 
    [ForeignKey("CatalogId")]
    [ValidateNever]
    public Catalog Catalog { get; set; }

    [ValidateNever]
    public ICollection<ProductImage> Images { get; set; }
    [ValidateNever]
    public ICollection<Review> Reviews { get; set; }
    [ValidateNever]
    public ICollection<OrderItem> OrderItems { get; set; }
    [ValidateNever]
    public ICollection<Wishlist> WishlistedBy { get; set; }
}