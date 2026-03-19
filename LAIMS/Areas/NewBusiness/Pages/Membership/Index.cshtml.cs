using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LAIMS.Areas.NewBusiness.Pages.Membership
{
    public class IndexModel : PageModel
    {
        private readonly IMemberRepository _memberRepository;

        public IndexModel(IMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
        }

        public List<Member> Members { get; set; }

        public void OnGet()
        {
            Members = _memberRepository.GetAllMembers();
        }
    }
}
