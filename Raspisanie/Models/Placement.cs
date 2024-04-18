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
        public int index { get; set; }
    }
}
