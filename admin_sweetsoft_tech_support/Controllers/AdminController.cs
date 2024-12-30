using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using admin_sweetsoft_tech_support.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using BCrypt.Net;
using System.Net.Mail;
using System.Net;
using admin_sweetsoft_tech_support.Attributes;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class AdminController : Controller
    {
        private readonly RequestContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly SessionService _sessionService;
        private readonly IConfiguration _configuration;

        public AdminController(RequestContext context, ILogger<AdminController> logger,SessionService sessionService, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _sessionService = sessionService;
        }

        private async Task<bool> Validate(string secretKey, string recaptchaResponse)
        {
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", secretKey),
                    new KeyValuePair<string, string>("response", recaptchaResponse)
                });

                var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
                return jsonResponse.success == "true"; // Kiểm tra xem reCAPTCHA có hợp lệ không
            }
        }

        // Trang Login (GET)
        public IActionResult Login()
        {
            var siteKey = _configuration["ReCaptcha:SiteKey"];
            ViewBag.SiteKey = siteKey;
            return View();
        }

        // Xử lý Login (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var siteKey = _configuration["ReCaptcha:SiteKey"];
            const int MaxFailedAttempts = 5; // Số lần đăng nhập thất bại tối đa
            const int LockoutDurationMinutes = 15; // thời gian khóa tài khoản
            var user = await _context.TblUsers.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || user.Status != 1)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản không hợp lệ.");
                ViewBag.SiteKey = siteKey;
                return View();
            }

            if (user.LockoutTime.HasValue && user.LockoutTime > DateTime.Now)
            {
                // Nếu tài khoản đang bị khóa
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa. Vui lòng thử lại sau.");
                ViewBag.SiteKey = siteKey;
                return View();
            }
            var hasPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
            if (!BCrypt.Net.BCrypt.Verify(password, hasPassword))
            {
                // Xử lý đăng nhập thất bại
                user.FailedLoginAttempts = (user.FailedLoginAttempts ?? 0) + 1;

                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.LockoutTime = DateTime.Now.AddMinutes(LockoutDurationMinutes);
                    ModelState.AddModelError(string.Empty, $"Tài khoản của bạn đã bị khóa trong {LockoutDurationMinutes} phút.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Tên người dùng hoặc mật khẩu không chính xác.");
                }

                _context.Update(user);
                await _context.SaveChangesAsync();
                ViewBag.SiteKey = siteKey;
                return View();
            }
            
            var recaptchaSecretKey = _configuration["ReCaptcha:SecretKey"];
            var recaptchaResponseValue = Request.Form["g-recaptcha-response"];
            var isCaptchaValid = await Validate(recaptchaSecretKey, recaptchaResponseValue);

            if (!isCaptchaValid)
            {
                ModelState.AddModelError("", "Mã xác thực không hợp lệ.");
                ViewBag.SiteKey = siteKey;
                return View();
            }

            // Đăng nhập thành công
            user.LastLogin = DateTime.Now;
            user.FailedLoginAttempts = 0; // Reset số lần thất bại
            user.LockoutTime = null; // Bỏ khóa tài khoản nếu bị khóa
            _context.Update(user);
            await _context.SaveChangesAsync();
            await _sessionService.DeleteSessionAsync(user.UserId);
            await _sessionService.CreateSessionAsync(user.UserId, Guid.NewGuid().ToString());

            // Tạo các Claims và Identity cho người dùng đã đăng nhập
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Đăng nhập và lưu thông tin vào Cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
            
            TempData["UserId"] = user.UserId;
            TempData["IsAdmin"] = user.IsAdmin == true ? "true" : "false";
            return RedirectToAction("Index1", "Report"); // Sau khi đăng nhập, chuyển tới trang chính của quản trị viên
        }

        // Đăng xuất (Logout)
        public async Task<IActionResult> Logout()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var username = User.Identity.Name;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Quên mật khẩu (GET)
        public IActionResult ForgotPassword()
        {
            var siteKey = _configuration["ReCaptcha:SiteKey"];
            ViewBag.SiteKey = siteKey;
            return View();
        }

        // Xử lý Quên Mật khẩu (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var siteKey = _configuration["ReCaptcha:SiteKey"];
            var recaptchaSecretKey = _configuration["ReCaptcha:SecretKey"];
            var recaptchaResponseValue = Request.Form["g-recaptcha-response"];
            var isCaptchaValid = await Validate(recaptchaSecretKey, recaptchaResponseValue);
            if (!isCaptchaValid)
            {
                ModelState.AddModelError("", "Mã xác thực không hợp lệ.");
                ViewBag.SiteKey = siteKey;
                return View();
            }

            var user = await _context.TblUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.SiteKey = siteKey;
                ModelState.AddModelError(string.Empty, "Email không tồn tại");
                return View();
            }

            // Tạo mã reset và thời hạn
            string resetToken = Guid.NewGuid().ToString();
            user.ResetToken = resetToken;
            user.ResetTokenExpiry = DateTime.Now.AddMinutes(1); // Token có hiệu lực trong 30 phút

            _context.Update(user);
            await _context.SaveChangesAsync();

            // Gửi email
            string resetLink = Url.Action("ResetPassword", "Admin", new { token = resetToken }, Request.Scheme)!;
            await SendEmailAsync(email, "Đặt lại mật khẩu", $"Nhấp vào link sau để đặt lại mật khẩu: <a href='{resetLink}'>{resetLink}</a>");

            return RedirectToAction("Login");
        }

        // Reset mật khẩu (GET)
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Truy cập không hợp lệ.");
            }
            var user = _context.TblUsers.FirstOrDefault(u => u.ResetToken == token);
            if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
            {
                // Nếu token không tồn tại hoặc đã hết hạn, trả về lỗi
                return BadRequest("Token không hợp lệ hoặc đã hết hạn.");
            }
            // Lưu token vào ViewData để gửi về view
            var siteKey = _configuration["ReCaptcha:SiteKey"];
            ViewBag.SiteKey = siteKey;
            ViewData["Token"] = token;
            return View();
        }

        // Xử lý đặt lại mật khẩu (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string token, string password, string confirmPassword)
        {
            var siteKey = _configuration["ReCaptcha:SiteKey"];
            var recaptchaSecretKey = _configuration["ReCaptcha:SecretKey"];
            var recaptchaResponseValue = Request.Form["g-recaptcha-response"];
            var isCaptchaValid = await Validate(recaptchaSecretKey, recaptchaResponseValue);
            if (!isCaptchaValid)
            {
                ModelState.AddModelError("", "Mã xác thực không hợp lệ.");
                ViewBag.SiteKey = siteKey;
                return View();
            }
            if (password != confirmPassword)
            {
                ViewBag.SiteKey = siteKey;
                ModelState.AddModelError(string.Empty, "Mật khẩu không khớp.");
                return View();
            }

            // Tìm user theo token
            var user = await _context.TblUsers.FirstOrDefaultAsync(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.UtcNow);
            if (user == null)
            {
                return BadRequest("Token không hợp lệ hoặc đã hết hạn.");
            }

            // Cập nhật mật khẩu
            user.Password = BCrypt.Net.BCrypt.HashPassword(password);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            _context.TblUsers.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Login");
        }

        // Hàm gửi email
        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // Cấu hình SMTP client (ví dụ: Gmail SMTP)
            using var client = new System.Net.Mail.SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential("nhantrung890@gmail.com", "mika juyt thab rbit"),
                EnableSsl = true,
            };

            var mailMessage = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress("nhantrung890@gmail.com", "Support Team"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}