using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RequestContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionDB")));
//thêm chứng chỉ
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ServerCertificate = new X509Certificate2(@"D:\app\techSupport.pfx", "Nhan071103");
    });
});

// Thêm dịch vụ Authentication với Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/dang-nhap"; // Trang đăng nhập
        options.LogoutPath = "/Admin/Logout"; // Trang đăng xuất
        options.AccessDeniedPath = "/dang-nhap"; // Nếu không có quyền, chuyển đến trang đăng nhập
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian timeout của session
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<RequestContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSession();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "CustomLogin",
    pattern:"dang-nhap",
    defaults: new { controller = "Admin", action = "Login" });

app.MapControllerRoute(
    name: "UserList",
    pattern: "danh-sach-nguoi-dung",
    defaults: new { controller = "TblUsers", action = "Index" });

app.MapControllerRoute(
    name: "UserDetail",
    pattern: "chi-tiet-nguoi-dung-{id}",
    defaults: new { controller = "TblUsers", action = "Details" });

app.MapControllerRoute(
    name: "UserEdit",
    pattern: "chinh-sua-nguoi-dung-{id}",
    defaults: new { controller = "TblUsers", action = "Edit" });

app.MapControllerRoute(
    name: "UserAssign",
    pattern: "cap-quyen-nguoi-dung",
    defaults: new { controller = "TblUsers", action = "AssignPermissions" });

app.MapControllerRoute(
    name: "UserAccount",
    pattern: "ho-so-nguoi-dung",
    defaults: new { controller = "TblUsers", action = "MyAccount" });

app.MapControllerRoute(
    name: "UserCreate",
    pattern: "Them-nguoi-dung",
    defaults: new { controller = "TblUsers", action = "Create" });

app.MapControllerRoute(
    name: "DepartmentList",
    pattern: "danh-sach-phong-ban",
    defaults: new { controller = "TblDepartments", action = "Index" });

app.MapControllerRoute(
    name: "RequetsList",
    pattern: "danh-sach-yeu-cau",
    defaults: new { controller = "TblSupportRequests", action = "Index" });

app.MapControllerRoute(
    name: "Reportindex",
    pattern: "Bao-cao",
    defaults: new { controller = "Report", action = "Index1" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Login}/{id?}");

app.Run();
