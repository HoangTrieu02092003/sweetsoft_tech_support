using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using admin_sweetsoft_tech_support.Models;
using Microsoft.CodeAnalysis.Scripting;
using BCrypt.Net;


namespace admin_sweetsoft_tech_support.Controllers
{
    public class TblCustomersController : Controller
    {
        private readonly RequestContext _context;

        public TblCustomersController(RequestContext context)
        {
            _context = context;
        }

        // GET: TblCustomers
        public async Task<IActionResult> Index()
        {
            var requestContext = _context.TblCustomers.Include(t => t.CreatedUserNavigation).Include(t => t.UpdatedUserNavigation);
            return View(await requestContext.ToListAsync());
        }

        // GET: TblCustomers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblCustomer = await _context.TblCustomers
                .Include(t => t.CreatedUserNavigation)
                .Include(t => t.UpdatedUserNavigation)
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (tblCustomer == null)
            {
                return NotFound();
            }

            return View(tblCustomer);
        }

        // GET: TblCustomers/Create
        public IActionResult Create()
        {
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId");
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId");
            return View();
        }

        // POST: TblCustomers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,FullName,Email,Phone,TaxCode,Company,Product,Username,Password,Status,ResetToken,ResetTokenExpiry,Token,TokenExpiry,CreatedUser,CreatedAt,UpdatedUser,UpdatedAt")] TblCustomer tblCustomer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tblCustomer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblCustomer.CreatedUser);
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblCustomer.UpdatedUser);
            return View(tblCustomer);
        }

        // GET: TblCustomers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblCustomer = await _context.TblCustomers
                 .Include(t => t.CreatedUserNavigation)
                 .Include(t => t.UpdatedUserNavigation)
                 .FirstOrDefaultAsync(m => m.CustomerId == id);

            if (tblCustomer == null)
            {
                return NotFound();
            }
            var statusList = new List<SelectListItem> {
                new SelectListItem { Value = "1", Text = "Hoạt động" },
                new SelectListItem { Value = "0", Text = "Chưa hoạt động" }
            };
            ViewBag.StatusList = new SelectList(statusList, "Value", "Text", tblCustomer.Status);

            ViewBag.createdUser = tblCustomer.CreatedUserNavigation?.FullName ?? "N/A";
            ViewBag.updatedUser = tblCustomer.UpdatedUserNavigation?.FullName ?? "N/A";
           
            return View(tblCustomer);

        }

        // POST: TblCustomers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,FullName,Email,Phone,TaxCode,Company,Product,Username,Password,Status,ResetToken,ResetTokenExpiry,Token,TokenExpiry,CreatedUser,CreatedAt,UpdatedUser,UpdatedAt")] TblCustomer tblCustomer)
        {
            if (id != tblCustomer.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)

            {
                var existingUser = await _context.TblUsers.FindAsync(id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                // Cập nhật chỉ những thuộc tính được chỉnh sửa, các thuộc tính không thay đổi sẽ giữ nguyên
                if (!string.IsNullOrEmpty(tblCustomer.FullName)) existingUser.FullName = tblCustomer.FullName;
                if (!string.IsNullOrEmpty(tblCustomer.Email)) existingUser.Email = tblCustomer.Email;
                if (!string.IsNullOrEmpty(tblCustomer.Phone)) existingUser.Phone = tblCustomer.Phone;
                if (!string.IsNullOrEmpty(tblCustomer.Username)) existingUser.Username = tblCustomer.Username;
                
                existingUser.Status = tblCustomer.Status;
                existingUser.UpdatedAt = DateTime.Today;
                try
                {
                    _context.Update(tblCustomer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblCustomerExists(tblCustomer.CustomerId))
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
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblCustomer.CreatedUser);
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblCustomer.UpdatedUser);
            return View(tblCustomer);
        }

        // GET: TblCustomers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblCustomer = await _context.TblCustomers
                .Include(t => t.CreatedUserNavigation)
                .Include(t => t.UpdatedUserNavigation)
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (tblCustomer == null)
            {
                return NotFound();
            }

            return View(tblCustomer);
        }

        // POST: TblCustomers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tblCustomer = await _context.TblCustomers.FindAsync(id);
            if (tblCustomer != null)
            {
                _context.TblCustomers.Remove(tblCustomer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Thêm chức năng Register (Đăng ký khách hàng)
        // GET: TblCustomers/Register (Form đăng ký)
        public IActionResult Register()
        {
            return View();
        }

        // POST: TblCustomers/Register (Xử lý đăng ký)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("FullName, Email, Phone, TaxCode, Company, Product, Username, Password")] TblCustomer tblCustomer)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email và username có trùng không
                if (_context.TblCustomers.Any(c => c.Email == tblCustomer.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã tồn tại.");
                    return View(tblCustomer);
                }

                if (_context.TblCustomers.Any(c => c.Username == tblCustomer.Username))
                {
                    ModelState.AddModelError("Username", "Tên tài khoản này đã tồn tại.");
                    return View(tblCustomer);
                }

                // Mã hóa mật khẩu trước khi lưu
                tblCustomer.Password = BCrypt.Net.BCrypt.HashPassword(tblCustomer.Password);

                // Lưu thông tin khách hàng vào cơ sở dữ liệu
                _context.Add(tblCustomer);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index)); // Redirect sau khi đăng ký thành công
            }

            return View(tblCustomer);
        }

        // Kiểm tra tồn tại của khách hàng theo CustomerId
        private bool TblCustomerExists(int id)
        {
            return _context.TblCustomers.Any(e => e.CustomerId == id);
        }

    }
}
