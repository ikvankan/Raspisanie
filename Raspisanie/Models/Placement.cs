using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raspisanie.Models
{
    public class Placement
    {
        [Key]
        public int Id { get; set; }
        public string Date { get; set; }
        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; }
        public int PredmetId { get; set; }
        [ForeignKey("PredmetId")]
        public virtual Predmet Predmet { get; set; }
        public int SecondPredmetId { get; set; }
        [ForeignKey("SecondPredmetId")]
        public virtual Predmet SecondPredmet { get; set; }
        public int index { get; set; }
        public int AuditoriaId { get; set; }
        [ForeignKey("AuditoriaId")]
        public virtual Auditoria Auditoria { get; set; }
        public int SecondAuditoriaId { get; set; }
        [ForeignKey("SecondAuditoriaId")]
        public virtual Auditoria SecondAuditoria { get; set; }
        public int TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public virtual Teacher Teacher { get; set; }
        public int SecondTeacherId { get; set; }
        [ForeignKey("SecondTeacherId")]
        public virtual Teacher SecondTeacher { get; set; }
    }
}
