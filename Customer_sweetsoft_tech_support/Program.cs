using Customer_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RequestContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionDB")));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Thêm d?ch v? Authentication v?i Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/dang-nhap";
        options.LogoutPath = "/Custommer/Logout"; 
        options.AccessDeniedPath = "/dang-nhap"; 
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });
builder.Services.AddScoped<RequestContext>();

//var date = DateTime.Now;
//string adminLogPath = Path.GetFullPath(Path.Combine(@"C:\inetpub\wwwroot\myWebsite\Admin\Notifications", $"{date.Year}/{date.Month:D2}/{date.Day:D2}"));
//var normalizedPath = Path.GetFullPath(adminLogPath);
//// Tạo thư mục nếu chưa tồn tại
//if (!Directory.Exists(normalizedPath))
//{
//    Directory.CreateDirectory(normalizedPath);
//}

//string logFilePath = Path.Combine(normalizedPath, $"{date:yyyy-MM-dd-HH}.log");

//// Tạo file log nếu chưa có
//if (!File.Exists(logFilePath))
//{
//    using (var stream = File.Create(logFilePath))
//    {
//        // Đóng file ngay sau khi tạo
//    }
//}
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "account",
    pattern: "Tài-khoản",
    defaults: new { controller = "Account", action = "Index" });

app.MapControllerRoute(
    name: "addRequest",
    pattern: "Gửi-yêu-cầu",
    defaults: new { controller = "TblSupportRequests", action = "Create" });

app.MapControllerRoute(
    name: "followingRequest",
    pattern: "Theo-dõi-yêu-cầu",
    defaults: new { controller = "TblRequestsProcessings", action = "Index" });


app.MapControllerRoute(
    name: "FaqList",
    pattern: "Câu-hỏi-thường-gặp",
    defaults: new { controller = "TblFaqs", action = "Index" });

app.MapControllerRoute(
    name: "FaqDetails",
    pattern: "Chi-tiết-faq-{id}",
    defaults: new { controller = "TblFaqs", action = "Details" });

app.MapControllerRoute(
    name: "contact",
    pattern: "Thông-tin-liên-hệ",
    defaults: new { controller = "Contact", action = "Index" });

app.MapControllerRoute(
    name: "login",
    pattern: "Đăng-nhập",
    defaults: new { controller = "Custommer", action = "Login" });

app.MapControllerRoute(
    name: "register",
    pattern: "Đăng-ký",
    defaults: new { controller = "Custommer", action = "Register" });


app.MapControllerRoute(
    name: "contact",
    pattern: "Quên-mật-khẩu",
    defaults: new { controller = "Custommer", action = "ForgotPassword" });


app.MapControllerRoute(
    name: "contact",
    pattern: "Đặt-lại-mật-khẩu",
    defaults: new { controller = "Custommer", action = "ResetPassword" });


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
