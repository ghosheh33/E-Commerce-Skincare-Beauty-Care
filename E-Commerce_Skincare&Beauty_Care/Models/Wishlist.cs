using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Wishlist
{
    public int Id { get; set; }

    
    [Required]
    public string UserId { get; set; } 
    [ForeignKey("UserId")]
    [ValidateNever]
    public ApplicationUser User { get; set; }

    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    [ValidateNever]
    public Product Product { get; set; }
}