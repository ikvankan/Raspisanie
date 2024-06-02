using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raspisanie.Models
{
    public class Predmet
    {
        [Key]
        public int Id { get; set; }
        [DisplayName("Название")]
        public string PredmetName { get; set; }
        [DisplayName("Преподователь")]
        public int TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public virtual Teacher Teacher { get; set; }
        [DisplayName("Второй преподователь")]
        public int SecondTeacherId { get; set; }
        [ForeignKey("SecondTeacherId")]
        public virtual Teacher SecondTeacher { get; set; }
        [DisplayName("Часы")]
        public int Hours { get; set; }
        [DisplayName("Группа")]
        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; }
        [DisplayName("Лабораторная")]
        public bool Laboratory { get; set; }
        [DisplayName("Участие в генерации")]
        public bool GEN { get; set; }
        [DisplayName("Участие аудитории в проверке")]
        public bool NoAud { get; set; }
    }
}
