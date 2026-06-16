using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KT_LTWEB.Models;

namespace KT_LTWEB.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.MigrateAsync();

            string[] roles = { "ADMIN", "STUDENT" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "admin@example.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "ADMIN");
                }
            }
            else if (!await userManager.IsInRoleAsync(adminUser, "ADMIN"))
            {
                await userManager.AddToRoleAsync(adminUser, "ADMIN");
            }

            var categories = new Dictionary<string, Category>
            {
                ["Lập trình"] = await GetOrCreateCategoryAsync(context, "Lập trình"),
                ["Cơ sở dữ liệu"] = await GetOrCreateCategoryAsync(context, "Cơ sở dữ liệu"),
                ["Mạng máy tính"] = await GetOrCreateCategoryAsync(context, "Mạng máy tính"),
                ["Trí tuệ nhân tạo"] = await GetOrCreateCategoryAsync(context, "Trí tuệ nhân tạo"),
                ["Thiết kế web"] = await GetOrCreateCategoryAsync(context, "Thiết kế web")
            };

            await context.SaveChangesAsync();

            var courseSeeds = new[]
            {
                new CourseSeed(
                    "Lập trình C# cơ bản",
                    3,
                    "Nguyễn Văn A",
                    "Lập trình",
                    "/images/courses/csharp.png",
                    "Học phần này cung cấp các kiến thức cơ bản về ngôn ngữ lập trình C# và lập trình hướng đối tượng."),
                new CourseSeed(
                    "Lập trình Web ASP.NET Core MVC",
                    4,
                    "Trần Thị B",
                    "Thiết kế web",
                    "/images/courses/aspnet.png",
                    "Học phần trang bị kiến thức xây dựng ứng dụng web hiện đại sử dụng công nghệ ASP.NET Core MVC của Microsoft."),
                new CourseSeed(
                    "Cơ sở dữ liệu SQL Server",
                    3,
                    "Phạm Văn C",
                    "Cơ sở dữ liệu",
                    "/images/courses/sql.png",
                    "Học phần giới thiệu về cơ sở dữ liệu quan hệ, ngôn ngữ truy vấn cấu trúc SQL và hệ quản trị CSDL SQL Server."),
                new CourseSeed(
                    "Nhập môn Trí tuệ nhân tạo",
                    3,
                    "Lê Văn D",
                    "Trí tuệ nhân tạo",
                    "/images/courses/ai.png",
                    "Tìm hiểu các khái niệm cơ bản về AI, học máy (Machine Learning) và các thuật toán tìm kiếm phổ biến."),
                new CourseSeed(
                    "Quản trị Mạng máy tính",
                    3,
                    "Hoàng Thị E",
                    "Mạng máy tính",
                    "/images/courses/network.png",
                    "Học phần giới thiệu cấu trúc mạng, các mô hình TCP/IP, OSI và cách cấu hình mạng cơ bản."),
                new CourseSeed(
                    "Lập trình Python nâng cao",
                    3,
                    "Vũ Văn F",
                    "Lập trình",
                    "/images/courses/python.png",
                    "Tìm hiểu các cấu trúc dữ liệu nâng cao, lập trình bất đồng bộ và xử lý dữ liệu với Python."),
                new CourseSeed(
                    "Thiết kế giao diện UI/UX",
                    2,
                    "Đặng Thị G",
                    "Thiết kế web",
                    "/images/courses/uiux.png",
                    "Cung cấp nguyên lý thiết kế giao diện người dùng đẹp mắt, nâng cao trải nghiệm người dùng (UX).")
            };

            foreach (var seed in courseSeeds)
            {
                var course = await context.Courses.FirstOrDefaultAsync(c => c.Image == seed.Image || c.Name == seed.Name);
                var categoryId = categories[seed.CategoryName].Id;

                if (course == null)
                {
                    context.Courses.Add(new Course
                    {
                        Name = seed.Name,
                        Credits = seed.Credits,
                        Lecturer = seed.Lecturer,
                        CategoryId = categoryId,
                        Image = seed.Image,
                        Description = seed.Description,
                        CreatedAt = DateTime.Now
                    });

                    continue;
                }

                if (LooksCorrupted(course.Name, course.Lecturer, course.Description, course.Category?.Name))
                {
                    course.Name = seed.Name;
                    course.Credits = seed.Credits;
                    course.Lecturer = seed.Lecturer;
                    course.CategoryId = categoryId;
                    course.Image = seed.Image;
                    course.Description = seed.Description;
                    course.UpdatedAt = DateTime.Now;
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task<Category> GetOrCreateCategoryAsync(ApplicationDbContext context, string name)
        {
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == name);
            if (category != null)
            {
                return category;
            }

            category = new Category { Name = name };
            context.Categories.Add(category);
            return category;
        }

        private static bool LooksCorrupted(params string?[] values)
        {
            return values.Any(value =>
                !string.IsNullOrEmpty(value)
                && (value.Contains('�')
                    || value.Contains("Ã", StringComparison.Ordinal)
                    || value.Contains("Ä", StringComparison.Ordinal)
                    || value.Contains("áº", StringComparison.Ordinal)
                    || value.Contains("á»", StringComparison.Ordinal)
                    || value.Contains("Æ", StringComparison.Ordinal)
                    || value.Contains("Å", StringComparison.Ordinal)
                    || value.Contains("?", StringComparison.Ordinal)));
        }

        private sealed record CourseSeed(
            string Name,
            int Credits,
            string Lecturer,
            string CategoryName,
            string Image,
            string Description);
    }
}
