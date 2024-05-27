

using System.ComponentModel.DataAnnotations;

namespace Raspisanie.Models
{
    public class TGUser
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int ChatId { get; set; }

    }
}
