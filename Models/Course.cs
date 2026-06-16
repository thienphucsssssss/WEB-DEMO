using System.ComponentModel.DataAnnotations;

namespace KT_LTWEB.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên học phần không được để trống")]
        [StringLength(200, ErrorMessage = "Tên học phần không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        public string? Image { get; set; }

        [Required(ErrorMessage = "Số tín chỉ không được để trống")]
        [Range(1, 10, ErrorMessage = "Số tín chỉ phải từ 1 đến 10")]
        public int Credits { get; set; }

        [Required(ErrorMessage = "Tên giảng viên không được để trống")]
        [StringLength(150, ErrorMessage = "Tên giảng viên không được vượt quá 150 ký tự")]
        public string Lecturer { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn danh mục học phần")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
