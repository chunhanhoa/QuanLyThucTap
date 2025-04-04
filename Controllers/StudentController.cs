using ABC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABC.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly QlpcthucTapContext _context;

        public StudentController(QlpcthucTapContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1)
        {
            // Số bản ghi trên mỗi trang
            const int pageSize = 5;
            // Tính số bản ghi cần bỏ qua
            int skip = (page - 1) * pageSize;

            // Lấy danh sách sinh viên với phân trang
            var students = _context.Sinhviens
                .Include(s => s.MaKhoaNavigation)
                .Include(s => s.MaGvNavigation)
                .Include(s => s.MaNptNavigation)
                .Include(s => s.MaDtNavigation)
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            // Tổng số bản ghi để tính số trang
            int totalRecords = _context.Sinhviens.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Truyền dữ liệu phân trang vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            // Populate ViewBag for dropdowns
            ViewBag.Khoas = _context.Khoas.ToList();
            ViewBag.GiangViens = _context.Giangviens.ToList();
            ViewBag.DeTais = _context.Detais.ToList();
            ViewBag.NguoiPhuTrachs = _context.Nguoiphutraches.ToList();

            return View(students);
        }

        // GET: Student/Create
        public IActionResult Create()
        {
            // Populate dropdowns with data from the database
            ViewBag.Khoas = _context.Khoas.ToList();
            ViewBag.GiangViens = _context.Giangviens.ToList();
            ViewBag.DeTais = _context.Detais.ToList();
            ViewBag.NguoiPhuTrachs = _context.Nguoiphutraches.ToList();

            return View();
        }

        // POST: Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Sinhvien sinhvien)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sinhvien);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            ViewBag.Khoas = _context.Khoas.ToList();
            ViewBag.GiangViens = _context.Giangviens.ToList();
            ViewBag.DeTais = _context.Detais.ToList();
            ViewBag.NguoiPhuTrachs = _context.Nguoiphutraches.ToList();

            return View(sinhvien);
        }

        // GET: Student/Edit/5
        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sinhvien = _context.Sinhviens
                .Include(s => s.MaKhoaNavigation)
                .Include(s => s.MaGvNavigation)
                .Include(s => s.MaNptNavigation)
                .Include(s => s.MaDtNavigation)
                .FirstOrDefault(m => m.MaSv == id);

            if (sinhvien == null)
            {
                return NotFound();
            }

            // Kiểm tra nếu là yêu cầu AJAX (từ fetch), trả về JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    maSv = sinhvien.MaSv,
                    tenSv = sinhvien.TenSv,
                    lop = sinhvien.Lop,
                    sdt = sinhvien.Sdt,
                    email = sinhvien.Email,
                    maGv = sinhvien.MaGv,
                    maKhoa = sinhvien.MaKhoa,
                    maDt = sinhvien.MaDt,
                    maNpt = sinhvien.MaNpt
                });
            }

            // Nếu không phải AJAX, trả về View như cũ
            ViewBag.Khoas = _context.Khoas.ToList();
            ViewBag.GiangViens = _context.Giangviens.ToList();
            ViewBag.DeTais = _context.Detais.ToList();
            ViewBag.NguoiPhuTrachs = _context.Nguoiphutraches.ToList();

            return View(sinhvien);
        }

        // POST: Student/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Sinhvien sinhvien, string OldMaSv)
        {
            if (string.IsNullOrEmpty(OldMaSv))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Tìm sinh viên hiện tại trong database bằng mã cũ
                    var existingStudent = _context.Sinhviens.Find(OldMaSv);
                    if (existingStudent == null)
                    {
                        return NotFound();
                    }

                    // Kiểm tra xem mã sinh viên có thay đổi không
                    bool idChanged = OldMaSv != sinhvien.MaSv;

                    if (idChanged)
                    {
                        // Kiểm tra xem mã mới đã tồn tại chưa
                        if (_context.Sinhviens.Any(s => s.MaSv == sinhvien.MaSv))
                        {
                            TempData["ErrorMessage"] = $"Cảnh báo: Mã sinh viên {sinhvien.MaSv} đã tồn tại trong hệ thống!";
                            ModelState.AddModelError("MaSv", "Mã sinh viên này đã tồn tại trong hệ thống");
                            // Populate ViewBag for dropdowns
                            ViewBag.Khoas = _context.Khoas.ToList();
                            ViewBag.GiangViens = _context.Giangviens.ToList();
                            ViewBag.DeTais = _context.Detais.ToList();
                            ViewBag.NguoiPhuTrachs = _context.Nguoiphutraches.ToList();
                            return View(sinhvien);
                        }

                        // Xóa bản ghi cũ
                        _context.Sinhviens.Remove(existingStudent);
                        _context.SaveChanges();

                        // Tạo bản ghi mới với mã mới
                        _context.Sinhviens.Add(sinhvien);
                        _context.SaveChanges();
                    }
                    else
                    {
                        // Cập nhật các thuộc tính của sinh viên như cũ
                        existingStudent.TenSv = sinhvien.TenSv;
                        existingStudent.Lop = sinhvien.Lop;
                        existingStudent.Sdt = sinhvien.Sdt;
                        existingStudent.Email = sinhvien.Email;
                        existingStudent.MaGv = sinhvien.MaGv;
                        existingStudent.MaKhoa = sinhvien.MaKhoa;
                        existingStudent.MaDt = sinhvien.MaDt;
                        existingStudent.MaNpt = sinhvien.MaNpt;

                        _context.Update(existingStudent);
                        _context.SaveChanges();
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SinhvienExists(OldMaSv))
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
            ViewBag.GiangViens = _context.Giangviens.ToList();
            ViewBag.DeTais = _context.Detais.ToList();
            ViewBag.NguoiPhuTrachs = _context.Nguoiphutraches.ToList();
            return View(sinhvien);
        }

        // POST: Student/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            var sinhvien = _context.Sinhviens.Find(id);
            if (sinhvien != null)
            {
                _context.Sinhviens.Remove(sinhvien);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SinhvienExists(string id)
        {
            return _context.Sinhviens.Any(e => e.MaSv == id);
        }
    }
}