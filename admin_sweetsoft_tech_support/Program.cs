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
    pattern: "Quyền-hạn",
    defaults: new { controller = "Home", action = "AccessDenied" });

    app.MapControllerRoute(
        name: "CustomLogin",
        pattern: "Đăng-nhập",
        defaults: new { controller = "Admin", action = "Login" });

    app.MapControllerRoute(
        name: "CustomForgot",
        pattern: "Quên-mật-khẩu",
        defaults: new { controller = "Admin", action = "ForgotPassword" });

    app.MapControllerRoute(
        name: "CustomReset",
        pattern: "Đặt-lại-mật-khẩu-{token}",
        defaults: new { controller = "Admin", action = "ResetPassword" });

    app.MapControllerRoute(
        name: "UserList",
        pattern: "Danh-sách-người-dùng",
        defaults: new { controller = "TblUsers", action = "Index", page = 1, status = "", search = "" });

    app.MapControllerRoute(
        name: "UserList1",
        pattern: "Danh-sách-người-dùng/page-{page}/{status}/{search}",
        defaults: new { controller = "TblUsers", action = "Index"});
    
    app.MapControllerRoute(
        name: "auditlog",
        pattern: "Nhật-ký-kiểm-tra",
        defaults: new { controller = "AuditLogs", action = "Index" });

    app.MapControllerRoute(
        name: "activity",
        pattern: "Nhật-ký-yêu-cầu",
        defaults: new { controller = "ActivityLogs", action = "Index" });

    app.MapControllerRoute(
        name: "session",
        pattern: "Quản-lý-phiên",
        defaults: new { controller = "Sessions", action = "Index" });

    app.MapControllerRoute(
        name: "notification",
        pattern: "Quản-lý-thông-báo",
        defaults: new { controller = "Notifications", action = "Index" });
    
    app.MapControllerRoute(
        name: "myNotification",
        pattern: "Thông-báo-của-tôi",
        defaults: new { controller = "Notifications", action = "MyNotifications" });

    app.MapControllerRoute(
        name: "UserDetail",
        pattern: "Chi-tiết-người-dùng-{id}",
        defaults: new { controller = "TblUsers", action = "Details" });

    app.MapControllerRoute(
        name: "UserEdit",
        pattern: "Chỉnh-sửa-người-dùng-{id}",
        defaults: new { controller = "TblUsers", action = "Edit" });

    app.MapControllerRoute(
        name: "UserAssign",
        pattern: "Cấp-quyền-người-dùng-{id}",
        defaults: new { controller = "TblUsers", action = "AssignPermissions" });

    app.MapControllerRoute(
        name: "UserAccount",
        pattern: "Hồ-sơ-người-dùng",
        defaults: new { controller = "TblUsers", action = "MyAccount" });

    app.MapControllerRoute(
        name: "UserCreate",
        pattern: "Thêm-người-dùng",
        defaults: new { controller = "TblUsers", action = "Create" });

    app.MapControllerRoute(
        name: "DepartmentList",
        pattern: "Danh-sách-phòng-ban",
        defaults: new { controller = "TblDepartments", action = "Index" });

    app.MapControllerRoute(
        name: "DepartmentEdit",
        pattern: "Chỉnh-sửa-phòng-ban-{id}",
        defaults: new { controller = "TblDepartments", action = "Edit" });

    app.MapControllerRoute(
        name: "RequetsList",
        pattern: "Danh-sách-yêu-cầu",
        defaults: new { controller = "TblSupportRequests", action = "Index" });

    app.MapControllerRoute(
        name: "RequetsDetails",
        pattern: "Chi-tiết-yêu-cầu-{id}",
        defaults: new { controller = "TblSupportRequests", action = "Details" });

    app.MapControllerRoute(
        name: "RequetsEdit",
        pattern: "Chỉnh-sửa-yêu-cầu-{id}",
        defaults: new { controller = "TblSupportRequests", action = "Edit" });

    app.MapControllerRoute(
        name: "RequetsTrans",
        pattern: "Chuyển-giao-yêu-cầu-{id}",
        defaults: new { controller = "TblSupportRequests", action = "Transfer" });

    app.MapControllerRoute(
        name: "RequetsCre",
        pattern: "Thêm-yêu-cầu",
        defaults: new { controller = "TblSupportRequests", action = "Create" });

    app.MapControllerRoute(
        name: "Reportindex",
        pattern: "Báo-cáo",
        defaults: new { controller = "Report", action = "Index1" });

    app.MapControllerRoute(
        name: "CustomerList",
        pattern: "Danh-sách-khách-hàng",
        defaults: new { controller = "TblCustomers", action = "Index" });

    app.MapControllerRoute(
        name: "CustomerCre",
        pattern: "Thêm-khách-hàng",
        defaults: new { controller = "TblCustomers", action = "Create" });

    app.MapControllerRoute(
        name: "CustomerEdit",
        pattern: "Chỉnh-sửa-khách-hàng-{id}",
        defaults: new { controller = "TblCustomers", action = "Edit" });

    app.MapControllerRoute(
        name: "FaqList",
        pattern: "Danh-sách-Faq",
        defaults: new { controller = "TblFaqs", action = "Index" });

    app.MapControllerRoute(
        name: "FaqCre",
        pattern: "Thêm-Faq",
        defaults: new { controller = "TblFaqs", action = "Create" });

    app.MapControllerRoute(
        name: "FaqEdit",
        pattern: "Chỉnh-sửa-Faq-{id}",
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