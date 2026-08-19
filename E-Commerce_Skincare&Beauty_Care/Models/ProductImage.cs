using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ProductImage 
{
    public int Id { get; set; }

    [Required]
    public string ImageUrl { get; set; }

    public bool IsMainImage { get; set; }

    public int ProductId { get; set; } 
    [ForeignKey("ProductId")]
    public Product Product { get; set; }
}