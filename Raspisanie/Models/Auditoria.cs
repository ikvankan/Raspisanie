using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Raspisanie.Models
{
    public class Auditoria
    {
        [Key]
        public int Id { get; set; }
        [DisplayName("Номер аудитории")]
        public string AuditoryName { get; set; }
        [DisplayName("Наличие проектора / мультиборда")]
        public bool Proektor { get; set; }
        [DisplayName("Лаборатория")]
        public bool LekciaOrPractica { get; set; }

    }
}
