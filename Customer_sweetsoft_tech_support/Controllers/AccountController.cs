using Customer_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Customer_sweetsoft_tech_support.Controllers
{
    public class AccountController : Controller
    {
        private readonly RequestContext _context;

        public AccountController(RequestContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ReturnUrl"] = Url.RouteUrl("account");
                return RedirectToAction("Login", "Custommer");
            }
            var id = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var tblCustomer = await _context.TblCustomers.FindAsync(id);
            ViewData["CreatedBy"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblCustomer.CreatedBy);
            ViewData["UpdatedBy"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblCustomer.UpdatedBy);
            return View(tblCustomer);
        }

        // POST: TblCustomers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string fullname, string company, string taxCode, string Email, string phone)
        {
            var customerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (id != customerId)
            {
                return NotFound();
            }
            try
            {
                var existingCustomer = await _context.TblCustomers.FindAsync(id);
                if (existingCustomer == null)
                {
                    return NotFound();
                }

                // Cập nhật các trường thay đổi
                existingCustomer.FullName = fullname;
                existingCustomer.Email = Email;
                existingCustomer.Phone = phone;
                existingCustomer.TaxCode = taxCode;
                existingCustomer.Company = company;
                existingCustomer.UpdatedAt = DateTime.Now;

                _context.Update(existingCustomer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TblCustomerExists(customerId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ChangePassword(int id, string oldPassword, string newPassword, string confirmPassword)
        {
            try
            {
                // Kiểm tra xem người dùng có tồn tại không
                var user = _context.TblCustomers.FirstOrDefault(u => u.CustomerId == id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                    RedirectToAction("Index");
                }

                // Kiểm tra mật khẩu cũ
                else if (!VerifyPasswordHash(oldPassword, user.Password))
                {
                    TempData["ErrorMessage"] = "Mật khẩu cũ không chính xác.";
                    RedirectToAction("Index");
                }

                // Kiểm tra mật khẩu mới và xác nhận mật khẩu có khớp không
                else if (newPassword != confirmPassword)
                {
                    TempData["ErrorMessage"] = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                    RedirectToAction("Index");
                }

                // Kiểm tra độ mạnh của mật khẩu mới (ví dụ: sử dụng thư viện Regular Expressions)
                else if (!IsValidPassword(newPassword))
                {
                    TempData["ErrorMessage"] = "Mật khẩu mới không đủ mạnh. Vui lòng sử dụng ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt.";
                    RedirectToAction("Index");
                }

                else
                {
                    TempData["SuccessMessage"] = "Thay đổi mật khẩu thành công.";
                    user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    _context.Update(user);
                    _context.SaveChanges();
                }
                
                return RedirectToAction("Index"); ;
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi thay đổi mật khẩu. Vui lòng thử lại sau.";
                return View(nameof(Index));
            }
        }

        // Hàm kiểm tra độ mạnh của mật khẩu
        private bool IsValidPassword(string password)
        {
            // Sử dụng biểu thức chính quy để kiểm tra
            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");
            return regex.IsMatch(password);
        }

        // Phương thức kiểm tra hash mật khẩu
        private bool VerifyPasswordHash(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        private bool TblCustomerExists(int id)
        {
            return _context.TblCustomers.Any(e => e.CustomerId == id);
        }
    }
}
