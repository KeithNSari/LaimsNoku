using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Text.Encodings.Web;

namespace LAIMS.Areas.NewBusiness.Pages.Membership
{
    [Authorize(Roles = "New Business Initiator, New Business Approver")]
    public class SearchMembersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemberRepository _memberRepository;
        //public string SearchTerm;
       
        public SearchMembersModel (UserManager<ApplicationUser> userManager,
            IMemberRepository memberRepository)
        {
            _userManager = userManager;
            _memberRepository = memberRepository; 
        }
        public string SearchPageUrl;
        [BindProperty]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;

        public DataTable MembersDT;
        public IActionResult OnGetSearch(string? search)
        {
            try
            {
                SearchTerm = search;
                SearchPageUrl = "SearchMembers";
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    MembersDT = _memberRepository.Search(SearchTerm);
                    ResultsCount = MembersDT.Rows.Count;
                }
                return Page();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl ="" });
            }
          
		}
        public IActionResult OnPost() 
        {
            try
            {
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    MembersDT = _memberRepository.Search(SearchTerm);
                    ResultsCount = MembersDT.Rows.Count;
                }
                return Page();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = "" });
            }
        }
    }
}
