using ABC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABC.Controllers
{
    public class EvaluationController : Controller
    {
        private readonly QlpcthucTapContext _context;

        public EvaluationController(QlpcthucTapContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1)
        {
            // Số bản ghi trên mỗi trang
            const int pageSize = 5;
            // Tính số bản ghi cần bỏ qua
            int skip = (page - 1) * pageSize;

            // Lấy danh sách phiếu đánh giá với phân trang
            var evaluations = _context.Phieudanhgia
                .Include(p => p.MaGvNavigation)
                .Include(p => p.MaSvNavigation)
                .Include(p => p.CtDanhgia)
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            // Tổng số bản ghi để tính số trang
            int totalRecords = _context.Phieudanhgia.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Truyền dữ liệu phân trang vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            ViewBag.Giangviens = _context.Giangviens.ToList();
            ViewBag.Sinhviens = _context.Sinhviens.ToList();
            return View(evaluations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Phieudanhgium phieudanhgium)
        {
            if (ModelState.IsValid)
            {
                _context.Phieudanhgia.Add(phieudanhgium);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            var evaluations = _context.Phieudanhgia
                .Include(p => p.MaGvNavigation)
                .Include(p => p.MaSvNavigation)
                .Include(p => p.CtDanhgia)
                .ToList();
            ViewBag.Giangviens = _context.Giangviens.ToList();
            ViewBag.Sinhviens = _context.Sinhviens.ToList();
            return View("Index", evaluations);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var phieudanhgium = _context.Phieudanhgia
                .Include(p => p.MaGvNavigation)
                .Include(p => p.MaSvNavigation)
                .Include(p => p.CtDanhgia)
                .FirstOrDefault(p => p.MaPdg == id);

            if (phieudanhgium == null)
            {
                return NotFound();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    maPdg = phieudanhgium.MaPdg,
                    ngayLap = phieudanhgium.NgayLap?.ToString("yyyy-MM-dd"),
                    nhanXet = phieudanhgium.NhanXet ?? "N/A",
                    maGv = phieudanhgium.MaGv,
                    maSv = phieudanhgium.MaSv,
                    tenGv = phieudanhgium.MaGvNavigation?.TenGv ?? "N/A",
                    tenSv = phieudanhgium.MaSvNavigation?.TenSv ?? "N/A",
                    diemTb = phieudanhgium.CtDanhgia.Any() ? phieudanhgium.CtDanhgia.Average(ct => ct.DiemSo ?? 0m) : 0m,
                    xepLoai = CalculateRanking(phieudanhgium.CtDanhgia.Any() ? phieudanhgium.CtDanhgia.Average(ct => ct.DiemSo ?? 0m) : 0m)
                });
            }

            ViewBag.Giangviens = _context.Giangviens.ToList();
            ViewBag.Sinhviens = _context.Sinhviens.ToList();
            return View(phieudanhgium);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Phieudanhgium phieudanhgium)
        {
            if (phieudanhgium.MaPdg <= 0)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Tìm phiếu đánh giá hiện tại trong database
                    var existingEvaluation = _context.Phieudanhgia.Find(phieudanhgium.MaPdg);
                    if (existingEvaluation == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các thuộc tính của phiếu đánh giá
                    existingEvaluation.NgayLap = phieudanhgium.NgayLap;
                    existingEvaluation.NhanXet = phieudanhgium.NhanXet;
                    existingEvaluation.MaGv = phieudanhgium.MaGv;
                    existingEvaluation.MaSv = phieudanhgium.MaSv;

                    _context.Update(existingEvaluation);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhieudanhgiumExists(phieudanhgium.MaPdg))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var evaluations = _context.Phieudanhgia
                .Include(p => p.MaGvNavigation)
                .Include(p => p.MaSvNavigation)
                .Include(p => p.CtDanhgia)
                .ToList();
            ViewBag.Giangviens = _context.Giangviens.ToList();
            ViewBag.Sinhviens = _context.Sinhviens.ToList();
            return View("Index", evaluations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var phieudanhgium = _context.Phieudanhgia.Find(id);
            if (phieudanhgium == null)
            {
                return NotFound();
            }
            _context.Phieudanhgia.Remove(phieudanhgium);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var details = _context.CtDanhgia
                .Where(ct => ct.MaPdg == id)
                .ToList();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(details.Select(ct => new
                {
                    maPdg = ct.MaPdg,
                    maTuan = ct.MaTuan,
                    diemSo = ct.DiemSo ?? 0m,
                    ngayLap = ct.NgayLap?.ToString("yyyy-MM-dd") ?? "N/A",
                    ghiChu = ct.GhiChu ?? "N/A"
                }));
            }

            return View(details);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditWeek(CtDanhgium ctDanhgium)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            try
            {
                var existingDetail = _context.CtDanhgia
                    .FirstOrDefault(ct => ct.MaPdg == ctDanhgium.MaPdg && ct.MaTuan == ctDanhgium.MaTuan);

                if (existingDetail != null)
                {
                    // Cập nhật chi tiết tuần hiện có
                    existingDetail.DiemSo = ctDanhgium.DiemSo;
                    existingDetail.NgayLap = ctDanhgium.NgayLap;
                    existingDetail.GhiChu = ctDanhgium.GhiChu;
                    _context.Update(existingDetail);
                }
                else
                {
                    // Thêm chi tiết tuần mới
                    _context.CtDanhgia.Add(ctDanhgium);
                }
                _context.SaveChanges();

                // Lấy lại danh sách chi tiết để cập nhật giao diện
                var updatedDetails = _context.CtDanhgia
                    .Where(ct => ct.MaPdg == ctDanhgium.MaPdg)
                    .Select(ct => new
                    {
                        maPdg = ct.MaPdg,
                        maTuan = ct.MaTuan,
                        diemSo = ct.DiemSo ?? 0m,
                        ngayLap = ct.NgayLap,
                        ghiChu = ct.GhiChu ?? "N/A"
                    })
                    .ToList();

                return Json(new { success = true, details = updatedDetails });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteWeek(int maPdg, int maTuan)
        {
            try
            {
                var ctDanhgia = _context.CtDanhgia
                    .FirstOrDefault(ct => ct.MaPdg == maPdg && ct.MaTuan == maTuan);
                if (ctDanhgia == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy chi tiết tuần để xóa." });
                }

                _context.CtDanhgia.Remove(ctDanhgia);
                _context.SaveChanges();

                // Lấy lại danh sách chi tiết để cập nhật giao diện
                var updatedDetails = _context.CtDanhgia
                    .Where(ct => ct.MaPdg == maPdg)
                    .Select(ct => new
                    {
                        maPdg = ct.MaPdg,
                        maTuan = ct.MaTuan,
                        diemSo = ct.DiemSo ?? 0m,
                        ngayLap = ct.NgayLap ,
                        ghiChu = ct.GhiChu ?? "N/A"
                    })
                    .ToList();

                return Json(new { success = true, details = updatedDetails });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        private bool PhieudanhgiumExists(int id)
        {
            return _context.Phieudanhgia.Any(e => e.MaPdg == id);
        }

        private string CalculateRanking(decimal averageScore)
        {
            if (averageScore < 0) return "Không xác định";
            if (averageScore >= 8.5m) return "Giỏi";
            if (averageScore >= 7.0m) return "Khá";
            if (averageScore >= 5.0m) return "Trung bình";
            return "Yếu";
        }
    }
}