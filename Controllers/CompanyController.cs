using ABC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABC.Controllers
{
    /// <summary>
    /// Controller quản lý thông tin công ty
    /// </summary>
    [Authorize]
    public class CompanyController : Controller
    {
        private readonly QlpcthucTapContext _context;

        public CompanyController(QlpcthucTapContext context)
        {
            _context = context;
        }

        // GET: Company/Index
        public IActionResult Index(int page = 1)
        {
            // Số bản ghi trên mỗi trang
            const int pageSize = 5;
            // Tính số bản ghi cần bỏ qua
            int skip = (page - 1) * pageSize;

            // Lấy danh sách doanh nghiệp với phân trang
            var companies = _context.Doanhnghieps
                .Include(d => d.Nguoiphutraches) // Include để đếm số sinh viên qua người phụ trách
                .ThenInclude(n => n.Sinhviens)    // Include Sinhviens từ Nguoiphutrach
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            // Tổng số bản ghi để tính số trang
            int totalRecords = _context.Doanhnghieps.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Truyền dữ liệu phân trang vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(companies);
        }

        // GET: Company/Create
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Company/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create(Doanhnghiep doanhnghiep)
        {
            if (ModelState.IsValid)
            {
                _context.Add(doanhnghiep);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(doanhnghiep);
        }

        // GET: Company/Edit/5
        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doanhnghiep = _context.Doanhnghieps
                .FirstOrDefault(d => d.MaDn == id);

            if (doanhnghiep == null)
            {
                return NotFound();
            }

            // Kiểm tra nếu là yêu cầu AJAX, trả về JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    maDn = doanhnghiep.MaDn,
                    tenDn = doanhnghiep.TenDn,
                    diaChi = doanhnghiep.DiaChi ?? "N/A",
                    linhVuc = doanhnghiep.LinhVuc ?? "N/A"
                });
            }

            return View(doanhnghiep);
        }

        // POST: Company/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Doanhnghiep doanhnghiep)
        {
            if (string.IsNullOrEmpty(doanhnghiep.MaDn))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Tìm doanh nghiệp hiện tại trong database
                    var existingCompany = _context.Doanhnghieps.Find(doanhnghiep.MaDn);
                    if (existingCompany == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các thuộc tính của doanh nghiệp
                    existingCompany.TenDn = doanhnghiep.TenDn;
                    existingCompany.DiaChi = doanhnghiep.DiaChi;
                    existingCompany.LinhVuc = doanhnghiep.LinhVuc;

                    _context.Update(existingCompany);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoanhnghiepExists(doanhnghiep.MaDn))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(doanhnghiep);
        }

        // POST: Company/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            var doanhnghiep = _context.Doanhnghieps.Find(id);
            if (doanhnghiep != null)
            {
                _context.Doanhnghieps.Remove(doanhnghiep);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DoanhnghiepExists(string id)
        {
            return _context.Doanhnghieps.Any(e => e.MaDn == id);
        }
    }
}