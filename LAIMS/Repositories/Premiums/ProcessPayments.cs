using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Investments;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Policies;
using LAIMS.Models.Premiums;
using LAIMS.Repositories.Commissions;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Runtime.Intrinsics.Arm;
using static LAIMS.Repositories.Premiums.ProcessPayments;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
namespace LAIMS.Repositories.Premiums
{
    public class ProcessPayments: IProcessPayments
    {
        private readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        private ISuspenseProcessing _suspenseProcessing;
        private IPolicyRepository _policyRepository;
        private readonly IPaymentRepository _paymentRepository;
        Guid UploadID = Guid.Empty;
        long BillingBatchID; 
        int identityType;
        int identifierColumn;
        int currencyID;
        int amountColumn;
        int referenceColumn;
        int datePaidColumn;
        DataTable paymentData = new DataTable();
        int paymentMethod;
        int paymentProvider;
        string addedBy;
        string identifierColumnName;
        string amountColumnName;
        string referenceColumnName;
        string datePaidColumnName;
        public ProcessPayments(IConfiguration configuration, IWebHostEnvironment environment, 
            ISuspenseProcessing suspenseProcessing, IPolicyRepository policyRepository,
            IPaymentRepository paymentRepository)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
            _suspenseProcessing = suspenseProcessing;
            _policyRepository = policyRepository;
            _paymentRepository = paymentRepository;
        }
        public void RunBatch(PaymentUploadModel paymentUploadModel)
        {
            UploadID = paymentUploadModel.UploadID;
            BillingBatchID = paymentUploadModel.BillingBatchID; 
            identityType = paymentUploadModel.IdentityType;
            identifierColumn = paymentUploadModel.IdentifierColumn + 1;
            currencyID = paymentUploadModel.CurrencyID;
            amountColumn = paymentUploadModel.AmountColumn + 1;
            referenceColumn = paymentUploadModel.ReferenceColumn + 1;
            datePaidColumn = paymentUploadModel.DatePaidColumn + 1;

            paymentMethod = paymentUploadModel.PaymentMethod;
            paymentProvider = paymentUploadModel.PaymentProvider;
            addedBy = paymentUploadModel.AddedBy;

            identifierColumnName = "Column" + identifierColumn;
            amountColumnName = "Column" + amountColumn;
            referenceColumnName = "Column" + referenceColumn;
            datePaidColumnName = "Column" + datePaidColumn;
            SavePaymentsData();
            paymentData = GetPaymentData(BillingBatchID);   
            if (identityType == 0)//policy
            {
                RunForProvidedPolicy();
            }
            else
            { 
                RunForOtherID(identityType);
            }
            UpdateBatchBalances(BillingBatchID);
            //Add messages for billing
            GenerateMessages(BillingBatchID);
        }
        private void RunForProvidedPolicy()
        {
            foreach (DataRow DR in paymentData.Rows)
            {
                Guid policyID = GetPolicyID(DR["Details"].ToString());
                InsertPaymentRecord(policyID, DR);
            }
            UpdateBillingBatch(BillingBatchID);
            UpdateCommissionGrossAmounts(BillingBatchID);
        }
        private void RunForOtherID(int IdentityType) //if identifier is not policy ID
        {
            foreach (DataRow DR in paymentData.Rows)
            {
                int premiumPayerID = 0;
                string identifier = DR["Details"].ToString();
                int paymentID = Convert.ToInt32(DR["ID"]);
                switch (IdentityType)
                {
                    case 1: //Employee Number
                        premiumPayerID = GetPremiumPayerByEmploymentNo(identifier, paymentProvider);
                        break;
                    case 2: //Identity Number
                        premiumPayerID = GetPremiumPayerByNationalID(identifier);
                        break;
                    case 3: //Bank Account
                        premiumPayerID = GetPremiumPayerByBankAccountNo(identifier, paymentProvider);
                        break;
                    default:
                        throw new Exception("Invalid Identifier Type");
                }
                //get billing header 
                decimal paidAmount = Convert.ToDecimal(DR["Amount"]);

                DateTime datePaymentReceived = Convert.ToDateTime(DR["PaymentDate"].ToString());
                string reference = DR["Reference"].ToString();
                string target = DR["Details"].ToString();
                BillingHeader billingHeader = GetBatchBillByPremiumPayer(BillingBatchID, premiumPayerID);
                if (billingHeader != null)
                {
                    InsertPaymentRecord(billingHeader, paymentID, target, paidAmount, datePaymentReceived, reference);
                }
                else
                {
                    //throw into system suspense account 
                    SuspenseHeader suspenseHeader = new()
                    {
                        BatchID = BillingBatchID,
                        MemberID = 0,
                        PaymentMethodID = paymentMethod,
                        PaymentTypeID = 0,
                        PaymentID = paymentID,
                        Source = "",
                        InternalBankAccountID = 0,
                        SuspenseType = 3,
                        PolicyID = Guid.Empty,
                        TargetPolicyNo = target,
                        PaymentDate = datePaymentReceived,
                        Reference = reference,
                        StatusID = 0,
                        ProcessedAmount = 0,
                        CurrencyID = currencyID,
                        Balance = paidAmount,
                        AddedOn = DateTime.Now,
                        AddedBy = addedBy
                    };
                    int id = _suspenseProcessing.AddSuspenseHeader(suspenseHeader);
                    SuspenseLine suspenseLine = new()
                    {
                        HeaderID = id,
                        Credit = paidAmount,
                        Debit = 0,
                        AddedOn = DateTime.Now,
                        AddedBy = addedBy
                    };
                    _suspenseProcessing.AddSuspenseLine(suspenseLine);
                }
            }
            UpdateCommissionGrossAmounts(BillingBatchID);
        }
        private CollectionCommission GetCollectionCommission(long BatchID)
        {
            CollectionCommission CC = new CollectionCommission();
            using (SqlConnection connection = new(Database))
            {
                connection.Open();
                string sql = "DECLARE @PCCID int=0; SELECT @PCCID=[PCCID] FROM[BillingBatches] WHERE [BillingBatches].[BatchID]=@BatchID; SELECT ISNULL([CollectionCommissionRate],0) AS [CollectionCommissionRate],ISNULL([Net],0) AS [Net] FROM [dbo].[PremiumCollectionConfigHeader] WHERE [Archived]=0 AND [ID]=@PCCID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            CC.Net = (byte)reader["Net"];
                            CC.CollectionCommissionRate = (decimal)reader["CollectionCommissionRate"];
                        }
                    }
                }
            }
            return CC;
        }
        public class BillDetails
        {
            public decimal BilledAmount { get; set; }
            public int BillID { get; set; }
            public long BatchID { get; set; }
        }
        public BillDetails GetBillDetails(decimal PaidAmount, long BatchID, Guid PolicyID)
        {
            BillDetails billDetails = new BillDetails();
            using (SqlConnection connection = new SqlConnection(Database))
            {

                // string query = "DECLARE @BillID int=0;DECLARE @BilledAmount decimal(18,3)=0; SELECT @BillID=[BillID],@BilledAmount=[Amount] FROM [BilledPolicies] WHERE [Paid]=0 AND [Amount]<=@PaidAmount AND [BatchID]=@BatchID AND [PolicyID]=@PolicyID;  SELECT @BillID AS BillID,@BilledAmount AS BilledAmount";
                string query = "DECLARE @BillID int=0;DECLARE @BilledAmount decimal(18,3)=0; SELECT @BillID=[BillID],@BilledAmount=[Amount] FROM [BilledPolicies] WHERE [Paid]=0 AND [BatchID]=@BatchID AND [PolicyID]=@PolicyID;  SELECT @BillID AS BillID,@BilledAmount AS BilledAmount";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PaidAmount", PaidAmount);
                command.Parameters.AddWithValue("@BatchID", BatchID);
                command.Parameters.AddWithValue("@PolicyID", PolicyID);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                       billDetails.BilledAmount = (decimal)reader["BilledAmount"];
                       billDetails.BillID = (int)reader["BillID"];
                    }
                }                
            }
            return billDetails;
        }
        public BillDetails GetFirstUnpaidBillDetails(decimal PaidAmount, Guid PolicyID)
        {
            BillDetails billDetails = new BillDetails();
            using (SqlConnection connection = new SqlConnection(Database))
            {

                string query = "DECLARE @BatchID bigint=0; DECLARE @BillID int=0;DECLARE @BilledAmount decimal(18,3)=0; SELECT Top(1) @BillID=[BillID],@BilledAmount=[Amount],@BatchID=[BatchID] FROM [BilledPolicies] WHERE [Paid]=0 AND [Amount]<=@PaidAmount AND [PolicyID]=@PolicyID ORDER BY [ID] ASC;  SELECT @BillID AS BillID,@BilledAmount AS BilledAmount, @BatchID AS [BatchID]";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PaidAmount", PaidAmount); 
                command.Parameters.AddWithValue("@PolicyID", PolicyID);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        billDetails.BilledAmount = (decimal)reader["BilledAmount"];
                        billDetails.BillID = (int)reader["BillID"];
                        billDetails.BatchID = (long)reader["BatchID"];
                    }
                }
            }
            return billDetails;
        }
        private void InsertPaymentRecord(Guid PolicyID, DataRow DR)
        {
            decimal paidAmount = Convert.ToDecimal(DR["Amount"]);
            int paymentID = Convert.ToInt32(DR["ID"]);
            if (PolicyID != Guid.Empty)
            {
                //BilledPremium bp = GetBill(BillingBatchID, PolicyID);
                BillDetails billDetails = GetBillDetails(paidAmount, BillingBatchID, PolicyID);
                decimal billedAmount = billDetails.BilledAmount;
                int billID = billDetails.BillID; 
                decimal suspenseAmount;
                if (billedAmount != 0 && billID != 0)
                {
                    decimal expectedAmount = billedAmount;
                    CollectionCommission collectionCommission = GetCollectionCommission(BillingBatchID);
                    if (collectionCommission.Net == 1)
                    {
                        //if we billed amount x, we want a payment of (1-collectionCommissionRate/100)x or greater, because it's a net payment- collection commission has already been deducted
                        expectedAmount = decimal.Round((1 - collectionCommission.CollectionCommissionRate / 100) * billedAmount, 2);
                    }
                    if (paidAmount < expectedAmount) //the whole amount goes to policy suspense
                    {
                        suspenseAmount = paidAmount;
                    }
                    else
                    {
                        suspenseAmount = paidAmount - expectedAmount;
                        foreach (BilledPremium bp in GetBilledPremiums(billID, PolicyID))
                        {
                            PremiumHeader premiumHeader = new PremiumHeader();
                            premiumHeader.BillingID = billID;
                            premiumHeader.BilledPremiumID = bp.ID;
                            premiumHeader.PaymentID = paymentID;
                            premiumHeader.DatePaymentReceived = Convert.ToDateTime(DR["PaymentDate"].ToString());
                            premiumHeader.TotalAmount = bp.Amount;
                            premiumHeader.AddedBy = addedBy;
                            int premiumHeaderID = AddPremiumHeader(premiumHeader);
                            PremiumLine premiumLines = new PremiumLine
                            {
                                PremiumHeaderID = premiumHeaderID,
                                PaymentMethodID = paymentMethod,
                                PaymentProviderID = paymentProvider,
                                CurrencyID = currencyID,
                                Amount = bp.Amount,
                                Reference = DR["Reference"].ToString()
                            };
                            AddPremiumLine(premiumLines);                            
                            PremiumBreakDown(premiumHeaderID);
                            PremiumBreakDownSaveAcquisitionExpenses(premiumHeaderID);
                            AddCommission(premiumHeaderID, bp.PolicyPremiumID,BillingBatchID);
                            BuyUnits(premiumHeaderID);
                        } 
                        UpdateBilledPremiums(billID, PolicyID);
                        UpdateBilledPolicies(billID, PolicyID);
                        UpdateBillingHeader(billID);
                        UpdatePolicyStatus(PolicyID);
                        UpdatePremiumCommencement(billID);
                    }
                }
                else
                {
                    suspenseAmount = paidAmount;// send full amount to Policy Suspense
                }
                if (suspenseAmount > 0)
                {
                    int proposerID = _policyRepository.GetProposerID(PolicyID);
                    SuspenseHeader suspenseHeader = new()
                    {
                        BatchID = BillingBatchID,
                        MemberID = proposerID,
                        PaymentMethodID = paymentMethod,
                        PaymentTypeID = 0,
                        PaymentID = paymentID,
                        Source = "",
                        InternalBankAccountID = 0,
                        SuspenseType = 2,
                        PolicyID = PolicyID,
                        PaymentDate = Convert.ToDateTime(DR["PaymentDate"].ToString()),
                        Reference = DR["Reference"].ToString(),
                        StatusID = 0,
                        ProcessedAmount = 0,
                        CurrencyID = currencyID,
                        Balance = suspenseAmount,
                        AddedOn = DateTime.Now,
                        AddedBy = addedBy
                    };
                    int id = _suspenseProcessing.AddSuspenseHeader(suspenseHeader);
                    SuspenseLine suspenseLine = new()
                    {
                        HeaderID = id,
                        Credit = suspenseAmount,
                        Debit = 0,
                        AddedOn = DateTime.Now,
                        AddedBy = addedBy
                    };
                    _suspenseProcessing.AddSuspenseLine(suspenseLine);
                }
            }
            else
            {
                //we dont have a policy send to system suspense
                SuspenseHeader suspenseHeader = new()
                {
                    BatchID = BillingBatchID,
                    MemberID = 0,
                    PaymentMethodID = paymentMethod,
                    PaymentTypeID = 0,
                    PaymentID = paymentID,
                    Source = "",
                    InternalBankAccountID = 0,
                    SuspenseType = 3,
                    PolicyID = Guid.Empty,
                    TargetPolicyNo = DR["Details"].ToString(),
                    PaymentDate = Convert.ToDateTime(DR["PaymentDate"].ToString()),
                    Reference = DR["Reference"].ToString(),
                    StatusID = 0,
                    ProcessedAmount = 0,
                    CurrencyID = currencyID,
                    Balance = paidAmount,
                    AddedOn = DateTime.Now,
                    AddedBy = addedBy
                };
                int id = _suspenseProcessing.AddSuspenseHeader(suspenseHeader);
                SuspenseLine suspenseLine = new()
                {
                    HeaderID = id,
                    Credit = paidAmount,
                    Debit = 0,
                    AddedOn = DateTime.Now,
                    AddedBy = addedBy
                };
                _suspenseProcessing.AddSuspenseLine(suspenseLine);
            }
        }
        public decimal AdhocPayment(Guid PolicyID, string Reference, decimal Amount, int PaymentMethodID, int PaymentID, DateTime PaymentDate, string AddedBy)
        {
            addedBy = AddedBy; 
            decimal paidAmount = Amount;
            int paymentID = PaymentID;
            if (PolicyID != Guid.Empty)
            {
                BillDetails billDetails = GetFirstUnpaidBillDetails(paidAmount, PolicyID);
                decimal billedAmount = billDetails.BilledAmount;
                int billID = billDetails.BillID; 
                if (billedAmount != 0 && billID != 0)
                {  
                    foreach (BilledPremium bp in GetBilledPremiums(billID, PolicyID))
                    {
                        PremiumHeader premiumHeader = new PremiumHeader();
                        premiumHeader.BillingID = billID;
                        premiumHeader.BilledPremiumID = bp.ID;
                        premiumHeader.PaymentID = paymentID;
                        premiumHeader.DatePaymentReceived = PaymentDate;
                        premiumHeader.TotalAmount = bp.Amount;
                        premiumHeader.AddedBy = AddedBy;
                        int premiumHeaderID = AddPremiumHeader(premiumHeader);
                        PremiumLine premiumLines = new PremiumLine
                        {
                            PremiumHeaderID = premiumHeaderID,
                            PaymentMethodID = PaymentMethodID,
                            PaymentProviderID = 0,
                            CurrencyID = currencyID,
                            Amount = bp.Amount,
                            Reference = Reference
                        };
                        AddPremiumLine(premiumLines);
                        PremiumBreakDown(premiumHeaderID);
                        PremiumBreakDownSaveAcquisitionExpenses(premiumHeaderID);
                        AddCommission(premiumHeaderID, bp.PolicyPremiumID, billDetails.BatchID);
                        BuyUnits(premiumHeaderID);
                    }
                    UpdateBilledPremiums(billID, PolicyID);
                    UpdateBilledPolicies(billID, PolicyID);
                    UpdateBillingHeader(billID);
                    UpdateBillingBatch(billDetails.BatchID);
                    UpdateCommissionGrossAmounts(billDetails.BatchID);
                }
                return billedAmount; //return the amount used to pay off the bill
            }
            return 0;//if we get here, the amount wasn't used
        }
        //private void InsertPaymentRecord(Guid PolicyID, DataRow DR)
        //{
        //    decimal paidAmount = Convert.ToDecimal(DR["Amount"]);
        //    int paymentID=Convert.ToInt32(DR["ID"]);
        //    CollectionCommission collectionCommission = GetCollectionCommission(BillingBatchID);
        //    if (PolicyID != Guid.Empty)
        //    {
        //        BilledPremium bp = GetBill(BillingBatchID, PolicyID);
        //        decimal expectedAmount = bp.Amount;
        //        decimal suspenseAmount;                
        //        if ((bp!=null) && (bp.ID != 0))
        //        {
        //            int billID = (int)bp.BillID;
        //            if (collectionCommission.Net == 1)
        //            {
        //                //if we billed amount x, we want a payment of (1-collectionCommissionRate/100)x or greater, because it's a net payment- collection commission has already been deducted
        //                expectedAmount = decimal.Round((1 - collectionCommission.CollectionCommissionRate / 100) * bp.Amount, 2);
        //            }
        //            if (paidAmount<expectedAmount) //the whole amount goes to policy suspense
        //            {
        //                suspenseAmount = paidAmount;
        //            }
        //            else
        //            {
        //                suspenseAmount = paidAmount - expectedAmount;
        //                PremiumHeader premiumHeader = new PremiumHeader();
        //                premiumHeader.BillingID = billID;
        //                premiumHeader.BilledPremiumID = bp.ID;
        //                premiumHeader.PaymentID = paymentID;
        //                premiumHeader.DatePaymentReceived = Convert.ToDateTime(DR["PaymentDate"].ToString());
        //                premiumHeader.TotalAmount = bp.Amount;
        //                premiumHeader.AddedBy = addedBy;
        //                int premiumHeaderID = AddPremiumHeader(premiumHeader);
        //                PremiumLine premiumLines = new PremiumLine
        //                {
        //                    PremiumHeaderID = premiumHeaderID,
        //                    PaymentMethodID = paymentMethod,
        //                    PaymentProviderID = paymentProvider,
        //                    CurrencyID = currencyID,
        //                    Amount = bp.Amount,
        //                    Reference = DR["Reference"].ToString()
        //                };
        //                AddPremiumLine(premiumLines);
        //                UpdatePolicyStatus(PolicyID);
        //                PremiumBreakDown(premiumHeaderID);
        //                PremiumBreakDownSaveAcquisitionExpenses(premiumHeaderID);
        //                AddCommission(premiumHeaderID, bp.PolicyPremiumID);
        //                UpdateBilledPremiums(billID);
        //                UpdateBillingHeader(billID);
        //                UpdateBillingBatch(BillingBatchID); 
        //            }
        //        }
        //        else
        //        {
        //            suspenseAmount=paidAmount;// send full amount to Policy Suspense
        //        }
        //        if (suspenseAmount > 0)
        //        {
        //            int proposerID = _policyRepository.GetProposerID(PolicyID);
        //            SuspenseHeader suspenseHeader = new()
        //            {
        //                BatchID = BillingBatchID,
        //                MemberID = proposerID,
        //                PaymentMethodID = paymentMethod,
        //                PaymentTypeID = 0,
        //                PaymentID = paymentID,
        //                Source = "",
        //                InternalBankAccountID = 0,
        //                SuspenseType = 2,
        //                PolicyID = PolicyID,
        //                PaymentDate = Convert.ToDateTime(DR["PaymentDate"].ToString()),
        //                Reference = DR["Reference"].ToString(),
        //                StatusID = 0,
        //                ProcessedAmount = 0,
        //                CurrencyID = currencyID,
        //                Balance = suspenseAmount,
        //                AddedOn = DateTime.Now,
        //                AddedBy = addedBy
        //            };
        //            int id = _suspenseProcessing.AddSuspenseHeader(suspenseHeader);
        //            SuspenseLine suspenseLine = new()
        //            {
        //                HeaderID = id,
        //                Credit = suspenseAmount,
        //                Debit = 0,
        //                AddedOn = DateTime.Now,
        //                AddedBy = addedBy
        //            };
        //            _suspenseProcessing.AddSuspenseLine(suspenseLine);
        //        }
        //    }
        //    else
        //    {
        //        //we dont have a policy send to system suspense
        //        SuspenseHeader suspenseHeader = new()
        //        {
        //            BatchID = BillingBatchID,
        //            MemberID = 0,
        //            PaymentMethodID = paymentMethod,
        //            PaymentTypeID = 0,
        //            PaymentID = paymentID,
        //            Source = "",
        //            InternalBankAccountID = 0,
        //            SuspenseType = 3,
        //            PolicyID = Guid.Empty,
        //            TargetPolicyNo = DR["Details"].ToString(),
        //            PaymentDate = Convert.ToDateTime(DR["PaymentDate"].ToString()),
        //            Reference = DR["Reference"].ToString(),
        //            StatusID = 0,
        //            ProcessedAmount = 0,
        //            CurrencyID = currencyID,
        //            Balance = paidAmount,
        //            AddedOn = DateTime.Now,
        //            AddedBy = addedBy
        //        };
        //        int id = _suspenseProcessing.AddSuspenseHeader(suspenseHeader);
        //        SuspenseLine suspenseLine = new()
        //        {
        //            HeaderID = id,
        //            Credit = paidAmount,
        //            Debit = 0,
        //            AddedOn = DateTime.Now,
        //            AddedBy = addedBy
        //        };
        //        _suspenseProcessing.AddSuspenseLine(suspenseLine);
        //    }
        //}

        private void InsertPaymentRecord(BillingHeader BH, int PaymentID,string TargetedIndetifier, decimal PaidAmount, DateTime DatePaymentReceived, string PaymentReference)
        {
            decimal suspenseAmount;
            if (BH.ID != 0)
            {
                int billID = (int)BH.BillID;
                decimal expectedAmount = BH.TotalAmount;
                CollectionCommission collectionCommission = GetCollectionCommission(BillingBatchID);
                if (collectionCommission.Net == 1)
                {
                    //if we billed amount x, we want a payment of (1-collectionCommissionRate/100)x or greater, because it's a net payment- collection commission has already been deducted
                    expectedAmount = decimal.Round((1 - collectionCommission.CollectionCommissionRate / 100) * BH.TotalAmount, 2);
                }
                if (PaidAmount < expectedAmount) //the whole amount goes to system suspense
                {
                    suspenseAmount = PaidAmount;
                }
                else
                {
                    suspenseAmount = PaidAmount - expectedAmount;
                    //get the BilledPremium records in this billing header and pay them off
                    foreach (BilledPremium bp in GetBilledPremiums(billID))
                    {
                        PremiumHeader premiumHeader = new PremiumHeader();
                        premiumHeader.BillingID = billID;
                        premiumHeader.BilledPremiumID = bp.ID;
                        premiumHeader.PaymentID = PaymentID;
                        premiumHeader.DatePaymentReceived = DatePaymentReceived;
                        premiumHeader.TotalAmount = bp.Amount;
                        premiumHeader.AddedBy = addedBy;
                        int premiumHeaderID = AddPremiumHeader(premiumHeader);
                        PremiumLine premiumLines = new();
                        premiumLines.PremiumHeaderID = premiumHeaderID;
                        premiumLines.PaymentMethodID = paymentMethod;
                        premiumLines.PaymentProviderID = paymentProvider;
                        premiumLines.CurrencyID = currencyID;
                        premiumLines.Amount = bp.Amount;
                        premiumLines.Reference = PaymentReference;
                        AddPremiumLine(premiumLines);
                        UpdatePolicyStatus(bp.PolicyID);
                        PremiumBreakDown(premiumHeaderID);
                        PremiumBreakDownSaveAcquisitionExpenses(premiumHeaderID);
                        AddCommission(premiumHeaderID, bp.PolicyPremiumID, BillingBatchID);
                        BuyUnits(premiumHeaderID);
                    }
                    UpdateBilledPremiums(billID);
                    UpdateBilledPolicies(billID);
                    UpdateBillingHeader(billID);
                    UpdatePremiumCommencement(billID);
                    UpdateBillingBatch(BillingBatchID);
                }               
            }
            else
            {
                suspenseAmount = PaidAmount;
            }
            if(suspenseAmount > 0)
            {
                //send to system suspense
                SuspenseHeader suspenseHeader = new()
                {
                    BatchID = BillingBatchID,
                    MemberID = 0,
                    PaymentMethodID = paymentMethod,
                    PaymentTypeID = 0,
                    PaymentID = PaymentID,
                    Source = "",
                    InternalBankAccountID = 0,
                    SuspenseType = 3,
                    PolicyID = Guid.Empty,
                    TargetPolicyNo = TargetedIndetifier,
                    PaymentDate = DatePaymentReceived,
                    Reference = PaymentReference,
                    StatusID = 0,
                    ProcessedAmount = 0,
                    CurrencyID = currencyID,
                    Balance = suspenseAmount,
                    AddedOn = DateTime.Now,
                    AddedBy = addedBy
                };
                int id = _suspenseProcessing.AddSuspenseHeader(suspenseHeader);
                SuspenseLine suspenseLine = new()
                {
                    HeaderID = id,
                    Credit = suspenseAmount,
                    Debit = 0,
                    AddedOn = DateTime.Now,
                    AddedBy = addedBy
                };
                _suspenseProcessing.AddSuspenseLine(suspenseLine);
            }
        }
        private Guid GetPolicyID(string PolicyNo)
        {
            Guid PolicyID = default;
            using (SqlConnection connection = new(Database))
            {
                connection.Open();
                string sql = "DECLARE @PolicyID uniqueidentifier='00000000-0000-0000-0000-000000000000'; SELECT @PolicyID=[ID] FROM [Policy] WHERE [PolicyNo]=@PolicyNo; SELECT @PolicyID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PolicyNo", PolicyNo);
                    PolicyID = (Guid)command.ExecuteScalar();
                }
            }
            return PolicyID;
        }
        private BilledPremium GetBill(long BatchID, Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "BilledPremiums_GetBillByPolicyID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    command.Parameters.AddWithValue("@PolicyID", PolicyID); 
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToBilledPremium(reader);
                        }
                    }
                }
                return null;
            }
        }
        
        private BilledPremium GetBillByPremiumPayer(long BatchID, int PremiumPayerID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "BilledPremiums_GetBillByPremiumPayer";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    command.Parameters.AddWithValue("@PremiumPayerID", PremiumPayerID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToBilledPremium(reader);
                        }
                    }
                }
                return null;
            }
        }
        private BillingHeader GetBatchBillByPremiumPayer(long BatchID, int PremiumPayerID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "BillingHeader_GetBatchBillByPremiumPayer";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    command.Parameters.AddWithValue("@PremiumPayerID", PremiumPayerID); 
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToBillingHeader(reader);
                        }
                    }
                }
                return null;
            }
        }
        public int GetPremiumPayerByNationalID(string NationalID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DECLARE @ID int=0; SELECT @ID=[ID] FROM [dbo].[Members] WHERE ([NormalisedNationalID]=@NormalisedNationalID); SELECT @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NormalisedNationalID", NationalID.ToUpper().Replace("-", "").Replace(" ", ""));
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int GetPremiumPayerByEmploymentNo(string EmploymentNo, int PaymentProviderID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PremiumPayer_ByEmploymentNo";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@EmploymentNo", EmploymentNo);
                    command.Parameters.AddWithValue("@PaymentProviderID", PaymentProviderID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int GetPremiumPayerByBankAccountNo(string BankAccountNo, int PaymentProviderID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PremiumPayer_BankAccountNo";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@BankAccountNo", BankAccountNo);
                    command.Parameters.AddWithValue("@PaymentProviderID", PaymentProviderID);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public int AddPremiumHeader(PremiumHeader premiumHeader)
        {
            int headerID = -1;
            using (SqlConnection conn = new SqlConnection(Database))
            {
                conn.Open();
                string sql = "DECLARE @CurrentDate date=GETDATE();INSERT INTO [PremiumHeader] ([BillingID],[BilledPremiumID],[PaymentID],[TotalAmount],[DatePaymentReceived],[DatePaymentRecorded],[AddedBy],[AddedOn]) VALUES (@BillingID,@BilledPremiumID,@PaymentID,@TotalAmount,@DatePaymentReceived,@CurrentDate,@AddedBy,@CurrentDate);SELECT SCOPE_IDENTITY()";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@BillingID", premiumHeader.BillingID);
                    cmd.Parameters.AddWithValue("@BilledPremiumID", premiumHeader.BilledPremiumID);
                    cmd.Parameters.AddWithValue("@PaymentID", premiumHeader.PaymentID);
                    cmd.Parameters.AddWithValue("@TotalAmount", premiumHeader.TotalAmount);
                    cmd.Parameters.AddWithValue("@DatePaymentReceived", premiumHeader.DatePaymentReceived);
                    cmd.Parameters.AddWithValue("@AddedBy", premiumHeader.AddedBy);
                    headerID = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            return headerID;
        }
        public void AddPremiumLine(PremiumLine premiumLine)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "INSERT INTO [PremiumLines] ([PremiumHeaderID],[PaymentMethodID],[PaymentProviderID],[CurrencyID],[Amount],[Reference]) VALUES (@PremiumHeaderID,@PaymentMethodID,@PaymentProviderID,@CurrencyID,@Amount,@Reference)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PremiumHeaderID", premiumLine.PremiumHeaderID);
                    command.Parameters.AddWithValue("@PaymentMethodID", premiumLine.PaymentMethodID);
                    command.Parameters.AddWithValue("@PaymentProviderID", premiumLine.PaymentProviderID);
                    command.Parameters.AddWithValue("@CurrencyID", premiumLine.CurrencyID);
                    command.Parameters.AddWithValue("@Amount", premiumLine.Amount);
                    command.Parameters.AddWithValue("@Reference", premiumLine.Reference);
                    command.ExecuteNonQuery();

                }
            }
        }
        public void UpdatePolicyStatus(Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "Policies_UpdateStatus";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdatePremiumCommencement(int BillID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "PolicyPremium_SetCommencementDate";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@BillID", BillID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateBilledPolicies(int BillID, Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "UPDATE [BilledPolicies] SET [Paid]=1 WHERE [BillID]=@BillID AND [PolicyID]=@PolicyID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@BillID", BillID);
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateBilledPolicies(int BillID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "UPDATE [BilledPolicies] SET [Paid]=1 WHERE [BillID]=@BillID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@BillID", BillID); 
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateBilledPremiums(int BillID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "UPDATE [BilledPremiums] SET [Paid]=1 WHERE [BillID]=@BillID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@BillID", BillID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateBilledPremiums(int BillID, Guid PolicyID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "UPDATE [BilledPremiums] SET [Paid]=1 WHERE [BillID]=@BillID AND [PolicyID]=@PolicyID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@BillID", BillID);
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateBillingHeader(int BillID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "BillingHeader_UpdatePaidStatus";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@BillID", BillID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateBillingBatch(long BatchID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "BillingBatches_UpdatePaidStatus";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void PremiumBreakDown(int PremiumID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PremiumBreakdown_SaveBPPComponents";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PremiumID", PremiumID);
                    command.ExecuteNonQuery(); 
                }
            }
        }
        public void PremiumBreakDownSaveAcquisitionExpenses(int PremiumID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PremiumBreakdown_SaveAcquisitionExpenses";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PremiumID", PremiumID);
                    command.ExecuteNonQuery(); 
                }
            }
        }
        private void BuyUnits(int PremiumID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "PolicyUnits_Buy";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PremiumID", PremiumID); 
                    command.ExecuteNonQuery();
                }
            }
        }
        private bool Save(decimal Amount, int PremiumID, int PolicyTypesExpensesID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "SET NOCOUNT OFF;INSERT INTO [PremiumBreakDown] ([PolicyTypesExpensesID],[PremiumID],[Amount]) VALUES (@PolicyTypesExpensesID, @PremiumID, @Amount)";
                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    using (connection)
                    {
                        cmd.Parameters.AddWithValue("@Amount", Amount);
                        cmd.Parameters.AddWithValue("@PremiumID", PremiumID);
                        cmd.Parameters.AddWithValue("@PolicyTypesExpensesID", PolicyTypesExpensesID);
                        return Convert.ToBoolean(cmd.ExecuteNonQuery());
                    }
                }
            }
        }
        private bool AddMainProductExpenseType(int PremiumID, int ExpenseTypeID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PremiumBreakdown_SaveMainProductComponent";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (connection)
                    { 
                        cmd.Parameters.AddWithValue("@PremiumID", PremiumID);
                        cmd.Parameters.AddWithValue("@ExpenseTypeID", ExpenseTypeID);
                        return Convert.ToBoolean(cmd.ExecuteNonQuery());
                    }
                }
            }
        }
        private DataTable GetExpenseType(Guid PolicyID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            string query = "SELECT [PolicyTypesExpenses].[ID],[ExpenseTypeID],[Ispercentage],[Amount],[MainProductOnly] FROM [dbo].[PolicyTypesExpenses] " +
                "LEFT JOIN [PolicyTypes] ON [PolicyTypes].[ID]=[PolicyTypesExpenses].[PolicyTypeID] " +
                "LEFT JOIN [Policy] ON [Policy].[PolicyType]=[PolicyTypes].[ID] WHERE [PolicyTypesExpenses].[Archived]=0" +
                "AND [Policy].[ID]=@PolicyID";
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@PolicyID", PolicyID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        private bool CheckAggregation(int ProviderID)
        {
            using (SqlConnection connection = new(Database))
            {
                connection.Open();
                string query = "DECLARE @Aggregated int=0; SELECT TOP (1) @Aggregated=[Aggregated] FROM [dbo].[PremiumCollectionConfigHeader] WHERE [PaymentProviderID]=@ProviderID AND [Archived]=0; SELECT @Aggregated";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProviderID", ProviderID);
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }
        public List<BilledPremium> GetBilledPremiums(int BillID)
        {
            List<BilledPremium> billedPremiums = new List<BilledPremium>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "BilledPremiums_GetByBillID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@BillID", BillID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            BilledPremium bp = MapToBilledPremium(reader);
                            billedPremiums.Add(bp);
                        }
                    }
                }
            }
            return billedPremiums;
        }
        public List<BilledPremium> GetBilledPremiums(int BillID, Guid PolicyID)
        {
            List<BilledPremium> billedPremiums = new List<BilledPremium>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "BilledPremiums_GetByPolicyByBillID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@BillID", BillID);
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            BilledPremium bp = MapToBilledPremium(reader);
                            billedPremiums.Add(bp);
                        }
                    }
                }
            }
            return billedPremiums;
        }
        private BillingHeader MapToBillingHeader(SqlDataReader reader)
        {
            return new BillingHeader
            {
                ID = (int)reader["ID"],
                BatchID = (long)reader["BatchID"],
                BillID = (int)reader["BillID"],
                PCCID = (int)reader["PCCID"],
                InvoiceNo = reader["InvoiceNo"] == DBNull.Value ? null : (string)reader["InvoiceNo"],
                MemberID = (int)reader["MemberID"],
                PaymentProviderID = (int)reader["PaymentProviderID"],
                PaymentMethodID = (int)reader["PaymentMethodID"],
                CurrencyID = (int)reader["CurrencyID"],
                TotalAmount = (decimal)reader["TotalAmount"],
                Paid = (byte)reader["Paid"],
                Printed = (byte)reader["Printed"]
            };
        }
        private BilledPremium MapToBilledPremium(SqlDataReader reader)
        {
            return new BilledPremium
            {
                ID = (int)reader["ID"],
                BatchID = (long)reader["BatchID"],
                BillID = reader["BillID"] != DBNull.Value ? (int?)reader["BillID"] : null,
                PCCID = (int)reader["PCCID"],
                MemberID = (int)reader["MemberID"],
                PolicyID = (Guid)reader["PolicyID"],
                PolicyPremiumID = (int)reader["PolicyPremiumID"],
                CurrencyID = (int)reader["CurrencyID"],
                Amount = (decimal)reader["Amount"],
                PaymentMethodID = (byte)reader["PaymentMethodID"],
                PaymentProviderID = reader["PaymentProviderID"] != DBNull.Value ? (int?)reader["PaymentProviderID"] : null,
                Paid = reader["Paid"] != DBNull.Value ? (byte?)reader["Paid"] : null,
                AddedOn = reader["AddedOn"] != DBNull.Value ? (DateTime?)reader["AddedOn"] : null
            };
        }
        private bool SavePaymentsData()
        {
            using (SqlConnection connection = new(Database))
            {
                connection.Open();
                string query = "INSERT INTO Payments (BatchID, CurrencyID, Amount, Reference, PaymentMethod, PaymentType, PaymentProvider, PaidBy, PaymentDate, AddedOn, AddedBy, Details)" +
                    "SELECT " + BillingBatchID + "," + currencyID +"," + amountColumnName + "," + referenceColumnName + "," + paymentMethod + "," + 0 +"," + paymentProvider + "," + paymentProvider + "," + datePaidColumnName +  ",GetDate()," + "'" + addedBy + "',"  + identifierColumnName + " FROM [dbo].[ExcelUploadData] WHERE [MediaUploadID]=@UploadID";
                 using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@UploadID", UploadID);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
        private DataTable GetPaymentData(long BatchID)
        {
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            string query = "SELECT [Details],[Amount],[Reference],[PaymentDate],[ID] FROM [dbo].[Payments]WHERE [BatchID]=@BatchID";
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@BatchID", BatchID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        public decimal GetBasicPolicyPremium(int PremiumHeaderID)
        {
            using (SqlConnection connection = new(Database))
            {
                connection.Open();
                string query = "SELECT [BasicPolicyPremium] FROM [dbo].[PremiumHeader] WHERE [ID]=@PremiumHeaderID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PremiumHeaderID", PremiumHeaderID);
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }
        public void AddCommission(int PremiumHeaderID, int PolicyPremiumID, long BillingBatchID)
        {
            CommissionLineParameters commissionLineParameters = GetCommissionLine(PremiumHeaderID);
            commissionLineParameters.AddedBy = addedBy;
            commissionLineParameters.BatchID = BillingBatchID;
            foreach (PolicyPremiumLine policyPremiumLine in GetPolicyPremiumLines(PolicyPremiumID))
            {
                commissionLineParameters.ProductID = policyPremiumLine.PolicyProductID;
                commissionLineParameters.PolicyPremiumLinesID = policyPremiumLine.ID;
                if (policyPremiumLine.IsMainProduct)
                {
                    //Run for main ;
                    commissionLineParameters.BasicPolicyPremium = GetBasicPolicyPremium(PremiumHeaderID);
                    commissionLineParameters.MainProduct = 1;
                }
                else
                {
                    //Run for riders
                    commissionLineParameters.BasicPolicyPremium = policyPremiumLine.Premium;
                    commissionLineParameters.MainProduct = 0;
                }
                AddCommissionLines(commissionLineParameters);
            }
        }
        public void AddCommissionLines(CommissionLineParameters commissionLineParameters)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "Commissions_Generate";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@PolicyTypeID", commissionLineParameters.PolicyTypeID);
                    command.Parameters.AddWithValue("@PolicyID", commissionLineParameters.PolicyID);
                    command.Parameters.AddWithValue("@PremiumID", commissionLineParameters.PremiumID);
                    command.Parameters.AddWithValue("@BatchID", commissionLineParameters.BatchID);
                    command.Parameters.AddWithValue("@ProductID", commissionLineParameters.ProductID);
                    command.Parameters.AddWithValue("@MainProduct", commissionLineParameters.MainProduct);
                    command.Parameters.AddWithValue("@PolicyPremiumID", commissionLineParameters.PolicyPremiumID);
                    command.Parameters.AddWithValue("@BasicPolicyPremium", commissionLineParameters.BasicPolicyPremium);
                    command.Parameters.AddWithValue("@PolicyPremiumLinesID", commissionLineParameters.PolicyPremiumLinesID);
                    command.Parameters.AddWithValue("@BilledPremiumID", commissionLineParameters.BilledPremiumID);
                    command.Parameters.AddWithValue("@CurrencyID", commissionLineParameters.CurrencyID);
                    command.Parameters.AddWithValue("@DueDate", commissionLineParameters.DueDate);
                    command.Parameters.AddWithValue("@DueYear", commissionLineParameters.DueYear);
                    command.Parameters.AddWithValue("@DueMonth", commissionLineParameters.DueMonth);
                    command.Parameters.AddWithValue("@PolicyAge", commissionLineParameters.PolicyAge);
                    command.Parameters.AddWithValue("@CommencementDate", commissionLineParameters.CommencementDate);
                    command.Parameters.AddWithValue("@AddedBy", commissionLineParameters.AddedBy); 
                    command.ExecuteNonQuery();
                }
            }
        }
        public List<PolicyPremiumLine> GetPolicyPremiumLines(int PolicyPremiumID)
        {
            List<PolicyPremiumLine> policyPremiumLines = new List<PolicyPremiumLine>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "PolicyPremium_GetLines";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("PolicyPremiumID", PolicyPremiumID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PolicyPremiumLine policyPremiumLine = new()
                            {
                                PolicyID = Guid.Parse(reader["PolicyID"].ToString()),
                                PolicyType = Guid.Parse(reader["PolicyType"].ToString()),
                                ID = (int)reader["PolicyPremiumsLineID"],
                                Premium= (decimal)reader["Premium"],
                                PolicyProductID = Guid.Parse(reader["ProductID"].ToString()),
                                IsMainProduct = (bool)reader["IsMainProduct"]
                            };
                            policyPremiumLines.Add(policyPremiumLine);
                        }
                    }
                }
            }
            return policyPremiumLines;
        }
        public bool UpdateBatchBalances(long BatchID)
        {
            using (SqlConnection connection = new(Database))
            {
                connection.Open();
                string query = "DECLARE @PaidTotalAmount decimal(18,2)=0; DECLARE @PolicyBalance decimal(18,2)=0; DECLARE @SystemBalance decimal(18,2)=0; DECLARE @AllocationBalance decimal(18,2)=0; SELECT @PolicyBalance=ISNULL(SUM([Balance]),0) FROM [dbo].[SuspenseHeader] WHERE [BatchID]=@BatchID AND [Reversed]=0 AND [SuspenseType]=2;SELECT @SystemBalance=ISNULL(SUM([Balance]),0) FROM [dbo].[SuspenseHeader] WHERE [BatchID]=@BatchID AND [Reversed]=0 AND [SuspenseType]=3; SELECT @PolicyBalance=ISNULL(SUM([Balance]),0) FROM [dbo].[SuspenseHeader] WHERE [BatchID]=@BatchID AND [Reversed]=0 AND [SuspenseType]=2; SELECT @AllocationBalance=ISNULL(SUM([Amount]),0) FROM [dbo].[BilledPremiums] WHERE [BatchID]=@BatchID AND [Reversed]=0 AND [Paid]=1;SELECT @PaidTotalAmount=ISNULL(SUM([Amount]),0) FROM [dbo].[Payments] WHERE [BatchID]=@BatchID AND [Reversed]=0; UPDATE [dbo].[BillingBatches] SET [AllocationSuspenseAmount]=@AllocationBalance,[SystemSuspenseAmount]=@SystemBalance,[PolicySuspenseAmount]=@PolicyBalance,[PaidTotalAmount]=@PaidTotalAmount WHERE [BatchID]=@BatchID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }
        public bool UpdateCommissionGrossAmounts(long BatchID)
        {
            using (SqlConnection connection = new(Database))
            {
                connection.Open();
                string query = "IntermediaryCommissionPayments_UpdateGrossAmounts";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure; 
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
        public bool GenerateMessages(long BatchID)
        {
            using (SqlConnection connection = new(Database))
            {
                connection.Open();
                string query = "BillingBatches_GenerateMessages";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    command.Parameters.AddWithValue("@AddedBy",addedBy);
                    return Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
        }
        public BillingBatch GetBillingBatchById(long batchId)
        {
            BillingBatch billingBatch = null;
            string query = "BillingBatches_GetByBatchID";
            using (SqlConnection connection = new SqlConnection(Database))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@BatchID", batchId);

                connection.Open();

                SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.Read())
                {
                    billingBatch = new BillingBatch
                    {
                        BatchID = reader.GetInt64(reader.GetOrdinal("BatchID")),
                        PCCID = reader.GetInt32(reader.GetOrdinal("PCCID")),
                        Provider = reader.GetString(reader.GetOrdinal("Provider")),
                        PaymentProviderID = reader.GetInt32(reader.GetOrdinal("PaymentProviderID")),
                        PaymentMethodID = reader.GetInt32(reader.GetOrdinal("PaymentMethodID")),
                        PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                        StatusID = reader.GetInt32(reader.GetOrdinal("StatusID")),
                        Entries = reader.GetInt32(reader.GetOrdinal("Entries")),
                        CurrencyID = reader.GetInt32(reader.GetOrdinal("CurrencyID")),
                        Currency = reader.GetString(reader.GetOrdinal("Currency")),
                        AllocationSuspenseAmount = reader.GetDecimal(reader.GetOrdinal("AllocationSuspenseAmount")),
                        PolicySuspenseAmount = reader.GetDecimal(reader.GetOrdinal("PolicySuspenseAmount")),
                        SystemSuspenseAmount = reader.GetDecimal(reader.GetOrdinal("SystemSuspenseAmount")),
                        BatchTotalAmount = reader.GetDecimal(reader.GetOrdinal("BatchTotalAmount")),
                        PaidTotalAmount = reader.GetDecimal(reader.GetOrdinal("PaidTotalAmount")),
                        AddedOn = reader.GetDateTime(reader.GetOrdinal("AddedOn"))
                    };
                }
                reader.Close();
            }
            return billingBatch;
        }
        public decimal GetBatchAllocationSuspenseBalance(long BatchID)
        {
            using (SqlConnection connection = new(Database))
            {
                connection.Open();
                string query = "SELECT ISNULL(SUM([Amount]),0) FROM [dbo].[BilledPremiums] WHERE [BatchID]=@BatchID AND [Reversed]=0";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@BatchID", BatchID);
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }
        public CommissionLineParameters GetCommissionLine(int PremiumID)
        {
            CommissionLineParameters commissionLineParameters = null;
            using (SqlConnection connection = new SqlConnection(Database))
            {
                using (SqlCommand command = new SqlCommand("CommissionLine_GetParameters", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PremiumID", PremiumID);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            commissionLineParameters = new CommissionLineParameters
                            {
                                PolicyTypeID = reader.GetGuid(reader.GetOrdinal("PolicyTypeID")),
                                PolicyID = reader.GetGuid(reader.GetOrdinal("PolicyID")),
                                PremiumID = reader.GetInt32(reader.GetOrdinal("PremiumID")),
                                CurrencyID = reader.GetInt32(reader.GetOrdinal("CurrencyID")),
                                //ProductID = reader.GetGuid(reader.GetOrdinal("ProductID")),
                               // MainProduct = reader.GetByte(reader.GetOrdinal("MainProduct")),
                                PolicyPremiumID = reader.GetInt32(reader.GetOrdinal("PolicyPremiumID")),
                               // BasicPolicyPremium = reader.GetDecimal(reader.GetOrdinal("BasicPolicyPremium")),
                                //PolicyPremiumLinesID = reader.GetInt32(reader.GetOrdinal("PolicyPremiumLinesID")),
                                BilledPremiumID = reader.GetInt32(reader.GetOrdinal("BilledPremiumID")),
                                DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
                                DueYear = reader.GetInt32(reader.GetOrdinal("DueYear")),
                                DueMonth = reader.GetInt32(reader.GetOrdinal("DueMonth")),
                                PolicyAge = reader.GetInt32(reader.GetOrdinal("PolicyAge")),
                                CommencementDate = reader.GetDateTime(reader.GetOrdinal("CommencementDate")),
                               // AddedBy = reader.GetString(reader.GetOrdinal("AddedBy"))
                            };
                        }
                    }
                }
            }
            return commissionLineParameters;
        }
    }
}
