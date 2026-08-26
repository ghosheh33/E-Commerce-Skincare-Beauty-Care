using System.Collections.Generic;

namespace E_Commerce_Skincare_Beauty_Care.ViewModels
{
    public class HomeViewModel
    {
        public List<Product> FeaturedProducts { get; set; }
            = new List<Product>();

        public List<Catalog> Categories { get; set; }
            = new List<Catalog>();

        public List<Testimonial> Testimonials { get; set; }
            = new List<Testimonial>();
    }
}