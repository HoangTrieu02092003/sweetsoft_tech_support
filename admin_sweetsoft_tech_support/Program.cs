using admin_sweetsoft_tech_support.Attributes;
using admin_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using System.Security.Cryptography.X509Certificates;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    LogSetup.CreateLogAuditDirectories();
    LogSetup.CreateLogNotificationDirectories();
    LogSetup.CreateLogActivityDirectories();

    var builder = WebApplication.CreateBuilder(args);
    //them NLog
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();
    //them DB
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
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins", builder =>
        {
            builder.WithOrigins("http://stechno.runasp.net") // URL của hệ thống khách hàng
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
    });
    // Add services to the container.
    builder.Services.AddControllersWithViews();
    builder.Services.AddSingleton<EmailHelper>();
    builder.Services.AddScoped<RequestContext>();
    builder.Services.AddScoped<LogService>();
    builder.Services.AddScoped<SessionService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    app.UseCors("AllowSpecificOrigins");
    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<SessionValidationMiddleware>();

    app.MapControllerRoute(
    name: "Denied",
    pattern: "Quyen-han",
    defaults: new { controller = "Home", action = "AccessDenied" });

    app.MapControllerRoute(
        name: "CustomLogin",
        pattern: "Dang-nhap",
        defaults: new { controller = "Admin", action = "Login" });

    app.MapControllerRoute(
        name: "CustomForgot",
        pattern: "Quen-mat-khau",
        defaults: new { controller = "Admin", action = "ForgotPassword" });

    app.MapControllerRoute(
        name: "CustomReset",
        pattern: "Dat-lai-mat-khau-{token}",
        defaults: new { controller = "Admin", action = "ResetPassword" });

    app.MapControllerRoute(
        name: "UserList",
        pattern: "Danh-sach-nguoi-dung",
        defaults: new { controller = "TblUsers", action = "Index" });

    app.MapControllerRoute(
        name: "auditlog",
        pattern: "Nhat-ky-kiem-tra",
        defaults: new { controller = "AuditLogs", action = "Index" });

    app.MapControllerRoute(
        name: "activity",
        pattern: "Nhat-ky-yeu-cau",
        defaults: new { controller = "ActivityLogs", action = "Index" });

    app.MapControllerRoute(
        name: "session",
        pattern: "Quan-ly-phien",
        defaults: new { controller = "Sessions", action = "Index" });

    app.MapControllerRoute(
        name: "notification",
        pattern: "Quan-ly-thong-bao",
        defaults: new { controller = "Notifications", action = "Index" });

    app.MapControllerRoute(
        name: "myNotification",
        pattern: "Thong-bao-cua-toi",
        defaults: new { controller = "Notifications", action = "MyNotifications" });

    app.MapControllerRoute(
        name: "UserDetail",
        pattern: "Chi-tiet-nguoi-dung-{id}",
        defaults: new { controller = "TblUsers", action = "Details" });

    app.MapControllerRoute(
        name: "UserEdit",
        pattern: "Chinh-sua-nguoi-dung-{id}",
        defaults: new { controller = "TblUsers", action = "Edit" });

    app.MapControllerRoute(
        name: "UserAssign",
        pattern: "Cap-quyen-nguoi-dung-{id}",
        defaults: new { controller = "TblUsers", action = "AssignPermissions" });

    app.MapControllerRoute(
        name: "UserAccount",
        pattern: "Ho-so-nguoi-dung",
        defaults: new { controller = "TblUsers", action = "MyAccount" });

    app.MapControllerRoute(
        name: "UserCreate",
        pattern: "Them-nguoi-dung",
        defaults: new { controller = "TblUsers", action = "Create" });

    app.MapControllerRoute(
        name: "DepartmentList",
        pattern: "Danh-sach-phong-ban",
        defaults: new { controller = "TblDepartments", action = "Index" });

    app.MapControllerRoute(
        name: "DepartmentEdit",
        pattern: "Chinh-sua-phong-ban-{id}",
        defaults: new { controller = "TblDepartments", action = "Edit" });

    app.MapControllerRoute(
        name: "RequetsList",
        pattern: "Danh-sach-yeu-cau",
        defaults: new { controller = "TblSupportRequests", action = "Index" });

    app.MapControllerRoute(
        name: "RequetsDetails",
        pattern: "Chi-tiet-yeu-cau-{id}",
        defaults: new { controller = "TblSupportRequests", action = "Details" });

    app.MapControllerRoute(
        name: "RequetsEdit",
        pattern: "Chinh-sua-yeu-cau-{id}",
        defaults: new { controller = "TblSupportRequests", action = "Edit" });

    app.MapControllerRoute(
        name: "RequetsTrans",
        pattern: "Chuyen-giao-yeu-cau-{id}",
        defaults: new { controller = "TblSupportRequests", action = "Transfer" });

    app.MapControllerRoute(
        name: "RequetsCre",
        pattern: "Them-yeu-cau",
        defaults: new { controller = "TblSupportRequests", action = "Create" });

    app.MapControllerRoute(
        name: "Reportindex",
        pattern: "Bao-cao",
        defaults: new { controller = "Report", action = "Index1" });

    app.MapControllerRoute(
        name: "CustomerList",
        pattern: "Danh-sach-khach-hang",
        defaults: new { controller = "TblCustomers", action = "Index" });

    app.MapControllerRoute(
        name: "CustomerCre",
        pattern: "Them-khach-hang",
        defaults: new { controller = "TblCustomers", action = "Create" });

    app.MapControllerRoute(
        name: "CustomerEdit",
        pattern: "Chinh-sua-khach-hang-{id}",
        defaults: new { controller = "TblCustomers", action = "Edit" });

    app.MapControllerRoute(
        name: "FaqList",
        pattern: "Danh-sach-Faq",
        defaults: new { controller = "TblFaqs", action = "Index" });

    app.MapControllerRoute(
        name: "FaqCre",
        pattern: "Them-Faq",
        defaults: new { controller = "TblFaqs", action = "Create" });

    app.MapControllerRoute(
        name: "FaqEdit",
        pattern: "Chinh-sua-Faq-{id}",
        defaults: new { controller = "TblFaqs", action = "Edit" });

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Report}/{action=Index1}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Stopped program because of exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}