using System.ComponentModel.DataAnnotations;

namespace Raspisanie.Models
{
    public class Auditoria
    {
        [Key]
        public int Id { get; set; }
        public string AuditoryName { get; set; }
        public bool Proektor { get; set; }
        public bool LekciaOrPractica { get; set; }

    }
}
