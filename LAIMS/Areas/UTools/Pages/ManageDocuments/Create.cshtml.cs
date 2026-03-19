using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection.Metadata;
using LAIMS.Models.LifeProducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using LAIMS.Interfaces.Lifeproducts;
using System.Data;
using LAIMS.Models.Security;

namespace LAIMS.Areas.UTools.Pages.ManageDocuments
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDocumentsRepository _documentRepository;
        public DataTable DocumentsDT { get; set; }  
        public CreateModel(UserManager<ApplicationUser> userManager,IDocumentsRepository documentsRepository)
        {
            _userManager = userManager;
            _documentRepository = documentsRepository;
          
        }
        [BindProperty]
        public LAIMS.Models.LifeProducts.Document Document { get; set; }
        public void OnGet()
        {          
            try
            {
              ReturnUrl = Request.Path + Request.QueryString;
              DocumentsDT = _documentRepository.Get();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
        }
        public IActionResult OnPost()
        {   
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    
                //    return Page();
                //}
                Document.AddedBy = _userManager.GetUserId(User).ToString();
                Document.AddedOn = DateTime.Now;
                if (_documentRepository.CheckExistence(Document.DocumentName) == 0)
                {
                    _documentRepository.InsertDocument(Document);
                }
                else
                {
                    throw new Exception("This entry already exists!");
                }
                return Redirect("Create");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPostRemove(Guid ID)
        {           
            try
            {
                Document.ID = ID;
                Document.AddedBy = _userManager.GetUserId(User).ToString();
                Document.AddedOn = DateTime.Now;
                _documentRepository.ArchiveDocument(ID, Document.AddedBy, (DateTime)Document.AddedOn);
                return RedirectToPage("Create");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
    }
}
