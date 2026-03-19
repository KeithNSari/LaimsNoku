using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Security;
using LAIMS.Repositories.Lifeproducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace LAIMS.Areas.PolicySetUps.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class DocumentsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductRepository _productRepository;
        private readonly IDocumentsRepository _documentsRepository;
        private readonly IProductDocumentRepository _productDocumentRepository;
        public List<SelectListItem> DocumentsList = new List<SelectListItem>();
        public DataTable DocumentsDT;
        public DocumentsModel(UserManager<ApplicationUser> userManager, IProductRepository productRepository, IDocumentsRepository documentsRepository, IProductDocumentRepository productDocumentRepository)
        {
            _userManager = userManager;
            _productRepository = productRepository;
            _documentsRepository = documentsRepository;
            _productDocumentRepository = productDocumentRepository;
        }
        [BindProperty]
        public int LIRoleID { get; set; } = 0;
        [BindProperty]
        public int Tested { get; set; } = 0;

       [BindProperty]
        public string ReturnUrl { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SourceUrl { get; set; }
        [BindProperty]
        public Product MyProduct { get; set; } = default!;
        [BindProperty]
        public ProductDocument MyProductDocument { get; set; } = default!;
        public IActionResult OnGet(Guid id)
        {           
            try
            {
                SourceUrl = Request.Path + Request.QueryString;
                var product = _productRepository.GetProduct(id);
                if (product == null)
                {
                    return NotFound();
                }
                else
                {
                    LoadDocumentsSelectList();
                    MyProduct = product;
                    DocumentsDT = _productDocumentRepository.GetDocuments(id);
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
                MyProductDocument.Tested = Tested;
                MyProductDocument.ProductID = id;
                MyProductDocument.LIRoleID = LIRoleID;
                if (_productDocumentRepository.CheckExistence(MyProductDocument) == 0)
                {                   
                    MyProductDocument.Current = 1;
                    MyProductDocument.AddedOn = DateTime.Now;
                    MyProductDocument.AddedBy = _userManager.GetUserId(User).ToString();
                    _productDocumentRepository.InsertDocument(MyProductDocument);
                }
                DocumentsDT = _productDocumentRepository.GetDocuments(id);
                MyProduct = _productRepository.GetProduct(id);
                MyProductDocument.ValidationGroup = "";
                LoadDocumentsSelectList();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = SourceUrl });
            }
            return Page();
        }
        public IActionResult OnPostRemove(Guid id, int entryno)
        {            
            try
            {
                string addedBy = _userManager.GetUserId(User).ToString();
                DateTime deletedOn = DateTime.Now;
                _productDocumentRepository.DeleteDocument(entryno, addedBy, deletedOn);
                return Redirect(ReturnUrl);
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = SourceUrl });
            } 
        }
    }
}
