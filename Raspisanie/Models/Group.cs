using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raspisanie.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Course { get; set; }
        public string Specialnost { get; set; }
        public int AuditoriaId { get; set; }
        [ForeignKey("AuditoriaId")]
        public virtual Auditoria Auditoria { get; set; }
    }
}
