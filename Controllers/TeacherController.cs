using ABC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABC.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class TeacherController : Controller
    {
        private readonly QlpcthucTapContext _context;

        public TeacherController(QlpcthucTapContext context)
        {
            _context = context;
        }

        // GET: Teacher/Index
        public IActionResult Index(int page = 1)
        {
            // Số bản ghi trên mỗi trang
            const int pageSize = 5;
            // Tính số bản ghi cần bỏ qua
            int skip = (page - 1) * pageSize;

            // Lấy danh sách giảng viên với phân trang
            var teachers = _context.Giangviens
                .Include(g => g.MaKhoaNavigation) // Liên kết với Khoa
                .Include(g => g.Sinhviens)        // Include Sinhviens để đếm số lượng
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            // Tổng số bản ghi để tính số trang
            int totalRecords = _context.Giangviens.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Truyền dữ liệu phân trang vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            // Populate ViewBag cho form
            ViewBag.Khoas = _context.Khoas.ToList();

            return View(teachers);
        }

        // GET: Teacher/Create
        public IActionResult Create()
        {
            ViewBag.Khoas = _context.Khoas.ToList();
            return View();
        }

        // POST: Teacher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Giangvien giangvien)
        {
            if (ModelState.IsValid)
            {
                _context.Add(giangvien);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Khoas = _context.Khoas.ToList();
            return View(giangvien);
        }

        // GET: Teacher/Edit/5
        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var giangvien = _context.Giangviens
                .Include(g => g.MaKhoaNavigation)
                .FirstOrDefault(g => g.MaGv == id);

            if (giangvien == null)
            {
                return NotFound();
            }

            // Kiểm tra nếu là yêu cầu AJAX, trả về JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    maGv = giangvien.MaGv,
                    tenGv = giangvien.TenGv,
                    boMon = giangvien.BoMon ?? "N/A",
                    sdt = giangvien.Sdt ?? "N/A",
                    email = giangvien.Email ?? "N/A",
                    maKhoa = giangvien.MaKhoa
                });
            }

            ViewBag.Khoas = _context.Khoas.ToList();
            return View(giangvien);
        }

        // POST: Teacher/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Giangvien giangvien)
        {
            if (string.IsNullOrEmpty(giangvien.MaGv))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Tìm giảng viên hiện tại trong database
                    var existingTeacher = _context.Giangviens.Find(giangvien.MaGv);
                    if (existingTeacher == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các thuộc tính của giảng viên
                    existingTeacher.TenGv = giangvien.TenGv;
                    existingTeacher.BoMon = giangvien.BoMon;
                    existingTeacher.Sdt = giangvien.Sdt;
                    existingTeacher.Email = giangvien.Email;
                    existingTeacher.MaKhoa = giangvien.MaKhoa;

                    _context.Update(existingTeacher);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GiangvienExists(giangvien.MaGv))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Nếu có lỗi, trả lại form với dữ liệu đã nhập
            ViewBag.Khoas = _context.Khoas.ToList();
            return View(giangvien);
        }

        // POST: Teacher/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            var giangvien = _context.Giangviens.Find(id);
            if (giangvien != null)
            {
                _context.Giangviens.Remove(giangvien);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool GiangvienExists(string id)
        {
            return _context.Giangviens.Any(e => e.MaGv == id);
        }
    }
}