using System.ComponentModel.DataAnnotations;

namespace Raspisanie.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        public string TeacherName { get; set; }
        public int AuditoryID { get; set; }

    }
}
