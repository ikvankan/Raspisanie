using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raspisanie.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }
        [DisplayName("Имя")]
        public string TeacherName { get; set; }
        [DisplayName("Аудитория")]
        public int AuditoryId { get; set; }

        [ForeignKey ("AuditoryId")]
        public virtual Auditoria Auditoria { get; set; }
        public int? ChatId { get; set; }
    }
}
