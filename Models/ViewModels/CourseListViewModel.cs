using System.Collections.Generic;

namespace KT_LTWEB.Models.ViewModels
{
    public class CourseListViewModel
    {
        public List<Course> Courses { get; set; } = new();
        public string? SearchKeyword { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public HashSet<int> EnrolledCourseIds { get; set; } = new();
    }
}
