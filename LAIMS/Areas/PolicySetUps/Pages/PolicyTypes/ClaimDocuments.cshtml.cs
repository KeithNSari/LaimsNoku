using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Security;
using LAIMS.Repositories.Lifeproducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;


namespace LAIMS.Areas.PolicySetUps.Pages.PolicyTypes
{
    [Authorize(Roles = "Admin")]
    public class ClaimDocumentsModel : PageModel
    { 
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IPolicyTypesLinesRepository _policyTypesLinesRepository;
        private readonly IPolicyTypeLinesBenefitsDocumentsRepository _policyTypeLinesBenefitsDocumentsRepository;
        private readonly IDocumentsRepository _documentsRepository;
        public List<SelectListItem> DocumentsList = new List<SelectListItem>();
        public List<SelectListItem> MyProducts = new List<SelectListItem>();
        public DataTable ClaimDocumentsDT;
        public ClaimDocumentsModel(UserManager<ApplicationUser> userManager, IPolicyTypeRepository policyTypeRepository, IPolicyTypesLinesRepository policyTypesLinesRepository, IPolicyTypeLinesBenefitsDocumentsRepository policyTypeLinesBenefitsDocumentsRepository, IDocumentsRepository documentsRepository)
        {
            _userManager = userManager;
            _policyTypeRepository = policyTypeRepository;
            _policyTypesLinesRepository = policyTypesLinesRepository;
            _policyTypeLinesBenefitsDocumentsRepository = policyTypeLinesBenefitsDocumentsRepository;
            _documentsRepository = documentsRepository;
        }
        [BindProperty]
        public PolicyTypeLinesBenefitDocument MyPolicyTypeLinesBenefitDocument { get; set; } = default!;
        [BindProperty]
        public PolicyType MyPolicyType { get; set; } = default!;
        [BindProperty]
        public PolicyTypeLinesBenefit MyPolicyTypeLinesBenefit { get; set; } = default!;
        public IActionResult OnGet(Guid id)
        {
            try
            {
                var policytypes = _policyTypeRepository.GetPolicyType(id);
                if (policytypes == null)
                {
                    return NotFound();
                }
                LoadPage (id,policytypes);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }           
            return Page();
        }
        private void LoadPage(Guid id, PolicyType policytypes)
        { 
                MyPolicyType = policytypes;
                LoadProductsSelectList(id);
                LoadDocumentsSelectList();
                ClaimDocumentsDT = _policyTypeLinesBenefitsDocumentsRepository.GetDocuments(id); 
        }
        private void LoadProductsSelectList(Guid id)
        {
            foreach (DataRow dr in _policyTypesLinesRepository.GetProducts(id).Rows)
            {
                MyProducts.Add(new SelectListItem
                {
                    Value = dr["PTLID"].ToString(),
                    Text = dr["Product"].ToString()
                });
            }
        }
        private void LoadDocumentsSelectList()
        {
            foreach (Document document in _documentsRepository.GetAllDocuments())
            {
                DocumentsList.Add(new SelectListItem
                {
                    Value = document.ID.ToString(),
                    Text = document.DocumentName
                });
            }
        }
        public IActionResult OnPostSaveDocument(Guid id)
        {
            string errorMsg = string.Empty;
            try
            {
                var policytypes = _policyTypeRepository.GetPolicyType(id);
                if (policytypes == null)
                {
                    return NotFound();
                }
                try
                {                                 
                    MyPolicyTypeLinesBenefitDocument.PTLID = MyPolicyTypeLinesBenefit.PTLID;
                    if (_policyTypeLinesBenefitsDocumentsRepository.CheckExistence(MyPolicyTypeLinesBenefitDocument) == 0)
                    {
                        MyPolicyTypeLinesBenefitDocument.Current = 1;
                        MyPolicyTypeLinesBenefitDocument.AddedOn = DateTime.Now;
                        MyPolicyTypeLinesBenefitDocument.AddedBy = _userManager.GetUserId(User).ToString();
                        _policyTypeLinesBenefitsDocumentsRepository.InsertDocument(MyPolicyTypeLinesBenefitDocument);
                    }
                    else
                    {
                        throw new Exception("Document has already been added for this validation group! If a document applies to both tested and untested business, select the 'All' option.");
                    }
                    return Redirect("ClaimDocuments?id=" + id.ToString());
                }
                catch (Exception ex)
                {
                    errorMsg = "Error Processing request: " + ex.Message;
                    LoadPage(id, policytypes);
                }                        
            }
            catch (Exception ex)
            {
                errorMsg += Environment.NewLine + " Error Reloading Page: " + ex.Message;
            }
            ViewData["ErrorMessage"] = errorMsg;
            return Page();
        }
        public IActionResult OnPostRemoveDocument(Guid id, int entryID)
        {
            //process page, if error reload page and display processing error
            string errorMsg = string.Empty;
            try 
            {
                var policytypes = _policyTypeRepository.GetPolicyType(id);
                if (policytypes == null)
                {
                    return NotFound();
                }
                try
                {
                    string addedBy = _userManager.GetUserId(User).ToString();
                    DateTime deletedOn = DateTime.Now;
                    _policyTypeLinesBenefitsDocumentsRepository.ArchiveDocument(entryID, addedBy, deletedOn);
                    return Redirect("ClaimDocuments?id=" + id.ToString());
                }
                catch (Exception ex)
                {
                    ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
                    LoadPage(id, policytypes);                    
                }
            }
            catch (Exception ex)
            {
                errorMsg += Environment.NewLine + " Error Reloading Page: " + ex.Message;
            } 
            ViewData["ErrorMessage"] = errorMsg;         
            return Page();
        }
    }
}
