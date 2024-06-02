using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raspisanie.Models
{
    public class Placement
    {
        [Key]
        public int Id { get; set; }
        [DisplayName("Дата")]
        public string Date { get; set; }
        [DisplayName("Группа")]
        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; }
        [DisplayName("Предмет")]
        public int PredmetId { get; set; }
        [ForeignKey("PredmetId")]
        public virtual Predmet Predmet { get; set; }
        [DisplayName("Второй предмет")]
        public int SecondPredmetId { get; set; }
        [ForeignKey("SecondPredmetId")]
        public virtual Predmet SecondPredmet { get; set; }
        [DisplayName("Порядковый номер")]
        public int index { get; set; }
        [DisplayName("Аудитория")]
        public int AuditoriaId { get; set; }
        [ForeignKey("AuditoriaId")]
        public virtual Auditoria Auditoria { get; set; }
        [DisplayName("Вторая аудитория")]
        public int SecondAuditoriaId { get; set; }
        [ForeignKey("SecondAuditoriaId")]
        public virtual Auditoria SecondAuditoria { get; set; }
        [DisplayName("Преподователь")]
        public int TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public virtual Teacher Teacher { get; set; }
        [DisplayName("Второй преподователь")]
        public int SecondTeacherId { get; set; }
        [ForeignKey("SecondTeacherId")]
        public virtual Teacher SecondTeacher { get; set; }
        [DisplayName("Описание")]
        public string? Desc { get; set; }
        [DisplayName("Второе описание")]
        public string? SDesc { get; set; }
    }
}
