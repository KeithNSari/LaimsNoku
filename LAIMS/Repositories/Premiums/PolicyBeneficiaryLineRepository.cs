using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Documents;
using LAIMS.Models.LifeProducts;
using LAIMS.Models.Premiums;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LAIMS.Repositories.Premiums
{
    public class PolicyBeneficiaryLineRepository: IPolicyBeneficiaryLineRepository
    {
        public readonly string Database;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public PolicyBeneficiaryLineRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            Database = _configuration.GetConnectionString("DefaultConnection");
        }		
		public void InsertPolicyBeneficiaryLine(PolicyBeneficiaryLine beneficiaryLine)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PolicyBeneficiaryLine_Add";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@HeaderID", beneficiaryLine.HeaderID);
                    command.Parameters.AddWithValue("@PolicyPremiumID", beneficiaryLine.PolicyPremiumID);
                    command.Parameters.AddWithValue("@Cover", beneficiaryLine.Cover);
                    command.Parameters.AddWithValue("@Contribution", beneficiaryLine.Contribution);
                    command.Parameters.AddWithValue("@ProductID", beneficiaryLine.ProductID);
                    command.Parameters.AddWithValue("@RequestID", beneficiaryLine.RequestID);
                    command.Parameters.AddWithValue("@Approved", beneficiaryLine.Approved);
                    command.Parameters.AddWithValue("@Current", beneficiaryLine.Current);
                    command.Parameters.AddWithValue("@AddedOn", beneficiaryLine.AddedOn ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedBy", beneficiaryLine.AddedBy ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void InsertPolicyBeneficiaryLineDocument(int PolicyBeneficiaryLineID, int ProductDocumentID,Guid DocumentID, string AddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PolicyBeneficiaryLineDocuments_Insert";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType =CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyBeneficiaryLineID", PolicyBeneficiaryLineID);
                    command.Parameters.AddWithValue("@ProductDocumentID", ProductDocumentID);
                    command.Parameters.AddWithValue("@DocumentID", DocumentID); 
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void ConfirmDocumentUpload(Guid MediaUploadID, Guid PolicyID, Guid MemberUID, Guid DocumentID)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Policy_UpdateMemberDocumentsList";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@MediaUploadID", MediaUploadID);
                    command.Parameters.AddWithValue("@MemberUID", MemberUID);
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.Parameters.AddWithValue("@DocumentID", DocumentID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void PPDetailsApproveAdditionalComponents(Guid RequestID, string AddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "AdditionalComponents_PPDetailsApprove";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy); 
                    command.ExecuteNonQuery();
                }
            }
        }
        public PolicyBeneficiaryLine GetPolicyBeneficiaryLine(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "SELECT * FROM PolicyBeneficiariesLines WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToPolicyBeneficiaryLine(reader);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }
        public List<PolicyBeneficiaryLineDetail> GetPolicyBeneficiaryLineDetailsByPolicyID(Guid PolicyID)
        {
            List<PolicyBeneficiaryLineDetail> PolicyBeneficiaryLineDetails = new List<PolicyBeneficiaryLineDetail>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "PolicyBeneficiaryLine_DetailsByPolicyID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PolicyBeneficiaryLineDetail PolicyBeneficiaryLineDetail = new PolicyBeneficiaryLineDetail
                            {
                                LineId=(int)reader["ID"],
                                Product = reader["Product"].ToString(),
                                CategoryID = (int)reader["CategoryID"],
                                LIRole = reader["Role"].ToString(),
                                FullName = reader["FullName"].ToString(),
                                Relationship = reader["Relationship"].ToString(),
                                CommencementDate= reader["CommencementDate"] != DBNull.Value ? (DateTime?)reader["CommencementDate"] : null,
                                DOB = Convert.ToDateTime(reader["DOB"]),
                                IDType = reader["IDType"].ToString(),
                                ID = reader["IDDocument"].ToString(),
                                Premium = Convert.ToDecimal(reader["Premium"]),
                                Cover = Convert.ToDecimal(reader["Cover"])
                            };
                            PolicyBeneficiaryLineDetails.Add(PolicyBeneficiaryLineDetail);
                        }
                    }
                }
            }
            return PolicyBeneficiaryLineDetails;
        }
        public List<PolicyBeneficiaryLineDetail> GetPolicyBeneficiaryLineDetailsByRequestID(Guid PolicyID, Guid RequestID)
        {
            List<PolicyBeneficiaryLineDetail> PolicyBeneficiaryLineDetails = new List<PolicyBeneficiaryLineDetail>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "PolicyBeneficiaryLine_DetailsByRequestID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PolicyBeneficiaryLineDetail PolicyBeneficiaryLineDetail = new PolicyBeneficiaryLineDetail
                            {
                                LineId = (int)reader["ID"],
                                Product = reader["Product"].ToString(),
                                CategoryID = (int)reader["CategoryID"],
                                PolicyBeneficiariesLineApproved = (byte)reader["PolicyBeneficiariesLineApproved"],
                                ProposeToArchive = (byte)reader["PolicyBeneficiariesLineProposeToArchive"],
                                LIRole = reader["Role"].ToString(),
                                FullName = reader["FullName"].ToString(),
                                Relationship = reader["Relationship"].ToString(),
                                DOB = Convert.ToDateTime(reader["DOB"]),
                                IDType = reader["IDType"].ToString(),
                                ID = reader["IDDocument"].ToString(),
                                Premium = Convert.ToDecimal(reader["Premium"]),
                                Cover = Convert.ToDecimal(reader["Cover"])
                            };
                            PolicyBeneficiaryLineDetails.Add(PolicyBeneficiaryLineDetail);
                        }
                    }
                }
            }
            return PolicyBeneficiaryLineDetails;
        }

        public List<PBLDocumentUpload> GetPolicyBeneficiaryLineDocuments(Guid PolicyID)
        {
            List<PBLDocumentUpload> PBLDocumentUploads = new List<PBLDocumentUpload>();
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string sql = "Policy_GetMemberDocumentsList";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PBLDocumentUpload pBLDocumentUpload = new PBLDocumentUpload
                            {
                                //ID = Guid.Parse(reader["ID"].ToString()),
                                MemberUID = Guid.Parse(reader["MemberUID"].ToString()),
                                FullName = reader["FullName"].ToString(),
                                DocumentID = Guid.Parse(reader["DocumentID"].ToString()),
                                //ProductDocumentID = (int)reader["ProductDocumentID"],
                                DocumentName = reader["Document"].ToString(),
                                ValidationGroupUploaded = (int)reader["ValidationGroupUploaded"],
                                ValidationGroup = reader["ValidationGroup"].ToString(),
                                MediaUploadID = Guid.Parse(reader["MediaUploadID"].ToString()),
                                Uploaded = Convert.ToBoolean(reader["Uploaded"]),
                                UploadStatus = reader["UploadStatus"].ToString(),
                                UploadedOn=reader["UploadedOn"].ToString ()
                            };
                            PBLDocumentUploads.Add(pBLDocumentUpload); 
                        }
                    }
                    foreach (PBLDocumentUpload pBLDocumentUpload in PBLDocumentUploads) 
                    { 
                      if((pBLDocumentUpload.ValidationGroupUploaded>0 )&& (!pBLDocumentUpload.Uploaded))
                        {
                            pBLDocumentUpload.UploadStatus = "VG satisfied";  
                        }
                    }
                }
            }
            return PBLDocumentUploads;
        }
        public void UpdatePolicyBeneficiaryLine(PolicyBeneficiaryLine beneficiaryLine)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "UPDATE PolicyBeneficiariesLines SET Cover = @Cover, Contribution = @Contribution, AddedOn = @AddedOn, AddedBy = @AddedBy WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", beneficiaryLine.ID); 
                    command.Parameters.AddWithValue("@Cover", beneficiaryLine.Cover);
                    command.Parameters.AddWithValue("@Contribution", beneficiaryLine.Contribution);  
                    command.Parameters.AddWithValue("@AddedOn", beneficiaryLine.AddedOn ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AddedBy", beneficiaryLine.AddedBy ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeletePolicyBeneficiaryLine(int id)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "DELETE FROM PolicyBeneficiariesLines WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void ArchiveBeneficiaryRecords(Guid PolicyID, int ID, string ArchivedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Declare @ArchivedOn datetime2(7)=GetUTCDate(); Update [dbo].[PolicyBeneficiaries] SET [Beneficiary]=0,[ArchivedBy]=@ArchivedBy,[ArchivedOn]=@ArchivedOn WHERE [ID]=@ID AND [HeaderID]=@PolicyID;Update [dbo].[PolicyBeneficiariesLines] SET [Archived]=1,[ArchivedBy]=@ArchivedBy,[ArchivedOn]=@ArchivedOn WHERE [HeaderID]=@ID;  UPDATE [dbo].[PolicyBeneficiaryLineDocuments] SET [Archived]=1,[ArchivedBy]=@ArchivedBy,[ArchivedOn]=@ArchivedOn WHERE [PolicyBeneficiaryLineID] IN (SELECT [ID] FROM [PolicyBeneficiariesLines] WHERE [HeaderID]=@ID)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@ArchivedBy", ArchivedBy);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void ArchiveBeneficiaryStagingRecords(Guid PolicyID, int ID, string ArchivedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "Declare @ArchivedOn datetime2(7)=GetUTCDate(); Update [dbo].[PolicyBeneficiariesStaging] SET [Beneficiary]=0,[ArchivedBy]=@ArchivedBy,[ArchivedOn]=@ArchivedOn WHERE [ID]=@ID AND [HeaderID]=@PolicyID;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@PolicyID", PolicyID);
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@ArchivedBy", ArchivedBy);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void ProposeBeneficiaryLineArchive(int PolicyBeneficiaryLineID, Guid RequestID, string AddedBy)
        {
            using (SqlConnection connection = new SqlConnection(Database))
            {
                connection.Open();
                string query = "PolicyBeneficiaryLine_ProposeToArchive";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PolicyBeneficiaryLineID", PolicyBeneficiaryLineID);
                    command.Parameters.AddWithValue("@RequestID", RequestID);
                    command.Parameters.AddWithValue("@AddedBy", AddedBy);
                    command.ExecuteNonQuery();
                }
            }
        }
        public DataTable GetRiskPolices(Guid MemberUID)
        {
            var Database = _configuration.GetConnectionString("DefaultConnection");
            DataTable DT = new DataTable();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Database;
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PolicyBeneficiary_GetRiskPolices";
            cmd.Parameters.AddWithValue("@MemberUID", MemberUID); 
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DT);
            return DT;
        }
        private PolicyBeneficiaryLine MapReaderToPolicyBeneficiaryLine(SqlDataReader reader)
        {
            return new PolicyBeneficiaryLine
            {
                ID = (int)reader["ID"],
                HeaderID = (int)reader["HeaderID"],
                Cover = (decimal)reader["Cover"],
                Contribution = (decimal)reader["Contribution"],
                ProductID = Guid.Parse(reader["ProductID"].ToString()),
                Current = (byte)reader["Current"],
                AddedOn = reader["AddedOn"] != DBNull.Value ? (DateTime)reader["AddedOn"] : (DateTime?)null,
                AddedBy = reader["AddedBy"] != DBNull.Value ? (string)reader["AddedBy"] : null
            };
        }
    }
}
