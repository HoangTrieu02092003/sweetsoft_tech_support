using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using admin_sweetsoft_tech_support.Models;

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
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10; // or any number based on your requirement

            // Include related users for CreatedUser and UpdatedUser
            var query = _context.TblCustomers.Include(t => t.CreatedUserNavigation).Include(t => t.UpdatedUserNavigation);

            // Get the total count of customers
            var totalCount = await query.CountAsync();

            // Fetch the paged list of customers
            var customers = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Calculate total pages for pagination
            ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewData["CurrentPage"] = page;

            // You don't need to set CreatedUser and UpdatedUser for the entire list.
            // You can keep them for general purposes if needed:
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName");
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName");

            return View(customers);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActivation(int customerId)
        {
            var customer = await _context.TblCustomers.FindAsync(customerId);

            if (customer != null)
            {
                // Đổi trạng thái: Nếu hiện tại là 1 (kích hoạt), chuyển thành 0 (chưa kích hoạt) và ngược lại
                customer.Status = (short)(customer.Status == 1 ? 0 : 1);

                // Cập nhật khách hàng
                _context.Update(customer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Trạng thái đã được thay đổi.";
            }

            return RedirectToAction(nameof(Index));
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
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName");
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName");
            return View();
        }

        // POST: TblCustomers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,FullName,Email,Phone,TaxCode,Company,Product,Username,Password,Status,ResetToken,ResetTokenExpiry,Token,TokenExpiry,CreatedUser,CreatedAt,UpdatedUser,UpdatedAt")] TblCustomer tblCustomer)
        {
            // Kiểm tra sự trùng lặp của Username
            bool isUsernameExist = await _context.TblCustomers.AnyAsync(c => c.Username == tblCustomer.Username);
            if (isUsernameExist)
            {
                ModelState.AddModelError("Username", "Username đã tồn tại. Vui lòng chọn một tên khác.");
            }

            // Kiểm tra sự trùng lặp của Email
            bool isEmailExist = await _context.TblCustomers.AnyAsync(c => c.Email == tblCustomer.Email);
            if (isEmailExist)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại. Vui lòng sử dụng một email khác.");
            }

            // Nếu có lỗi trong ModelState, trả lại form để người dùng sửa
            if (!ModelState.IsValid)
            {
                ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName");
                ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName");
                return View(tblCustomer);
            }

            // Nếu không có lỗi, thêm khách hàng vào cơ sở dữ liệu
            tblCustomer.Status = 0;
            _context.Add(tblCustomer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: TblCustomers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblCustomer = await _context.TblCustomers.FindAsync(id);
            if (tblCustomer == null)
            {
                return NotFound();
            }
            ViewData["CreatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName", tblCustomer.CreatedUser);
            ViewData["UpdatedUser"] = new SelectList(_context.TblUsers, "UserId", "FullName", tblCustomer.UpdatedUser);
            return View(tblCustomer);
        }

        // POST: TblCustomers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        private bool TblCustomerExists(int id)
        {
            return _context.TblCustomers.Any(e => e.CustomerId == id);
        }
    }
}
