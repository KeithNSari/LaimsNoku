using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    public class test6Model : PageModel
    {
        [BindProperty]
        public decimal TotalAmount { get; set; } = 0;
        public class MyDataModel
        {
            public int Id { get; set; }
            public decimal? Amount { get; set; }
        }
        [BindProperty]
        public List<MyDataModel> MyDataList { get; set; }

        public void OnGet()
        {
            MyDataList = new List<MyDataModel>();
            for(int i=0;i<6;i++)
            {
                MyDataModel myDataModel = new MyDataModel();
                myDataModel .Id = i;
                MyDataList .Add(myDataModel);
            }
            // Initialize MyDataList with data if needed
        }

        public IActionResult OnPost()
        {
            foreach (MyDataModel model in MyDataList) 
            {
                if (model.Amount != null)
                {
                    TotalAmount += Convert.ToDecimal(model.Amount);
                }
            } 
            return Page();
        }
    }
}
