using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static LAIMS.Areas.NewBusiness.Pages.policies.test2Model;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    public class test3Model : PageModel
    {
        public IEnumerable<YourRowModel> TabularData { get; set; }

        public void OnGet()
        {
            // Fetch data from the database or another source
            TabularData = GetTabularData();
        }

        public IActionResult OnGetLoadData()
        {
            var data = GetTabularData();
            return new JsonResult(data);
        }
        // Simulated method to get tabular data
        private IEnumerable<YourRowModel> GetTabularData()
        {
            // Replace this with your actual data retrieval logic
            var data = new List<YourRowModel>
        {
            new YourRowModel { Column1 = "Value1A", Column2 = "Value1B" },
            new YourRowModel { Column1 = "Value2A", Column2 = "Value2B" },
            // Add more rows as needed
        };

            return data;
        }
    }
}
