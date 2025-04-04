using ABC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABC.Controllers
{
    /// <summary>
    /// Controller quản lý đề tài
    /// </summary>
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly QlpcthucTapContext _context;

        public ProjectController(QlpcthucTapContext context)
        {
            _context = context;
        }

        // GET: Project/Index
        public IActionResult Index(int page = 1)
        {
            // Số bản ghi trên mỗi trang
            const int pageSize = 5;
            // Tính số bản ghi cần bỏ qua
            int skip = (page - 1) * pageSize;

            // Lấy danh sách đề tài với phân trang
            var projects = _context.Detais
                .Include(d => d.Sinhviens) // Include Sinhviens để đếm số sinh viên (nếu cần)
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            // Tổng số bản ghi để tính số trang
            int totalRecords = _context.Detais.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Truyền dữ liệu phân trang vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(projects);
        }

        // GET: Project/Create
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Project/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create(Detai detai)
        {
            if (ModelState.IsValid)
            {
                _context.Add(detai);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(detai);
        }

        // GET: Project/Edit/5
        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detai = _context.Detais
                .FirstOrDefault(d => d.MaDt == id);

            if (detai == null)
            {
                return NotFound();
            }

            // Kiểm tra nếu là yêu cầu AJAX, trả về JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    maDt = detai.MaDt,
                    tenDt = detai.TenDt,
                    moTa = detai.MoTa ?? "N/A"
                });
            }

            return View(detai);
        }

        // POST: Project/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Detai detai)
        {
            if (string.IsNullOrEmpty(detai.MaDt))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Tìm đề tài hiện tại trong database
                    var existingProject = _context.Detais.Find(detai.MaDt);
                    if (existingProject == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các thuộc tính của đề tài
                    existingProject.TenDt = detai.TenDt;
                    existingProject.MoTa = detai.MoTa;

                    _context.Update(existingProject);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetaiExists(detai.MaDt))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(detai);
        }

        // POST: Project/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            var detai = _context.Detais.Find(id);
            if (detai != null)
            {
                _context.Detais.Remove(detai);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DetaiExists(string id)
        {
            return _context.Detais.Any(e => e.MaDt == id);
        }
    }
}