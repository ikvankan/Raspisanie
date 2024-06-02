using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raspisanie.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        [DisplayName("Номер группы")]
        public string Name { get; set; }
        [DisplayName("Курс")]
        public int Course { get; set; }
        [DisplayName("Сппециальность")]
        public string Specialnost { get; set; }
        [DisplayName("Аудитория")]
        public int AuditoriaId { get; set; }
        [ForeignKey("AuditoriaId")]
        public virtual Auditoria Auditoria { get; set; }
        [DisplayName("Куратор")]
        public int TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public virtual Teacher Teacher { get; set; }
        public string? ChatId { get; set; }
    }
}
