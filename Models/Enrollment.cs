using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace KT_LTWEB.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public IdentityUser? User { get; set; }

        [Required]
        public int CourseId { get; set; }

        public Course? Course { get; set; }

        public DateTime EnrollDate { get; set; } = DateTime.Now;
    }
}
