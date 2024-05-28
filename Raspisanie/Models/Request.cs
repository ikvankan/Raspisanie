

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raspisanie.Models
{
    public class Request
    {
        [Key]
        public int Id { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public int TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public virtual Teacher Teacher { get; set; }
    }
}
