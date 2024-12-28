using Customer_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Common;

namespace Customer_sweetsoft_tech_support.Controllers
{
    public class CustommerController : Controller
    {
        private readonly RequestContext _context;
        private readonly IConfiguration _configuration;

        public CustommerController(RequestContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
            var recaptchaSecretKey = _configuration["ReCaptcha:SecretKey"];
            var recaptchaResponseValue = Request.Form["g-recaptcha-response"];
            var isCaptchaValid = await Validate(recaptchaSecretKey, recaptchaResponseValue);

            if (!isCaptchaValid)
            {
                ModelState.AddModelError("", "Mã xác thực không hợp lệ.");
                ViewBag.SiteKey = siteKey;
                return View();
            }

            var user = await _context.TblCustomers
                .FirstOrDefaultAsync(u => u.Username == username && u.Status == 1);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Username không tồn tại hoặc chưa kích hoạt tài khoản");
                ViewBag.SiteKey = siteKey;
                return View();
            }

            // Kiểm tra mật khẩu
            var hashPass = BCrypt.Net.BCrypt.HashPassword(user.Password);
            if (BCrypt.Net.BCrypt.Verify(password, hashPass)) // Sử dụng BCrypt để so sánh mật khẩu
            {
                // Tạo các Claims và Identity cho người dùng đã đăng nhập
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.CustomerId.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Đăng nhập và lưu thông tin vào Cookie
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                if (TempData["ReturnUrl"] != null)
                {
                    string returnUrl = TempData["ReturnUrl"].ToString();
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            ViewBag.SiteKey = siteKey;
            ModelState.AddModelError(string.Empty, "Mật khẩu sai.");
            return View();
        }

        // Đăng xuất (Logout)
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", new {controller = "Home"});
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

            var user = await _context.TblCustomers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.SiteKey = siteKey;
                ModelState.AddModelError(string.Empty, "Email không tồn tại");
                return View();
            }

            // Tạo mã reset và thời hạn
            string resetToken = Guid.NewGuid().ToString();
            user.ResetToken = resetToken;
            user.ResetTokenExpiry = DateTime.Now.AddMinutes(30);

            _context.Update(user);
            await _context.SaveChangesAsync();

            // Gửi email
            string resetLink = Url.Action("ResetPassword", "Custommer", new { token = resetToken }, Request.Scheme)!;
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
            var user = _context.TblCustomers.FirstOrDefault(u => u.ResetToken == token);
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
            var user = await _context.TblCustomers.FirstOrDefaultAsync(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.UtcNow);
            if (user == null)
            {
                return BadRequest("Token không hợp lệ hoặc đã hết hạn.");
            }

            // Cập nhật mật khẩu
            user.Password = BCrypt.Net.BCrypt.HashPassword(password);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            _context.TblCustomers.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // Hàm gửi email
        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // Cấu hình SMTP client (Gmail SMTP)
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
        // Phương thức hiển thị form đăng ký
        public IActionResult Register()
        {
            var siteKey = _configuration["ReCaptcha:SiteKey"];
            ViewBag.SiteKey = siteKey;
            return View();
        }

        // Phương thức xử lý đăng ký với POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("CustomerId,FullName,Email,Phone,TaxCode,Company,Username,Password,Status,IsDelete,ResetToken,ResetTokenExpiry,Token,TokenExpiry,CreatedBy,CreatedAt,UpdatedBy,UpdatedAt")] TblCustomer tblCustomer)
        {
            var siteKey = _configuration["ReCaptcha:SiteKey"];
            ViewBag.SiteKey = siteKey;
            
            // Thiết lập thông tin cho khách hàng
            tblCustomer.Status = 0;
            tblCustomer.IsDelete = false;
            tblCustomer.Password = BCrypt.Net.BCrypt.HashPassword(tblCustomer.Password);
            tblCustomer.ResetToken = null;
            tblCustomer.ResetTokenExpiry = null;
            tblCustomer.CreatedBy = 1;
            tblCustomer.CreatedAt = DateTime.Now;
            tblCustomer.UpdatedBy = 1;
            tblCustomer.UpdatedAt = DateTime.Now;
            string token = Guid.NewGuid().ToString();
            tblCustomer.Token = token;
            tblCustomer.TokenExpiry = DateTime.Now.AddMinutes(30);

            // Lưu khách hàng vào cơ sở dữ liệu
            _context.Add(tblCustomer);
            await _context.SaveChangesAsync();

            // Tạo liên kết xác nhận
            string link = Url.Action("ConfirmRegistration", "Custommer", new { token }, Request.Scheme)!;

            // Gửi email xác nhận
            await SendEmailAsync(tblCustomer.Email, "Xác nhận đăng ký tài khoản", $"Vui lòng nhấp vào link sau để kích hoạt tài khoản: <a href='{link}'>{link}</a>");

            // Hiển thị thông báo thành công
            TempData["Message"] = "Đăng ký thành công! Vui lòng kiểm tra email để kích hoạt tài khoản.";
            return RedirectToAction("Register");
        }

        // Phương thức xử lý xác nhận đăng ký
        [HttpGet]
        public async Task<IActionResult> ConfirmRegistration(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Token không hợp lệ.";
                return RedirectToAction("Register");
            }

            // Tìm khách hàng theo token
            var customer = await _context.TblCustomers.FirstOrDefaultAsync(c => c.Token == token && c.TokenExpiry > DateTime.Now);
            if (customer == null)
            {
                TempData["Error"] = "Token không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("Register");
            }

            // Kích hoạt tài khoản
            customer.Status = 1; // Kích hoạt
            customer.Token = null; // Xóa token sau khi xác nhận
            customer.TokenExpiry = null;
            _context.Update(customer);
            await _context.SaveChangesAsync();

            // Hiển thị thông báo thành công
            TempData["Message"] = "Tài khoản đã được kích hoạt thành công! Bạn có thể đăng nhập ngay bây giờ.";
            return RedirectToAction("Login", "Custommer"); // Điều hướng đến trang đăng nhập
        }
    }
}
