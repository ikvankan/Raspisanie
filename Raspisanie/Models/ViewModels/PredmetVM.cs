using Microsoft.AspNetCore.Mvc.Rendering;

namespace Raspisanie.Models.ViewModels
{
    public class PredmetVM
    {
        public Predmet Predmet { get; set; }
        public IEnumerable<SelectListItem> TeacherSelectList { get; set; }
        public IEnumerable<SelectListItem> GroupSelectList { get; set; }
    }
}
