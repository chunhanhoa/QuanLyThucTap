using ABC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABC.Controllers
{
    /// <summary>
    /// Controller quản lý cài đặt tài khoản
    /// </summary>
    public class AccountSettingsController : Controller
    {
        private readonly QlpcthucTapContext _context;

        public AccountSettingsController(QlpcthucTapContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Hiển thị trang danh sách tài khoản
        /// </summary>
        /// <param name="page">Số trang hiện tại</param>
        public IActionResult Index(int page = 1)
        {
            // Số bản ghi trên mỗi trang
            const int pageSize = 5;
            // Tính số bản ghi cần bỏ qua
            int skip = (page - 1) * pageSize;

            // Lấy danh sách tài khoản với phân trang
            var accounts = _context.Taikhoans
                .Include(t => t.MaQuyenNavigation)
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            // Tổng số bản ghi để tính số trang
            int totalRecords = _context.Taikhoans.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Truyền dữ liệu phân trang vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            ViewBag.Roles = _context.Quyens.ToList(); // Để đổ vào dropdown
            return View(accounts);
        }

        /// <summary>
        /// Tạo tài khoản mới
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Taikhoan taikhoan)
        {
            // Debug dữ liệu nhận được
            Console.WriteLine($"Received Taikhoan: MaTk={taikhoan.MaTk}, TaiKhoan={taikhoan.TaiKhoan}, MatKhau={taikhoan.MatKhau}, MaQuyen={taikhoan.MaQuyen}");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                TempData["ErrorMessage"] = "Lỗi validation: " + string.Join(", ", errors);
                ViewBag.Roles = _context.Quyens.ToList();
                return View("Index", _context.Taikhoans.ToList());
            }

            // Kiểm tra nếu MaTk đã tồn tại (nếu không tự tăng)
            if (_context.Taikhoans.Any(t => t.MaTk == taikhoan.MaTk))
            {
                TempData["ErrorMessage"] = "Mã tài khoản đã tồn tại!";
                ViewBag.Roles = _context.Quyens.ToList();
                return View("Index", _context.Taikhoans.ToList());
            }

            // Kiểm tra MaQuyen hợp lệ
            if (taikhoan.MaQuyen == null || !_context.Quyens.Any(q => q.MaQuyen == taikhoan.MaQuyen))
            {
                TempData["ErrorMessage"] = "Quyền không hợp lệ!";
                ViewBag.Roles = _context.Quyens.ToList();
                return View("Index", _context.Taikhoans.ToList());
            }

            try
            {
                _context.Taikhoans.Add(taikhoan);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Thêm tài khoản thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi thêm tài khoản: {ex.Message}";
                ViewBag.Roles = _context.Quyens.ToList();
                return View("Index", _context.Taikhoans.ToList());
            }
        }

        /// <summary>
        /// Lấy thông tin tài khoản để chỉnh sửa
        /// </summary>
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var taikhoan = _context.Taikhoans.Find(id);
            if (taikhoan == null)
            {
                return NotFound();
            }
            return Json(new
            {
                maTk = taikhoan.MaTk,
                taiKhoan = taikhoan.TaiKhoan,
                matKhau = taikhoan.MatKhau,
                maQuyen = taikhoan.MaQuyen
            });
        }

        /// <summary>
        /// Cập nhật tài khoản
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Taikhoan taikhoan)
        {
            // Debug dữ liệu nhận được
            Console.WriteLine($"Received Taikhoan (Edit): MaTk={taikhoan.MaTk}, TaiKhoan={taikhoan.TaiKhoan}, MatKhau={taikhoan.MatKhau}, MaQuyen={taikhoan.MaQuyen}");

            if (taikhoan.MaTk <= 0)
            {
                TempData["ErrorMessage"] = "Mã tài khoản không hợp lệ!";
                ViewBag.Roles = _context.Quyens.ToList();
                return View("Index", _context.Taikhoans.ToList());
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                TempData["ErrorMessage"] = "Lỗi validation: " + string.Join(", ", errors);
                ViewBag.Roles = _context.Quyens.ToList();
                return View("Index", _context.Taikhoans.ToList());
            }

            try
            {
                // Tìm tài khoản hiện tại trong database
                var existingAccount = _context.Taikhoans.Find(taikhoan.MaTk);
                if (existingAccount == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tài khoản!";
                    ViewBag.Roles = _context.Quyens.ToList();
                    return View("Index", _context.Taikhoans.ToList());
                }

                // Cập nhật các thuộc tính của tài khoản
                existingAccount.TaiKhoan = taikhoan.TaiKhoan;
                existingAccount.MatKhau = taikhoan.MatKhau;
                existingAccount.MaQuyen = taikhoan.MaQuyen;

                _context.Update(existingAccount);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật tài khoản thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Taikhoans.Any(t => t.MaTk == taikhoan.MaTk))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tài khoản để cập nhật!";
                    ViewBag.Roles = _context.Quyens.ToList();
                    return View("Index", _context.Taikhoans.ToList());
                }
                TempData["ErrorMessage"] = "Lỗi đồng bộ dữ liệu!";
                throw;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật tài khoản: {ex.Message}";
            }

            ViewBag.Roles = _context.Quyens.ToList();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Xóa tài khoản
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var taikhoan = _context.Taikhoans.Find(id);
            if (taikhoan == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản để xóa!";
                return RedirectToAction(nameof(Index));
            }
            _context.Taikhoans.Remove(taikhoan);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Xóa tài khoản thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}