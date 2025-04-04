using ABC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly QlpcthucTapContext _context;

        public UserController(QlpcthucTapContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Taikhoans.Include(t => t.MaQuyenNavigation).ToListAsync());
        }

        public IActionResult Create()
        {
            ViewBag.Roles = _context.Quyens.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TaiKhoan,MatKhau,MaQuyen")] Taikhoan taikhoan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(taikhoan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Roles = _context.Quyens.ToList();
            return View(taikhoan);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taikhoan = await _context.Taikhoans.FindAsync(id);
            if (taikhoan == null)
            {
                return NotFound();
            }
            ViewBag.Roles = _context.Quyens.ToList();
            return View(taikhoan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaTk,TaiKhoan,MatKhau,MaQuyen")] Taikhoan taikhoan)
        {
            if (id != taikhoan.MaTk)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taikhoan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaikhoanExists(taikhoan.MaTk))
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
            ViewBag.Roles = _context.Quyens.ToList();
            return View(taikhoan);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taikhoan = await _context.Taikhoans
                .Include(t => t.MaQuyenNavigation)
                .FirstOrDefaultAsync(m => m.MaTk == id);
            if (taikhoan == null)
            {
                return NotFound();
            }

            return View(taikhoan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taikhoan = await _context.Taikhoans.FindAsync(id);
            if (taikhoan != null)
            {
                _context.Taikhoans.Remove(taikhoan);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaikhoanExists(int id)
        {
            return _context.Taikhoans.Any(e => e.MaTk == id);
        }
    }
}
