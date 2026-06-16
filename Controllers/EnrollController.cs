using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KT_LTWEB.Data;
using KT_LTWEB.Models;
using KT_LTWEB.Models.ViewModels;

namespace KT_LTWEB.Controllers
{
    [Authorize(Roles = "STUDENT,Student")]
    public class EnrollController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EnrollController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null || !course.IsActive)
            {
                TempData["Error"] = "Học phần không tồn tại hoặc đã đóng đăng ký.";
                return RedirectToAction("Index", "Home");
            }

            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (alreadyEnrolled)
            {
                TempData["Error"] = "Bạn đã đăng ký học phần này rồi.";
                return RedirectToAction("Index", "Home");
            }

            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrollDate = DateTime.Now
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đăng ký thành công học phần: {course.Name}!";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (enrollment == null)
            {
                TempData["Error"] = "Bạn chưa đăng ký học phần này.";
                return RedirectToAction("MyCourses");
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            var course = await _context.Courses.FindAsync(courseId);
            TempData["Success"] = $"Đã hủy đăng ký học phần: {(course != null ? course.Name : "")}.";

            var referer = Request.Headers.Referer.ToString();
            if (!string.IsNullOrEmpty(referer) && referer.Contains("/Enroll/MyCourses", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(MyCourses));
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> MyCourses()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var myCourses = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.UserId == userId)
                .Select(e => new MyCoursesViewModel
                {
                    CourseId = e.CourseId,
                    CourseName = e.Course != null ? e.Course.Name : "Unknown",
                    Credits = e.Course != null ? e.Course.Credits : 0,
                    Lecturer = e.Course != null ? e.Course.Lecturer : "Unknown",
                    EnrollDate = e.EnrollDate
                })
                .OrderByDescending(e => e.EnrollDate)
                .ToListAsync();

            return View(myCourses);
        }
    }
}
