using LAIMS.Interfaces;
using LAIMS.Interfaces.Commissions;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Commissions;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using LAIMS.Repositories.Lifeproducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Data;
using System.Transactions;

namespace LAIMS.Areas.UTools.Pages.Commissions
{
	[Authorize(Roles = "Commission Definition")]
	public class PolicyDefinitionsModel : PageModel
    {
        private readonly IUploadData _dataUpload;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IProductRepository _productRepository;
        private readonly IIntermediaryTypeRepository _intermediaryTypeRepository;
        IPolicyTypeCommissionRepository _policyTypeCommissionRepository;
        public DataTable? DT;
        [BindProperty]
        public IFormFile Upload { get; set; }
        public PolicyDefinitionsModel(IWebHostEnvironment environment, UserManager<ApplicationUser> userManager,
            IUploadData dataUpload, IPolicyTypeRepository policyTypeRepository, IProductRepository productRepository,
            IIntermediaryTypeRepository intermediaryTypeRepository, IPolicyTypeCommissionRepository policyTypeCommissionRepository)
        {
            _environment = environment;
            _userManager = userManager;
            _dataUpload = dataUpload;
            _policyTypeRepository = policyTypeRepository;
            _productRepository = productRepository;
            _intermediaryTypeRepository = intermediaryTypeRepository;
            _policyTypeCommissionRepository = policyTypeCommissionRepository;
        }
        public void OnGet()
        {
            DT = _policyTypeCommissionRepository.Get();
        }
        public IActionResult OnPost()
        {
            try 
            {
				if (Upload != null)
				{
					string file = _dataUpload.Documentupload(Upload);
					DT = RemoveNullRows(_dataUpload.ExcelDataTable(file));
					SaveEntries(DT);
				}
			}
			catch (Exception ex)
			{
                DT = null;//displaying the data gives the illusion that it has been uploaded.
				ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
			}
            return Page();			
        }
        DataTable RemoveNullRows(DataTable DT)
        {
            for (int i = DT.Rows.Count - 1; i >= 0; i--)
            {
                DataRow row = DT.Rows[i];
                if (row[3] == DBNull.Value || row[3] == null)
                {
                    DT.Rows.RemoveAt(i);
                }
            }
            return DT;
        }
        private void SaveEntries(DataTable DT)
        {
            using (TransactionScope TS = new TransactionScope())
            {
                Guid PolicyTypeID = Guid.Empty;
                Guid ProductID = Guid.Empty;
                for (int i = 0; i < DT.Rows.Count; i++)
                {
                    DataRow DR = DT.Rows[i];
                    if ((i == 0) && (DR[0] == DBNull.Value || DR[0] == null))
                    {
                        throw new Exception("First row, first column cannot be null or empty!");
                    }

                    if (!(DR[0] == DBNull.Value || DR[0] == null))
                    {
                        //fetch policyID
                        PolicyTypeID = _policyTypeRepository.GetID(DR[0].ToString());
                        if (PolicyTypeID == Guid.Empty)
                        {
                            throw new Exception("Row " + i.ToString() + " Invalid policy found. Operation cancelled!");
                        }
                    }

                    if (!(DR[1] == DBNull.Value || DR[1] == null))
                    {
                        //fetch policyID
                        ProductID = _productRepository.GetID(DR[1].ToString());
                        if (ProductID == Guid.Empty)
                        {
                            throw new Exception("Row " + i.ToString() + " Invalid product name found. Operation cancelled!");
                        }
                    }
                    int intermediaryTypeID = 0;
                    if (!(DR[2] == DBNull.Value || DR[2] == null))
                    {
                        intermediaryTypeID = _intermediaryTypeRepository.GetTypeID(DR[2].ToString());
                        if (intermediaryTypeID == 0)
                        {
                            throw new Exception("Row " + i.ToString() + " Invalid Intermediary type found. Operation cancelled!");
                        }
                    }
                    else
                    {
                        throw new Exception("Row " + i.ToString() + " Intermediary type value cannot be empty. Operation cancelled!");
                    }
                    if (DR[3] == DBNull.Value || DR[3] == null)
                    {
                        throw new Exception("Row " + i.ToString() + " Invalid commission. Operation cancelled!");
                    }
                    if (decimal.TryParse(DR[3].ToString(), out decimal commission))
                    {
                        if (!((commission >= 0) && (commission <= 100)))
                        {
                            throw new Exception("Row " + i.ToString() + " Invalid commission. Operation cancelled!");
                        }
                    }
                    else
                    {
                        throw new Exception("Row " + i.ToString() + " Invalid commission. Operation cancelled!");
                    }
                    if (DR[4] == DBNull.Value || DR[4] == null)
                    {
                        throw new Exception("Row " + i.ToString() + " Calculation cannot be null. Operation cancelled!");
                    }
                    string calculation = DR[4].ToString();
                    //if (DR[5] == DBNull.Value || DR[5] == null)
                    //{
                    //    throw new Exception("Row " + i.ToString() + " Calculation cannot be null. Operation cancelled!");
                    //}
                    string function = DR[5].ToString();
                    if (DR[6] == DBNull.Value || DR[6] == null)
                    {
                        throw new Exception("Row " + i.ToString() + " Invalid CPP Start value. Operation cancelled!");
                    }
                    if (DR[7] == DBNull.Value || DR[7] == null)
                    {
                        throw new Exception("Row " + i.ToString() + " Invalid CPP Ends value. Operation cancelled!");
                    }

                    if (int.TryParse(DR[6].ToString(), out int cppStarts))
                    {
                        if (cppStarts < 1)
                        {
                            throw new Exception("CPP Start should be at least 1.");
                        }
                    }
                    else
                    {
                        throw new Exception("Row " + i.ToString() + " Invalid CPP Starts value. Operation cancelled!");
                    }
                    if (int.TryParse(DR[7].ToString(), out int cppEnds))
                    {
                        if (cppEnds < 0)
                        {
                            throw new Exception("Row " + i.ToString() + " CPP Ends should be 0 or greater.");
                        }
                    }
                    else
                    {
                        throw new Exception("Row " + i.ToString() + " Invalid CPP Ends value. Operation cancelled!");
                    }

                    if ((cppEnds < cppStarts) &&(cppEnds!=0))
                    {
                        throw new Exception("Row " + i.ToString() + " CPP Ends should be greater than CPP Starts.");
                    }
                    PolicyTypeCommission policyTypeCommission = new()
                    {
                        IntermediaryTypeID = intermediaryTypeID,
                        PolicyTypeID = PolicyTypeID,
                        ProductID = ProductID,
                        FunctionType = 0,
                        FunctionName = "",
                        Calculation = calculation,
                        CommissionRate = commission,
                        CPPStarts = cppStarts,
                        CPPEnds = cppEnds
                    };
                    _policyTypeCommissionRepository.AddPolicyTypeCommission(policyTypeCommission);                  
                }
                TS.Complete();
            }
        }
    }
}
