using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class OrderItem
{
    public int Id { get; set; }

    [Required]
    public int Quantity { get; set; } 

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal PriceAtPurchase { get; set; } 

    public int OrderId { get; set; } 
    [ForeignKey("OrderId")]
    [ValidateNever]
    public Order Order { get; set; }

    public int ProductId { get; set; } 
    [ForeignKey("ProductId")]
    [ValidateNever]
    public Product Product { get; set; }
}