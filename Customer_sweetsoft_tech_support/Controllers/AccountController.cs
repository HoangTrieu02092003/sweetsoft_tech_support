using Customer_sweetsoft_tech_support.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
                TempData["ReturnUrl"] = Url.Action("Index", "Account");
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
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,FullName,Email,Phone,TaxCode,Username,Password,Status,IsDelete,ResetToken,ResetTokenExpiry,Token,TokenExpiry,Company,UpdatedAt,UpdatedBy,CreateAt,CreateBy")] TblCustomer tblCustomer)
        {
            if (id != tblCustomer.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCustomer = await _context.TblCustomers.FindAsync(id);
                    if (existingCustomer == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các trường thay đổi
                    existingCustomer.FullName = tblCustomer.FullName;
                    existingCustomer.Email = tblCustomer.Email;
                    existingCustomer.Phone = tblCustomer.Phone;
                    existingCustomer.TaxCode = tblCustomer.TaxCode;
                    existingCustomer.Company = tblCustomer.Company;
                    existingCustomer.UpdatedAt = DateTime.Now;

                    _context.Update(existingCustomer);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
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
            TempData["ErrorMessage"] = "Cập nhật thông tin thất bại. Vui lòng kiểm tra lại dữ liệu.";
            // Trả về view với dữ liệu nếu ModelState không hợp lệ
            return View("Index", tblCustomer);
        }

        private bool TblCustomerExists(int id)
        {
            return _context.TblCustomers.Any(e => e.CustomerId == id);
        }
    }
}
