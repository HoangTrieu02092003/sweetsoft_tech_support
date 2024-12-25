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
    public class TblFaqsController : Controller
    {
        private readonly RequestContext _context;

        public TblFaqsController(RequestContext context)
        {
            _context = context;
        }

        // GET: TblFaqs
        public async Task<IActionResult> Index(string searchTerm, DateTime? createdFrom, DateTime? createdTo, DateTime? updatedFrom, DateTime? updatedTo, int page = 1)
        {
            var faqs = _context.TblFaqs.AsQueryable();

            // Lọc theo tên câu hỏi (tìm gần đúng)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                faqs = faqs.Where(f => f.Question.Contains(searchTerm));
            }

            // Lọc theo ngày tạo
            if (createdFrom.HasValue)
            {
                faqs = faqs.Where(f => f.CreatedAt >= createdFrom.Value);
            }
            if (createdTo.HasValue)
            {
                faqs = faqs.Where(f => f.CreatedAt <= createdTo.Value);
            }

            // Lọc theo ngày cập nhật
            if (updatedFrom.HasValue)
            {
                faqs = faqs.Where(f => f.UpdatedAt >= updatedFrom.Value);
            }
            if (updatedTo.HasValue)
            {
                faqs = faqs.Where(f => f.UpdatedAt <= updatedTo.Value);
            }

            // Phân trang
            int pageSize = 6;
            int totalItems = await faqs.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var skip = (page - 1) * pageSize;

            var pagedFaqs = await faqs
                .OrderBy(f => f.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            return View(pagedFaqs);
        }

        // GET: TblFaqs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblFaq = await _context.TblFaqs
                .FirstOrDefaultAsync(m => m.FaqId == id);
            if (tblFaq == null)
            {
                return NotFound();
            }

            return View(tblFaq);
        }

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
        public async Task<IActionResult> Create([Bind("FaqId,Question,Answer,CreatedAt,UpdatedAt")] TblFaq tblFaq)
        {
            if (ModelState.IsValid)
            {
                // Cập nhật ngày giờ cho trường UpdatedAt
                tblFaq.UpdatedAt = DateTime.Now;

                _context.Add(tblFaq);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tblFaq);
        }

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
        public async Task<IActionResult> Edit(int id, [Bind("FaqId,Question,Answer,CreatedAt,UpdatedAt")] TblFaq tblFaq)
        {
            if (id != tblFaq.FaqId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật ngày giờ cho trường UpdatedAt
                    tblFaq.UpdatedAt = DateTime.Now;

                    _context.Update(tblFaq);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblFaqExists(tblFaq.FaqId))
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
            return View(tblFaq);
        }

        // GET: TblFaqs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblFaq = await _context.TblFaqs
                .FirstOrDefaultAsync(m => m.FaqId == id);
            if (tblFaq == null)
            {
                return NotFound();
            }

            return View(tblFaq);
        }

        // POST: TblFaqs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tblFaq = await _context.TblFaqs.FindAsync(id);
            if (tblFaq != null)
            {
                _context.TblFaqs.Remove(tblFaq);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TblFaqExists(int id)
        {
            return _context.TblFaqs.Any(e => e.FaqId == id);
        }
    }
}
