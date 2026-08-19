using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Wishlist
{
    public int Id { get; set; }

    
    [Required]
    public string UserId { get; set; } 
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }

    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    public Product Product { get; set; }
}