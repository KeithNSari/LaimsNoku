using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Premiums;
using LAIMS.Interfaces.Utilities;
using LAIMS.Interfaces;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Premiums;
using LAIMS.Models.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Transactions;
using System.Security.Cryptography;
using LAIMS.Areas.NewBusiness.Pages.policies;
using LAIMS.Models.Security;

namespace LAIMS.Areas.PaymentCollection.Pages
{
    public class ProcessBillingFileModel : PageModel
    {
        private readonly IUploadData _dataUpload;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IExcelUploadColumnRepository _excelUploadColumnRepository;
        private readonly IExcelUploadDataRepository _excelUploadDataRepository;
        private readonly IBillingHeaderRepository _billingHeaderRepository; 
        private readonly IProcessPayments _processPayments;
        public List<SelectListItem> Currencies = new List<SelectListItem>();
        public List<SelectListItem> FieldList = new List<SelectListItem>();
        public List<PaymentProvider> PaymentProviders { get; set; }
        public List<SelectListItem> PaymentProvidersList = new List<SelectListItem>();
        public DataTable DT; 
        [BindProperty]
        public IFormFile Upload { get; set; }
        [BindProperty]
        public Guid UploadID { get; set; }
        [BindProperty]
        public PaymentUploadModel PaymentUploadModel { get; set; }
        [BindProperty]
        public BillingBatch BillingBatch { get; set; }
        public ProcessBillingFileModel(IWebHostEnvironment environment, IUploadData dataUpload,
            UserManager<ApplicationUser> userManager, IExcelUploadColumnRepository excelUploadColumnRepository,
            IExcelUploadDataRepository excelUploadDataRepository,IBillingHeaderRepository billingHeaderRepository 
            ,IProcessPayments processPayments)
        {
            _environment = environment;
            _userManager = userManager;
            _dataUpload = dataUpload;
            _excelUploadColumnRepository = excelUploadColumnRepository;
            _excelUploadDataRepository = excelUploadDataRepository;
            _billingHeaderRepository = billingHeaderRepository;
            _processPayments = processPayments;           
        }

        public void OnGet(int pid, int pccid, long batchid)
        {
            PaymentUploadModel = new ()
            {
                PaymentProvider = pid,
                PCCID = pccid
            };
            BillingBatch = _billingHeaderRepository.GetBillingBatchById(batchid);
        } 
        public IActionResult OnPost()
        {
            string ReturnUrl=Request.Path + Request.QueryString;
            try
            {
                if (Upload != null)
                {
                    var extension = Path.GetExtension(Upload.FileName);
                    if (!extension.Contains(".xls"))
                    {
                        throw new Exception($"This file extension is not allowed!");
                    }
                    UploadID = Guid.NewGuid();
                    PaymentUploadModel.UploadID = UploadID;
                    PaymentUploadModel.BillingBatchID = BillingBatch.BatchID;
                    using (TransactionScope TS = new TransactionScope())
                    {
                        Guid BatchID = Guid.NewGuid();
                        string AddedBy = _userManager.GetUserId(User).ToString();
                        string file = _dataUpload.Documentupload(Upload);
                        DT = RemoveNullRows(_dataUpload.ExcelDataTable(file));
                        LoadFieldSelectList(DT);
                        SaveExcelFields(DT, UploadID, AddedBy);
                        SaveExcelData(DT, UploadID, AddedBy);
                        TS.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { errorMessage = "An error occurred during processing. " + ex.Message, returnUrl = ReturnUrl });
            }
            return Page();
        }
        public IActionResult OnPostProcessPayment()
        {
            //using (TransactionScope TS = new TransactionScope())
            //{
                string AddedBy = _userManager.GetUserId(User).ToString();
                PaymentUploadModel.AddedBy = AddedBy;
                PaymentUploadModel.CurrencyID = BillingBatch.CurrencyID;   
                PaymentUploadModel.PaymentProvider = BillingBatch.PaymentProviderID; 
                PaymentUploadModel.PaymentMethod = BillingBatch.PaymentMethodID;
                PaymentUploadModel.BillingBatchID = BillingBatch.BatchID;
                _processPayments.RunBatch(PaymentUploadModel);
                //TS.Complete();
            //}
            return Redirect("/PaymentCollection/BatchSupenseIndex?batchid=" + BillingBatch.BatchID);
        } 
        DataTable RemoveNullRows(DataTable DT)
        {
            for (int i = DT.Rows.Count - 1; i >= 0; i--)
            {
                DataRow row = DT.Rows[i];
                if (row[0] == DBNull.Value || row[0] == null)
                {
                    DT.Rows.RemoveAt(i);
                }
            }
            return DT;
        }
        private void LoadFieldSelectList(DataTable DT)
        {
            for (int i = 0; i < DT.Columns.Count; i++)
            {
                DataColumn DC = DT.Columns[i];
                FieldList.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = DC.ColumnName
                });
            }
        }

        private void SaveExcelFields(DataTable DT, Guid UploadID, string AddedBy)
        {
            for (int i = 0; i < DT.Columns.Count; i++)
            {
                DataColumn DC = DT.Columns[i];
                ExcelUploadColumn excelUploadColumn = new ExcelUploadColumn()
                {
                    ID = Guid.NewGuid(),
                    MediaUploadID = UploadID,
                    ColumnName = DC.ColumnName,
                    ColumnID = i,
                    DataType = DC.DataType.ToString(),
                    AddedBy = AddedBy,
                    AddedOn = DateTime.Now
                };
                _excelUploadColumnRepository.Insert(excelUploadColumn);
            }
        }
        private void SaveExcelData(DataTable DT, Guid UploadID, string AddedBy)
        {
            foreach (DataRow DR in DT.Rows)
            {
                ExcelUploadData excelUploadData = new ExcelUploadData()
                {
                    ID = Guid.NewGuid(),
                    MediaUploadID = UploadID
                };
                for (int i = 0; i < DT.Columns.Count; i++)
                {
                    switch (i)
                    {
                        case 0:
                            excelUploadData.Column1 = DR[0].ToString();
                            break;
                        case 1:
                            excelUploadData.Column2 = DR[1].ToString();
                            break;
                        case 2:
                            excelUploadData.Column3 = DR[2].ToString();
                            break;
                        case 3:
                            excelUploadData.Column4 = DR[3].ToString();
                            break;
                        case 4:
                            excelUploadData.Column5 = DR[4].ToString();
                            break;
                        case 5:
                            excelUploadData.Column6 = DR[5].ToString();
                            break;
                        case 6:
                            excelUploadData.Column7 = DR[6].ToString();
                            break;
                        case 7:
                            excelUploadData.Column8 = DR[7].ToString();
                            break;
                        case 8:
                            excelUploadData.Column9 = DR[8].ToString();
                            break;
                        case 9:
                            excelUploadData.Column10 = DR[9].ToString();
                            break;
                        case 10:
                            excelUploadData.Column11 = DR[10].ToString();
                            break;
                        case 11:
                            excelUploadData.Column12 = DR[11].ToString();
                            break;
                        case 12:
                            excelUploadData.Column13 = DR[12].ToString();
                            break;
                        case 13:
                            excelUploadData.Column14 = DR[13].ToString();
                            break;
                        case 14:
                            excelUploadData.Column15 = DR[14].ToString();
                            break;
                        default:
                            break;
                    }
                }
                _excelUploadDataRepository.Insert(excelUploadData);
            }
        }
    }
}