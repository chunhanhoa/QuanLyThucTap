using System.Diagnostics;
using ABC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABC.Controllers
{
    /// <summary>
    /// Controller quản lý trang chủ và các chức năng chung
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QlpcthucTapContext _context;

        /// <summary>
        /// Constructor của HomeController
        /// </summary>
        /// <param name="logger">Logger để ghi log</param>
        /// <param name="context">DbContext để truy cập database</param>
        public HomeController(ILogger<HomeController> logger, QlpcthucTapContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Hiển thị trang chủ
        /// </summary>
        /// <param name="page">Số trang hiện tại</param>
        /// <returns>View trang chủ với thông tin người dùng và dữ liệu từ database</returns>
        [Authorize]
        public IActionResult Index(int page = 1)
        {
            // Lấy thông tin username từ claim
            string username = User.Identity?.Name ?? "Unknown";
            
            // Lấy thông tin role từ claim
            string role = User.Claims
                .FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? "Unknown";
            
            ViewBag.Username = username;
            ViewBag.Role = role;
            
            // Các thống kê
            ViewBag.TotalStudents = _context.Sinhviens?.Count() ?? 0;
            ViewBag.TotalLecturers = _context.Giangviens?.Count() ?? 0;
            ViewBag.TotalCompanies = _context.Doanhnghieps?.Count() ?? 0;
            ViewBag.TotalProjects = _context.Detais?.Count() ?? 0;

            // Số bản ghi trên mỗi trang
            const int pageSize = 5;
            // Tính số bản ghi cần bỏ qua
            int skip = (page - 1) * pageSize;

            // Lấy dữ liệu tiến độ thực tập từ SINHVIEN và DETAI với phân trang
            var internshipProgress = _context.Sinhviens
                .Include(s => s.MaDtNavigation)
                .Select(s => new
                {
                    StudentId = s.MaSv ?? "N/A",
                    Name = s.TenSv ?? "Chưa có tên", 
                    Topic = s.MaDtNavigation != null ? s.MaDtNavigation.TenDt : "Chưa có đề tài"
                })
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            // Tổng số bản ghi để tính số trang
            int totalRecords = _context.Sinhviens.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Truyền dữ liệu phân trang vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            _logger.LogInformation($"Username: {ViewBag.Username}, Role: {ViewBag.Role}");
            return View(internshipProgress);
        }

        /// <summary>
        /// API tìm kiếm giảng viên theo mã
        /// </summary>
        /// <param name="code">Mã giảng viên</param>
        /// <returns>Thông tin giảng viên dạng JSON</returns>
        [HttpGet]
        public IActionResult SearchLecturer(string code)
        {
            var lecturer = _context.Giangviens
                .FirstOrDefault(g => g.MaGv == code);

            if (lecturer == null)
            {
                return Json(new { success = false, message = "Không tìm thấy giảng viên" });
            }

            var result = new
            {
                LecturerCode = lecturer.MaGv,
                Name = lecturer.TenGv,
                Subject = lecturer.BoMon
            };
            return Json(new { success = true, data = result });
        }

        /// <summary>
        /// Hiển thị trang chính sách bảo mật
        /// </summary>
        /// <returns>View trang chính sách bảo mật</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Xử lý đăng xuất và chuyển hướng đến trang đăng nhập
        /// </summary>
        /// <returns>Chuyển hướng đến trang đăng nhập</returns>
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa Session
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Hiển thị trang lỗi
        /// </summary>
        /// <returns>View trang lỗi với thông tin lỗi</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}