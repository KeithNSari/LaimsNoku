using LAIMS.Interfaces.BusinessRules;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Questionnaires;
using LAIMS.Models.BusinessRules;
using LAIMS.Models.Questionnaires;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace LAIMS.Areas.PolicyServicing.Pages.Policies.Underwriting
{
    public class PBQuestionnairesModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyRepository _policyRepository;
        private readonly IQuestionnaireRepository _questionnaireRepository;
        private readonly IQuestionnaireQsnsRepository _questionnaireQsnsRepository;
        private readonly IQuestionnaireResponseLineRepository _questionnaireResponseLineRepository;
        private readonly IQuestionnaireResponseRepository _questionnaireResponseRepository;
        private readonly IBusinessRuleRepository _businessRuleRepository;
        public PBQuestionnairesModel(UserManager<ApplicationUser> userManager,
            IPolicyRepository policyRepository, IQuestionnaireRepository questionnaireRepository,
            IQuestionnaireQsnsRepository questionnaireQsnsRepository,
            IQuestionnaireResponseRepository questionnaireResponseRepository,
            IQuestionnaireResponseLineRepository questionnaireResponseLineRepository,
            IBusinessRuleRepository businessRuleRepository)
        {
            _userManager = userManager;
            _policyRepository = policyRepository;
            _questionnaireRepository = questionnaireRepository;
            _questionnaireQsnsRepository = questionnaireQsnsRepository;
            _questionnaireResponseLineRepository = questionnaireResponseLineRepository;
            _questionnaireResponseRepository = questionnaireResponseRepository;
            _businessRuleRepository = businessRuleRepository;
        }
        [BindProperty(SupportsGet = true)]
        public string SourceUrl { get; set; }
        [BindProperty(SupportsGet = true)]
        public string RootPath { get; set; }
        public Guid MemberID { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public Guid PolicyID { get; set; }
        public List<QuestionnaireResponse> QuestionnaireResponses { get; set; }
        public void OnGet(Guid id, Guid policyTypeid, Guid policyid)
        {
            PolicyID = policyid;
            MemberID = id;
            PolicyTypeID = policyTypeid;
            RootPath = Request.Path;
            SourceUrl = RootPath + Request.QueryString;
            _policyRepository.UpdatePolicyStage(PolicyID, 5);
            QuestionnaireResponses = _questionnaireResponseRepository.GetQuestionnaireResponseHeaders(policyid);
        }
        public IActionResult OnPostNext(Guid id, Guid policyTypeid, Guid policyid)
        {
            //string addedBy = _userManager.GetUserId(User).ToString();
            //_policyRepository.UpdatePolicyStatus(policyid, 4, string.Empty, addedBy);
            return Redirect("/PolicyServicing/Policies/PPReviews");
        }
        public IActionResult OnPost(Guid id, Guid policyTypeid, Guid policyid)
        {
            BusinessRulesParameters newBusinessParameters = new BusinessRulesParameters();
            newBusinessParameters.PolicyTypeID = PolicyTypeID;
            newBusinessParameters.PolicyID = PolicyID;
            newBusinessParameters.ProposerUID = id;
            StatusReport statusReport = _businessRuleRepository.CheckRules(PolicyTypeID, "Questionnaires", newBusinessParameters);
            if (statusReport.SuccessStatus == 0)
            {
                throw new Exception(statusReport.StatusMessage);
            }
            return Redirect("PBQuestionnaires?id=" + id + "&policytypeid=" + policyTypeid + "&policyid=" + policyid);
        }
    }
}
