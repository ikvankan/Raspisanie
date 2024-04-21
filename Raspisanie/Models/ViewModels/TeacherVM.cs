using Microsoft.AspNetCore.Mvc.Rendering;

namespace Raspisanie.Models.ViewModels
{
    public class TeacherVM
    {
        public Teacher Teacher { get; set; }
        public IEnumerable<SelectListItem> AuditoriaSelectList { get; set; }
    }
}
