using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using E_Commerce_Skincare_Beauty_Care.Data;

var builder = WebApplication.CreateBuilder(args);
//var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

// 1. إعداد الذاكرة المؤقتة وسلة المشتريات (Session)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 2. إعداد قاعدة البيانات
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. إعداد Identity (تم تخفيف القيود لتسريع العمل خلال الـ 6 أيام)
builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false; 
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// 4. دعم الـ MVC و Razor Pages (مهم جداً لصفحات الدخول)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 5. ترتيب هذه الأسطر الثلاثة حساس جداً! (لا تغير ترتيبها)
app.UseAuthentication(); 
app.UseAuthorization();  
app.UseSession();   
app.UseSession();

// 6. مسار لوحة تحكم الإدارة (Admin Area)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// 7. المسار الافتراضي للمتجر
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 8. تفعيل مسارات صفحات تسجيل الدخول الخاصة بـ Identity
app.MapRazorPages();

app.Run();