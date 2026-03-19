using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LAIMS.Areas.Claims.Pages
{
    public class TestPagePaymentsModel : PageModel
    {
        public class Claimant
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string NationalID { get; set; }
            public int BankId { get; set; }
            public string AccountNo { get; set; }
            public decimal Amount { get; set; }
        }

        public class Bank
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public List<Claimant> Claimants { get; set; }
        public List<Bank> Banks { get; set; }
        public void OnGet()
        {
            Claimants = new List<Claimant>
            {
            new Claimant { Id = 1, Name = "John Doe", NationalID = "12345678" },
            new Claimant { Id = 2, Name = "Jane Smith", NationalID = "87654321" }
            };

            Banks = new List<Bank>
            {
            new Bank { Id = 1, Name = "Bank A" },
            new Bank { Id = 2, Name = "Bank B" },
            new Bank { Id = 3, Name = "Bank C" }
            };
        }
        public void OnPost(List<Claimant> claimants)
        {
            foreach(Claimant claimant in claimants)
            {

            }
        }
    }
}
