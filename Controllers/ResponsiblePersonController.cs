using ABC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABC.Controllers
{
    public class ResponsiblePersonController : Controller
    {
        private readonly QlpcthucTapContext _context;

        public ResponsiblePersonController(QlpcthucTapContext context)
        {
            _context = context;
        }

        // GET: /ResponsiblePerson/Index
        public IActionResult Index(int page = 1)
        {
            // Số bản ghi trên mỗi trang
            const int pageSize = 5;
            // Tính số bản ghi cần bỏ qua
            int skip = (page - 1) * pageSize;

            // Lấy danh sách người phụ trách với phân trang
            var responsiblePersons = _context.Nguoiphutraches
                .Include(n => n.MaDnNavigation) // Include thông tin doanh nghiệp
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            // Tổng số bản ghi để tính số trang
            int totalRecords = _context.Nguoiphutraches.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Truyền dữ liệu phân trang vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            ViewBag.Companies = _context.Doanhnghieps.ToList(); // Để đổ vào dropdown
            return View(responsiblePersons);
        }

        // POST: /ResponsiblePerson/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Nguoiphutrach responsiblePerson)
        {
            if (ModelState.IsValid)
            {
                _context.Nguoiphutraches.Add(responsiblePerson);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Companies = _context.Doanhnghieps.ToList();
            var responsiblePersons = _context.Nguoiphutraches
                .Include(n => n.MaDnNavigation)
                .ToList();
            return View("Index", responsiblePersons);
        }

        // GET: /ResponsiblePerson/Edit/1
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var person = _context.Nguoiphutraches.Find(id);
            if (person == null)
            {
                return NotFound();
            }
            return Json(new
            {
                maNpt = person.MaNpt,
                tenNpt = person.TenNpt,
                chucVu = person.ChucVu,
                sdt = person.Sdt,
                email = person.Email,
                maDn = person.MaDn
            });
        }

        // POST: /ResponsiblePerson/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Nguoiphutrach responsiblePerson)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(responsiblePerson);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Nguoiphutraches.Any(n => n.MaNpt == responsiblePerson.MaNpt))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Companies = _context.Doanhnghieps.ToList();
            var responsiblePersons = _context.Nguoiphutraches
                .Include(n => n.MaDnNavigation)
                .ToList();
            return View("Index", responsiblePersons);
        }

        // POST: /ResponsiblePerson/Delete/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var person = _context.Nguoiphutraches.Find(id);
            if (person == null)
            {
                return NotFound();
            }
            _context.Nguoiphutraches.Remove(person);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}