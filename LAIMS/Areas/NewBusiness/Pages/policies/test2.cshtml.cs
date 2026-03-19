using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    public class test2Model : PageModel
    {
        public class YourRowModel
        {
            public string Column1 { get; set; }
            public string Column2 { get; set; }
            // Add more properties for each column
        }
        public IEnumerable<YourRowModel> TabularData { get; set; }

        public void OnGet()
        {
            // Fetch data from the database or another source
            TabularData = GetTabularData();
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
