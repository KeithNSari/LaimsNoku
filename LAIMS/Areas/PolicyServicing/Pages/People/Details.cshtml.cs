using LAIMS.Interfaces.Membership; 
using LAIMS.Models.Membership;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages; 

namespace LAIMS.Areas.PolicyServicing.Pages.People
{
    public class DetailsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemberRepository _memberRepository;
        public DetailsModel(IMemberRepository memberRepository, IMemberContactRepository memberContactRepository)
        {
            _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository)); 
        }
        public Member Member { get; set; }
        public Member StagingMember { get; set; }
        public IActionResult OnGet(Guid id)
        {
            Member = _memberRepository.GetMemberById(id);
            if (Member == null)
            {
                return NotFound();
            }
            StagingMember = _memberRepository.GetStagingMemberById(id); 
            return Page();
        }
    }
}
