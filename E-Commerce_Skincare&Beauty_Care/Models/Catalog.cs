using E_Commerce_Skincare_Beauty_Care.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Catalog 
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }
    [Required]
    public string CatalogImage { get; set; }

    [ValidateNever]
    public ICollection<Product> Products { get; set; }
}