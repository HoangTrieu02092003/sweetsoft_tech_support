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
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,FullName,Email,Phone,TaxCode,Company,Username,Password,Status,IsDelete,ResetToken,ResetTokenExpiry,Token,TokenExpiry,CreatedBy,UpdatedBy,CreatedAt,UpdatedAt")] TblCustomer tblCustomer)
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
            ViewData["CreatedBy"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblCustomer.CreatedBy);
            ViewData["UpdatedBy"] = new SelectList(_context.TblUsers, "UserId", "UserId", tblCustomer.UpdatedBy);
            return View(tblCustomer);
        }

        private bool TblCustomerExists(int id)
        {
            return _context.TblCustomers.Any(e => e.CustomerId == id);
        }
    }
}
