using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static LAIMS.Areas.NewBusiness.Pages.policies.test2Model;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    public class test4Model : PageModel
    {
        public IEnumerable<YourRowModel> TabularData { get; set; }

        public void OnGet()
        {
            // Fetch data from the database or another source
            //TabularData = GetTabularData();
        }

        public IActionResult OnGetLoadData(string id)
        {
            // Use the id parameter in your logic to fetch data
            var data = GetTabularData(id);
            return new JsonResult(data);
        }

        private IEnumerable<YourRowModel> GetTabularData(string id)
        {
            // Replace this with your actual data retrieval logic
            // Use the id parameter in your query or logic
            var data = new List<YourRowModel>
            {
              new YourRowModel { Column1 = "Value1A" + id, Column2 = "Value1B" + id },
              new YourRowModel { Column1 = "Value2A" + id, Column2 = "Value2B" + id },
             // Add more rows as needed
            };
            return data;
        }
    }
}
