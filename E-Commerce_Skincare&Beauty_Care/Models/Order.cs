using E_Commerce_Skincare_Beauty_Care.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Order 
{
    public int Id { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; } 

    [Required, MaxLength(50)]
    public string State { get; set; } = "Processing"; 

    [Required, MaxLength(50)]
    public string PaymentMethod { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;

    [Required]
    public string UserId { get; set; }
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }

    
    public ICollection<OrderItem> OrderItems { get; set; }
}