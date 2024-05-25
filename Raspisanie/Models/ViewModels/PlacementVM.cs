using Microsoft.AspNetCore.Mvc.Rendering;

namespace Raspisanie.Models.ViewModels
{
    public class PlacementVM
    {
        public Placement Placement { get; set; }
        public IEnumerable<SelectListItem> GroupSelectList { get; set; }
        public IEnumerable<SelectListItem> PredmetSelectList { get; set; }
        public IEnumerable<SelectListItem> SecondPredmetSelectList { get; set; }
        public IEnumerable<SelectListItem> AuditoriaSelectList { get; set; }
        public IEnumerable<SelectListItem> TeacherSelectList { get; set; }
        public int NumOfPredmets { get; set; }
    }
}
