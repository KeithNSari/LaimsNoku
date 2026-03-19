using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    public class test10Model : PageModel
    {
        public class Bank
        {
           public int value { get; set; }
           public  string text { get; set; }
        }
        [BindProperty]
        public int BankId { get; set; }
        [BindProperty]
        public string AccountNo { get; set; }
        public void OnGet()
        {
        }

        public IActionResult OnGetSearchProviders(string searchValue)
        {
            List<Bank> banks = new();
            //{
            //     new Bank {value=1,text=searchValue },
            //     new Bank {value=2,text="CBZ"}
            //};
            return new JsonResult(banks);
        }
        public void OnPostSaveData()
        {
            string acNo = AccountNo;
        }
    }
}
