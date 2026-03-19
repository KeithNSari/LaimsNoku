using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LAIMS.Areas.NewBusiness.Pages.Membership
{
    public class FillIn2Model : PageModel
    {
        public class CheckboxViewModel
        {
            public int Id { get; set; }
            public string LabelName { get; set; }
            public bool IsChecked { get; set; }
        }
        public static IReadOnlyList<CheckboxViewModel> GetCourses()
        {
            return new List<CheckboxViewModel>
            {
        new CheckboxViewModel
        {
            Id = 1,
            LabelName = "Physics",
            IsChecked = true
        },
        new CheckboxViewModel
        {
            Id = 2,
            LabelName = "Chemistry",
            IsChecked = false
        },
        new CheckboxViewModel
        {
            Id = 3,
            LabelName = "Mathematics",
            IsChecked = true
        },
        new CheckboxViewModel
        {
            Id = 4,
            LabelName = "Biology",
            IsChecked = false
        },
    };
        } 
        public void OnGet()
        {

        }
    }
}
