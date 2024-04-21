using Microsoft.AspNetCore.Mvc.Rendering;

namespace Raspisanie.Models.ViewModels
{
    public class PlacementVM
    {
        public Placement Placement { get; set; }
        public IEnumerable<SelectListItem> GroupSelectList { get; set; }
        public IEnumerable<SelectListItem> PredmetSelectList { get; set; }
    }
}
