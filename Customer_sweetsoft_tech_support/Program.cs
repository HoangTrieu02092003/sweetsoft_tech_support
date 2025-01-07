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
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn session
});
builder.Services.AddScoped<RequestContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseDeveloperExceptionPage();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "account",
    pattern: "Tai-khoan",
    defaults: new { controller = "Account", action = "Index" });

app.MapControllerRoute(
    name: "addRequest",
    pattern: "Gui-yeu-cau",
    defaults: new { controller = "TblSupportRequests", action = "Create" });

app.MapControllerRoute(
    name: "followingRequest",
    pattern: "Theo-doi-yeu-cau",
    defaults: new { controller = "TblRequestsProcessings", action = "Index" });


app.MapControllerRoute(
    name: "FaqList",
    pattern: "Cau-hoi-thuong-gap",
    defaults: new { controller = "TblFaqs", action = "Index" });

app.MapControllerRoute(
    name: "FaqDetails",
    pattern: "Chi-tiet-faq-{id}",
    defaults: new { controller = "TblFaqs", action = "Details" });

app.MapControllerRoute(
    name: "contact",
    pattern: "Thong-tin-lien-he",
    defaults: new { controller = "Contact", action = "Index" });

app.MapControllerRoute(
    name: "login",
    pattern: "Dang-nhap",
    defaults: new { controller = "Custommer", action = "Login" });

app.MapControllerRoute(
    name: "register",
    pattern: "Dang-ky",
    defaults: new { controller = "Custommer", action = "Register" });


app.MapControllerRoute(
    name: "contact",
    pattern: "Quen-mat-khau",
    defaults: new { controller = "Custommer", action = "ForgotPassword" });


app.MapControllerRoute(
    name: "contact",
    pattern: "Dat-lai-mat-khau",
    defaults: new { controller = "Custommer", action = "ResetPassword" });

app.MapControllerRoute(
    name: "email",
    pattern: "Kich-hoat-tai-khoan",
    defaults: new { controller = "Custommer", action = "EnterEmail" });

app.MapControllerRoute(
    name: "confirm",
    pattern: "Gui-lai-email",
    defaults: new { controller = "Custommer", action = "Confirmation" });


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
