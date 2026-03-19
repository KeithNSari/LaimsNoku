using LAIMS.Interfaces.Membership;
using LAIMS.Models.Membership;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;


namespace LAIMS.Areas.NewBusiness.Pages.Membership
{
    public class ConfirmIdentityModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemberRepository _memberRepository;

        public ConfirmIdentityModel(UserManager<ApplicationUser> userManager, IMemberRepository memberRepository)
        {
            _userManager = userManager;
            _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
        }

        [BindProperty]
        public Member Member { get; set; }
        [BindProperty]
        public Guid MemberID { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        public void OnGet(Guid id)
        {
            try
            {
                LoadPage(id);
            }
            catch (Exception ex)
            {
                RedirectToPage("Create");
            }           
        }
        private void LoadPage(Guid id)
        {
            MemberID = id;
            Member = _memberRepository.GetMemberById(id);

            if (Member == null)
            {
                RedirectToPage("Create");
            }
        }
        public IActionResult OnPost(Guid id)
        {
            ReturnUrl = Request.Path + Request.QueryString;
            try
            {
                MemberID = id;
                if (string.IsNullOrEmpty(Member.NationalIDConfirm) && (string.IsNullOrEmpty(Member.BirthCertificateConfirm) && (string.IsNullOrEmpty(Member.PassportConfirm))))
                {
                    throw new Exception("At least one form of Identity is required!");
                }
                if (string.IsNullOrEmpty(Member.NationalIDConfirm)) Member.NationalIDConfirm = string.Empty;
                if (Member.NationalIDConfirm != string.Empty)
                {
                    string pattern1 = @"(^\d{2})-(\d{4,7})\s([A-Za-z]{1}\s(\d{2}$))";
                    string pattern2 = @"(^\d{2})-(\d{4,7})([A-Za-z]{1}(\d{2}$))";
                    Regex regex1 = new Regex(pattern1);
                    Regex regex2 = new Regex(pattern2);
                    if (!(regex1.IsMatch(Member.NationalIDConfirm) || regex2.IsMatch(Member.NationalIDConfirm)))
                    {
                        throw new Exception("Invalid ID format");
                    }
                }
                if (string.IsNullOrEmpty(Member.PassportConfirm)) Member.PassportConfirm = string.Empty;
                if (string.IsNullOrEmpty(Member.BirthCertificateConfirm)) Member.BirthCertificateConfirm = string.Empty;                
                if (_memberRepository.ConfirmID(id, Member.NationalIDConfirm, Member.PassportConfirm, Member.BirthCertificateConfirm))
                {
                    return Redirect("MemberProfile?id=" + id.ToString());
                }
                else
                { 
                    return RedirectToPage("/Error", new { errorMessage = "Your entries do not match with those in the create page!.", returnUrl = ReturnUrl });

                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            } 
        }
    }
}
