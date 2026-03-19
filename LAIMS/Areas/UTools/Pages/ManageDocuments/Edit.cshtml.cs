using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace LAIMS.Areas.UTools.Pages.ManageDocuments
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDocumentsRepository _documentRepository; 
        public EditModel(UserManager<ApplicationUser> userManager, IDocumentsRepository documentsRepository)
        {
            _userManager = userManager;
            _documentRepository = documentsRepository;
        }
        [BindProperty]
        public LAIMS.Models.LifeProducts.Document Document { get; set; }
        public void OnGet(Guid id)
        {           
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                Document = _documentRepository.GetDocument(id);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
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
                Document.ID = id;
                Document.AddedBy = _userManager.GetUserId(User).ToString();
                Document.AddedOn = DateTime.Now;
                if(_documentRepository.CheckExistenceOther(Document.DocumentName,Document.ID)>0)
                {
                    throw new Exception("Another active document type with this name already exists!");
                }
                _documentRepository.UpdateDocument(Document);
                return Redirect("Create");
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        } 
    }
}
