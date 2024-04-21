using Microsoft.AspNetCore.Mvc.Rendering;

namespace Raspisanie.Models.ViewModels
{
    public class GroupVM
    {
        public Group Group { get; set; }
        public IEnumerable<SelectListItem> AuditoriaSelectList { get; set; }
    }
}
