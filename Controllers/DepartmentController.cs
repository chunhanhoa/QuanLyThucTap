using ABC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABC.Controllers
{
    /// <summary>
    /// Controller quản lý khoa
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class DepartmentController : Controller
    {
        private readonly QlpcthucTapContext _context;

        public DepartmentController(QlpcthucTapContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Hiển thị trang danh sách khoa
        /// </summary>
        /// <param name="page">Số trang hiện tại</param>
        /// <returns>View danh sách khoa</returns>
        public IActionResult Index(int page = 1)
        {
            // Số bản ghi trên mỗi trang
            const int pageSize = 5;
            // Tính số bản ghi cần bỏ qua
            int skip = (page - 1) * pageSize;

            // Lấy danh sách khoa với phân trang
            var departments = _context.Khoas
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            // Tổng số bản ghi để tính số trang
            int totalRecords = _context.Khoas.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Truyền dữ liệu phân trang vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(departments);
        }

        /// <summary>
        /// Tạo khoa mới
        /// </summary>
        /// <param name="khoa">Đối tượng Khoa từ form</param>
        /// <returns>Chuyển hướng về trang danh sách khoa</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Khoa khoa)
        {
            if (ModelState.IsValid)
            {
                _context.Khoas.Add(khoa);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View("Index", _context.Khoas.ToList());
        }

        /// <summary>
        /// Lấy thông tin khoa để chỉnh sửa
        /// </summary>
        /// <param name="id">Mã khoa</param>
        /// <returns>JSON chứa thông tin khoa</returns>
        [HttpGet]
        public IActionResult Edit(string id)
        {
            var khoa = _context.Khoas.Find(id);
            if (khoa == null)
            {
                return NotFound();
            }
            return Json(new
            {
                maKhoa = khoa.MaKhoa,
                tenKhoa = khoa.TenKhoa
            });
        }

        /// <summary>
        /// Cập nhật thông tin khoa
        /// </summary>
        /// <param name="khoa">Đối tượng Khoa từ form</param>
        /// <returns>Chuyển hướng về trang danh sách khoa</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Khoa khoa)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(khoa);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Khoas.Any(k => k.MaKhoa == khoa.MaKhoa))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View("Index", _context.Khoas.ToList());
        }

        /// <summary>
        /// Xóa khoa
        /// </summary>
        /// <param name="id">Mã khoa</param>
        /// <returns>Chuyển hướng về trang danh sách khoa</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id)
        {
            var khoa = _context.Khoas.Find(id);
            if (khoa == null)
            {
                return NotFound();
            }
            _context.Khoas.Remove(khoa);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}