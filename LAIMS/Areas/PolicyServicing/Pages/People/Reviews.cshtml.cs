using LAIMS.Interfaces.Membership;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.PolicyServicing.Pages.People
{
   [Authorize(Roles = "Policy Servicing Initiator")]
    public class ReviewsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemberRepository _memberRepository;
        //public string SearchTerm;
        public string SearchPageUrl;
        public ReviewsModel(UserManager<ApplicationUser> userManager,
            IMemberRepository memberRepository)
        {
            _userManager = userManager;
            _memberRepository = memberRepository;
        }
        [BindProperty]
        public string SearchTerm { get; set; }
        [BindProperty]
        public int ResultsCount { get; set; } = 0;

        public DataTable MembersDT;
        public void OnGetSearch(string? search)
        {
            SearchTerm = search;
            SearchPageUrl = "Reviews";
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                MembersDT = _memberRepository.SearchStaging(SearchTerm);
                ResultsCount = MembersDT.Rows.Count;
            }
        }
        public void OnGet()
        {
            MembersDT = _memberRepository.GetStaging();
            ResultsCount = MembersDT.Rows.Count;
        }
    }
}
