using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Repositories.Premiums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    public class test8Model : PageModel
    {
        private readonly IPolicyBeneficiaryLineRepository _policyBeneficiaryLineRepository;
        public test8Model(IPolicyBeneficiaryLineRepository policyBeneficiaryLineRepository)
        {
            _policyBeneficiaryLineRepository = policyBeneficiaryLineRepository;
        }
        public List<PolicyBeneficiaryLineDetail> PolicyBeneficiaryLineDetails = new List<PolicyBeneficiaryLineDetail>();
        public IActionResult OnGet()
        {
            Guid policyID = Guid.Parse("f40e93ad-cb01-4778-a47a-838014b50538");
            PolicyBeneficiaryLineDetails = _policyBeneficiaryLineRepository.GetPolicyBeneficiaryLineDetailsByPolicyID(policyID);
            return Page();
        }
        public void OnPost()
        {
            PolicyBeneficiaryLine pbl = new PolicyBeneficiaryLine();
            string AddedBy = "tester";
            pbl.AddedOn = DateTime.Now;
            pbl.AddedBy = AddedBy;
            foreach (PolicyBeneficiaryLineDetail beneficiaryLine in PolicyBeneficiaryLineDetails)
            {
                pbl.ID = beneficiaryLine.LineId;
                pbl.Contribution = beneficiaryLine.Premium;
                pbl.Cover = beneficiaryLine.Cover;
                _policyBeneficiaryLineRepository.UpdatePolicyBeneficiaryLine(pbl);
            }
        }
    }
}
