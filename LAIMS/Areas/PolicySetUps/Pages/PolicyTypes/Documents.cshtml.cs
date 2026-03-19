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
    public class DocumentsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string PageUrl { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyTypeRepository _policyTypeRepository; 
        private readonly IDocumentsRepository _documentsRepository;
        private readonly IPolicyTypeDocumentsRepository _policyTypeDocumentsRepository;
        public List<SelectListItem> DocumentsList = new List<SelectListItem>();
        public DataTable DocumentsDT;
        public DocumentsModel(UserManager<ApplicationUser> userManager, IPolicyTypeRepository policyTypeRepository,IDocumentsRepository documentsRepository,IPolicyTypeDocumentsRepository policyTypeDocumentsRepository)
        {
            _userManager = userManager;
            _policyTypeRepository = policyTypeRepository; 
            _documentsRepository = documentsRepository;
            _policyTypeDocumentsRepository = policyTypeDocumentsRepository;  
        }
        [BindProperty ]
        public string ReturnUrl { get; set; }
        [BindProperty]
        public PolicyType MyPolicyType { get; set; } = default!;
        [BindProperty]
        public PolicyTypeDocument MyPolicyTypeDocument { get; set; } = default!; 
        public IActionResult OnGet(Guid id)
        {
            try
            {
              
                var policytypes = _policyTypeRepository.GetPolicyType(id);
                if (policytypes == null)
                {
                    return NotFound();
                }
                else
                {  
                    PageUrl = Request.Path + Request.QueryString;
                    LoadDocumentsSelectList();
                    MyPolicyType = policytypes;
                    DocumentsDT = _policyTypeDocumentsRepository.GetDocuments(id);
                }
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }            
            return Page();
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
        public IActionResult OnPost(Guid id)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    
                //    return Page();
                //}
                MyPolicyTypeDocument.PolicyTypesID = id;
                if (_policyTypeDocumentsRepository.CheckExistence(MyPolicyTypeDocument) == 0)
                {
                    MyPolicyTypeDocument.Current = 1;
                    MyPolicyTypeDocument.AddedOn = DateTime.Now;
                    MyPolicyTypeDocument.AddedBy = _userManager.GetUserId(User).ToString();
                    _policyTypeDocumentsRepository.InsertDocument(MyPolicyTypeDocument);
                }
                DocumentsDT = _policyTypeDocumentsRepository.GetDocuments(id);
                MyPolicyType = _policyTypeRepository.GetPolicyType(id);
                MyPolicyTypeDocument.ValidationGroup = "";
                LoadDocumentsSelectList(); 
                return Page();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = PageUrl });
            }           
        }
        public IActionResult OnPostRemove(Guid id, int entryno)
        {
            try
            {
                string addedBy = _userManager.GetUserId(User).ToString();
                DateTime deletedOn = DateTime.Now;
                _policyTypeDocumentsRepository.ArchiveDocument(entryno, addedBy, deletedOn);
                return Redirect(ReturnUrl);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = PageUrl });
            } 
        }
    }
}
