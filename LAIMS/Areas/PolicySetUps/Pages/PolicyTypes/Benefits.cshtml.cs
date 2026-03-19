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
    public class BenefitsModel : PageModel
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyTypeRepository _policyTypeRepository; 
        private readonly IPolicyTypesLinesRepository _policyTypesLinesRepository;
        private readonly IPolicyTypeLinesBenefitsRepository _policyTypeLinesBenefitsRepository;  
        public List<SelectListItem> MyProducts = new List<SelectListItem>();
        public DataTable ProductsDT;
        public DataTable BenefitsDT;

        public BenefitsModel(UserManager<ApplicationUser> userManager, IPolicyTypeRepository policyTypeRepository, IPolicyTypesLinesRepository policyTypesLinesRepository,IPolicyTypeLinesBenefitsRepository policyTypeLinesBenefitsRepository)
        {
            _userManager = userManager;
            _policyTypeRepository = policyTypeRepository;
            _policyTypesLinesRepository = policyTypesLinesRepository;
            _policyTypeLinesBenefitsRepository = policyTypeLinesBenefitsRepository; 
        }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        [BindProperty]
        public PolicyType MyPolicyType { get; set; } = default!;
        [BindProperty]
        public PolicyTypesLines MyPolicyTypeLines { get; set; } = default!;
        [BindProperty ]
        public PolicyTypeLinesBenefit MyPolicyTypeLinesBenefit { get; set; } = default!;
        [BindProperty]
        public Guid PTLBenefitsID { get; set; }
        public IActionResult OnGet(Guid id)
        {           
            try
            {
                ReturnUrl = Request.Path + Request.QueryString;
                var policytypes = _policyTypeRepository.GetPolicyType(id);
                if (policytypes == null)
                {
                    return NotFound();
                }
                else
                {
                    MyPolicyType = policytypes;
                    LoadProductsSelectList(id);
                    BenefitsDT = _policyTypeLinesBenefitsRepository.GetBenefits(id);
                }
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }
            return Page();
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
        public IActionResult OnPostProductSelected(Guid id)
        {
            try
            {
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            { 
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            } 
        }
        public IActionResult OnPostSaveBenefit(Guid id)
        {            
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    
                //    return Page();
                //}
                if ((MyPolicyTypeLinesBenefit.MaximumBenefit < MyPolicyTypeLinesBenefit.Benefit) && (MyPolicyTypeLinesBenefit.MaximumBenefit != 0))
                {
                    throw new Exception("The maximum benefit cannot be less than its minimum!");
                }
                if ((MyPolicyTypeLinesBenefit.WaitingPeriod > 0)&&(!(MyPolicyTypeLinesBenefit.WPDurationUnit>0)))
                {
                    throw new Exception("Invalid duration unit for the entered waiting period!");
                }
                if((MyPolicyTypeLinesBenefit.Benefit==0) && (MyPolicyTypeLinesBenefit.MaximumBenefit==0) && (MyPolicyTypeLinesBenefit.Contribution == 0))
                {
                    if (!(MyPolicyTypeLinesBenefit.WaitingPeriod > 0))
                    {
                        throw new Exception("This record is inconsistent! Define at least one non-zero or non-empty parameter!");
                    }
                }
                MyPolicyTypeLinesBenefit.AddedOn = DateTime.Now;
                MyPolicyTypeLinesBenefit.AddedBy = _userManager.GetUserId(User).ToString();
                _policyTypeLinesBenefitsRepository.InsertBenefit(MyPolicyTypeLinesBenefit);
                BenefitsDT = _policyTypeLinesBenefitsRepository.GetBenefits(id);
                ProductsDT = _policyTypesLinesRepository.GetProducts(id);
                MyPolicyType = _policyTypeRepository.GetPolicyType(id);
                LoadProductsSelectList(id);
                return Redirect("Benefits?id=" + id.ToString());
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }
        public IActionResult OnPostRemoveBenefit(Guid id,int entryID)
        {
            try
            {
                string addedBy = _userManager.GetUserId(User).ToString();
                DateTime deletedOn = DateTime.Now;
                _policyTypeLinesBenefitsRepository.ArchiveBenefit(entryID, addedBy, deletedOn);
                return Redirect("Benefits?id=" + id.ToString());
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
        }         
    }
}
