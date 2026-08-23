using E_Commerce_Skincare_Beauty_Care.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Catalog 
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }
    [ValidateNever]
    public string CatalogImage { get; set; }

    [NotMapped]
    public IFormFile? ImageUrl { get; set; }
    [ValidateNever]
    public ICollection<Product> Products { get; set; }
}