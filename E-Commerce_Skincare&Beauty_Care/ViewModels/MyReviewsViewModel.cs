using E_Commerce_Skincare_Beauty_Care.Models;
using System.ComponentModel.DataAnnotations;

namespace E_Commerce_Skincare_Beauty_Care.Models
{
    public class MyReviewsViewModel
    {
        // لربط بيانات نموذج إضافة تقييم جديد
        [Required(ErrorMessage = "الرجاء اختيار المنتج")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "الرجاء اختيار التقييم من 1 إلى 5")]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required(ErrorMessage = "الرجاء كتابة تفاصيل التقييم")]
        [StringLength(1000)]
        public string Comment { get; set; }

        // القوائم المنقولة للـ View
        public List<Product> AvailableProducts { get; set; } = new List<Product>();
        public List<Review> UserReviews { get; set; } = new List<Review>();
    }
}