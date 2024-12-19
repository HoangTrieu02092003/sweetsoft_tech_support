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
        public async Task<IActionResult> Index()
        {
            return View(await _context.TblFaqs.ToListAsync());
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
