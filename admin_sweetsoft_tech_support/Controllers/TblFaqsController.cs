using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using admin_sweetsoft_tech_support.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using admin_sweetsoft_tech_support.Attributes;

namespace admin_sweetsoft_tech_support.Controllers
{
    public class TblFaqsController : Controller
    {
        private readonly RequestContext _context;

        public TblFaqsController(RequestContext context)
        {
            _context = context;
        }

        // GET: TblFaqs
        public async Task<IActionResult> Index(string search, DateTime? createdFrom, DateTime? createdTo, DateTime? updatedFrom, DateTime? updatedTo, int page = 1)
        {
            var faqs = _context.TblFaqs.AsQueryable();

            // Phân trang
            int pageSize = 6;
            int totalItems = await faqs.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var skip = (page - 1) * pageSize;

            if (!string.IsNullOrEmpty(search))
            {
                var searchUpper = search.ToUpper();
                faqs = faqs.Where(f =>
                    f.Question.Contains(searchUpper) ||
                    f.Answer.Contains(searchUpper)
                );
            }

            var pagedFaqs = await faqs
                .OrderBy(f => f.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;
            ViewData["search"] = search;
            ViewData["CreatedFrom"] = createdFrom?.ToString("dd-MM-yyyy");
            ViewData["CreatedTo"] = createdTo?.ToString("dd-MM-yyyy");
            ViewData["UpdatedFrom"] = updatedFrom?.ToString("yyyy-MM-dd");
            ViewData["UpdatedTo"] = updatedTo?.ToString("yyyy-MM-dd");

            return View(pagedFaqs);
        }

        [PermissionAuthorize("Thêm faq")]
        // GET: TblFaqs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TblFaqs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FaqId,Question,Answer,CreatedAt,UpdatedAt")] TblFaq tblFaq, IFormFile? FaqThumbnail)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    tblFaq.UpdatedAt = DateTime.Now;

                    if (FaqThumbnail != null && FaqThumbnail.Length > 0)
                    {
                        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/images/faqs");
                        var fileName = Path.GetFileName(FaqThumbnail.FileName);
                        var filePath = Path.Combine(uploadDir, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await FaqThumbnail.CopyToAsync(stream);
                        }

                        tblFaq.FaqThumbnail = $"/assets/images/faqs/{fileName}";
                    }

                    _context.Add(tblFaq);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "FAQ đã được tạo mới thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo FAQ. Vui lòng thử lại!";
                }
            }
            return View(tblFaq);
        }

        [PermissionAuthorize("Sửa faq")]
        // GET: TblFaqs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblFaq = await _context.TblFaqs.FindAsync(id);
            if (tblFaq == null)
            {
                return NotFound();
            }
            return View(tblFaq);
        }

        // POST: TblFaqs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FaqId,Question,Answer,CreatedAt,UpdatedAt,FaqThumbnail")] TblFaq tblFaq, IFormFile? FaqThumbnail)
        {
            if (id != tblFaq.FaqId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    tblFaq.UpdatedAt = DateTime.Now;

                    if (FaqThumbnail != null && FaqThumbnail.Length > 0)
                    {
                        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/images/faqs");
                        var fileName = Path.GetFileName(FaqThumbnail.FileName);
                        var filePath = Path.Combine(uploadDir, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await FaqThumbnail.CopyToAsync(stream);
                        }

                        tblFaq.FaqThumbnail = $"/assets/images/faqs/{fileName}";
                    }
                    else
                    {
                        _context.Entry(tblFaq).Property(x => x.FaqThumbnail).IsModified = false;
                    }

                    _context.Update(tblFaq);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "FAQ đã được chỉnh sửa thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblFaqExists(tblFaq.FaqId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Có lỗi xảy ra khi chỉnh sửa FAQ. Vui lòng thử lại!";
                        throw;
                    }
                }
            }
            return View(tblFaq);
        }

        [PermissionAuthorize("Xóa faq")]
        // POST: TblFaqs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tblFaq = await _context.TblFaqs.FindAsync(id);
            if (tblFaq != null)
            {
                _context.TblFaqs.Remove(tblFaq);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "FAQ đã được xóa thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy FAQ cần xóa.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TblFaqExists(int id)
        {
            return _context.TblFaqs.Any(e => e.FaqId == id);
        }
    }
}
