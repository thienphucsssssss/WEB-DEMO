using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KT_LTWEB.Data;
using KT_LTWEB.Models.ViewModels;

namespace KT_LTWEB.Controllers
{
    [Authorize(Roles = "ADMIN,Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var studentRoleNames = new[] { "STUDENT", "Student" };
            var totalStudents = await _context.UserRoles
                .Join(
                    _context.Roles.Where(r => r.Name != null && studentRoleNames.Contains(r.Name)),
                    userRole => userRole.RoleId,
                    role => role.Id,
                    (userRole, role) => userRole.UserId)
                .Distinct()
                .CountAsync();

            var viewModel = new DashboardViewModel
            {
                TotalCourses = await _context.Courses.CountAsync(),
                TotalStudents = totalStudents,
                TotalEnrollments = await _context.Enrollments.CountAsync()
            };

            // Get latest enrollments for premium dashboard content
            var latestEnrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.User)
                .OrderByDescending(e => e.EnrollDate)
                .Take(5)
                .ToListAsync();

            ViewBag.LatestEnrollments = latestEnrollments;

            return View(viewModel);
        }
    }
}
