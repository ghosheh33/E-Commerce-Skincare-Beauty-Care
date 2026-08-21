using E_Commerce_Skincare_Beauty_Care.Models;
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
    public Catalog Catalog { get; set; }

    
    public ICollection<ProductImage> Images { get; set; }
    public ICollection<Review> Reviews { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<Wishlist> WishlistedBy { get; set; }
}