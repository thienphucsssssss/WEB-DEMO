using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KT_LTWEB.Data;
using KT_LTWEB.Models;

namespace KT_LTWEB.Controllers
{
    [Authorize(Roles = "ADMIN,Admin")]
    public class AdminCoursesController : Controller
    {
        private const long MaxImageSize = 2 * 1024 * 1024;
        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        private readonly ApplicationDbContext _context;

        public AdminCoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdminCourses
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .Include(c => c.Category)
                .OrderByDescending(c => c.Id)
                .ToListAsync();
            return View(courses);
        }

        // GET: AdminCourses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: AdminCourses/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: AdminCourses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course, IFormFile? imageFile)
        {
            ValidateImageFile(imageFile);

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    course.Image = await SaveImageAsync(imageFile);
                }

                course.CreatedAt = DateTime.Now;
                course.IsActive = true;
                _context.Add(course);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tạo học phần mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", course.CategoryId);
            return View(course);
        }

        // GET: AdminCourses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", course.CategoryId);
            return View(course);
        }

        // POST: AdminCourses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course, IFormFile? imageFile)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            ValidateImageFile(imageFile);

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCourse = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (existingCourse == null)
                    {
                        return NotFound();
                    }

                    course.CreatedAt = existingCourse.CreatedAt;
                    course.UpdatedAt = DateTime.Now;

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        course.Image = await SaveImageAsync(imageFile);

                        // Delete old image file if it exists
                        if (!string.IsNullOrEmpty(existingCourse.Image) && existingCourse.Image.StartsWith("/images/courses/"))
                        {
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingCourse.Image.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
                    }
                    else
                    {
                        // Preserve existing image
                        course.Image = existingCourse.Image;
                    }

                    _context.Update(course);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật học phần thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", course.CategoryId);
            return View(course);
        }

        // GET: AdminCourses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: AdminCourses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                // Delete image file from server if exists
                if (!string.IsNullOrEmpty(course.Image) && course.Image.StartsWith("/images/courses/"))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", course.Image.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa học phần thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }

        private void ValidateImageFile(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return;
            }

            var extension = Path.GetExtension(imageFile.FileName);
            if (!AllowedImageExtensions.Contains(extension))
            {
                ModelState.AddModelError(string.Empty, "Ảnh minh họa chỉ hỗ trợ định dạng jpg, jpeg, png hoặc webp.");
            }

            if (!imageFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "File tải lên phải là hình ảnh hợp lệ.");
            }

            if (imageFile.Length > MaxImageSize)
            {
                ModelState.AddModelError(string.Empty, "Ảnh minh họa không được vượt quá 2MB.");
            }
        }

        private static async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "courses");

            Directory.CreateDirectory(uploadsDir);

            var filePath = Path.Combine(uploadsDir, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.CreateNew))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/images/courses/" + fileName;
        }
    }
}
