using System;

namespace KT_LTWEB.Models.ViewModels
{
    public class MyCoursesViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string Lecturer { get; set; } = string.Empty;
        public DateTime EnrollDate { get; set; }
    }
}
