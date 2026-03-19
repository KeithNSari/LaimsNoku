using LAIMS.Interfaces;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Premiums;
using LAIMS.Interfaces.Utilities;
using LAIMS.Models.Documents;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Premiums;
using LAIMS.Models.Security;
using LAIMS.Models.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Transactions;

namespace LAIMS.Areas.UTools.Pages.PaymentProviders.Orders
{
    //[Authorize(Roles = "Payment Servicing")]
    public class OrderPaymentModel : PageModel
    {
        private readonly IUploadData _dataUpload;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IExcelUploadColumnRepository _excelUploadColumnRepository;
        private readonly IExcelUploadDataRepository _excelUploadDataRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IPaymentProviderRepository _paymentProviderRepository;
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
        public OrderPaymentModel(IWebHostEnvironment environment, IUploadData dataUpload, 
            UserManager<ApplicationUser> userManager, IExcelUploadColumnRepository excelUploadColumnRepository,
            IExcelUploadDataRepository excelUploadDataRepository, IPaymentProviderRepository paymentProviderRepository
            , ICurrencyRepository currencyRepository, IProcessPayments processPayments)
        {
            _environment = environment;
            _userManager = userManager;
            _dataUpload = dataUpload;
            _excelUploadColumnRepository = excelUploadColumnRepository; 
            _excelUploadDataRepository =excelUploadDataRepository;
            _currencyRepository = currencyRepository;
            _paymentProviderRepository = paymentProviderRepository;
            _processPayments = processPayments;
        }

        public void OnGet()
        {
            LoadCurrencies();
            PaymentProviders = _paymentProviderRepository.GetAllPaymentProviders();
            LoadPaymentProvidersSelectList();
        }
        private void LoadPaymentProvidersSelectList()
        {
            foreach (PaymentProvider paymentProvider in PaymentProviders)
            {
                PaymentProvidersList.Add(
                    new SelectListItem
                    {
                        Value = paymentProvider.MemberID.ToString(),
                        Text = paymentProvider.ProviderName
                    }
                   );
            }
        }
        public void OnPost()
        {
            if (Upload != null)
            {
                UploadID= Guid.NewGuid();
                PaymentUploadModel.UploadID = UploadID; 
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
            LoadCurrencies();
            PaymentProviders = _paymentProviderRepository.GetAllPaymentProviders();
            LoadPaymentProvidersSelectList();
        }
        public IActionResult OnPostProcessPayment()
        {
            using (TransactionScope TS = new TransactionScope())
            {
                string AddedBy = _userManager.GetUserId(User).ToString();
                PaymentUploadModel.AddedBy = AddedBy;
                PaymentUploadModel.PaymentMethod = 2;
                _processPayments.RunBatch(PaymentUploadModel);
                TS.Complete();
            }
            return Redirect("/PaymentCollection/BillingHistory");
        }
        private void LoadCurrencies()
        {
            foreach (Currency currency in _currencyRepository.GetAllCurrencies())
            {
                Currencies.Add(new SelectListItem
                {
                    Value = currency.ID.ToString(),
                    Text = currency.CurrencyName
                });
            }
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
            for(int i= 0;i<DT.Columns.Count;i++)
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
            for ( int i=0; i<DT.Columns.Count;i++)
            {
                DataColumn DC = DT.Columns[i];
                ExcelUploadColumn excelUploadColumn = new ExcelUploadColumn()
                {
                    ID= Guid.NewGuid (),
                    MediaUploadID= UploadID,
                    ColumnName=DC.ColumnName,
                    ColumnID=i,
                    DataType=DC.DataType.ToString(),
                    AddedBy=AddedBy,
                    AddedOn=DateTime.Now
                };
                _excelUploadColumnRepository.Insert(excelUploadColumn); 
            }
        }
        private void SaveExcelData(DataTable DT, Guid UploadID, string AddedBy)
        {
            foreach(DataRow DR in DT.Rows)
            {
                ExcelUploadData excelUploadData = new ExcelUploadData()
                {
                    ID = Guid.NewGuid(),
                    MediaUploadID = UploadID
                };
                for(int i=0;i<DT.Columns.Count;i++)
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
