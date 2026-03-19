namespace LAIMS.Models.Premiums
{
    public class IntermediaryImport
    {
        public string EmployeeNumber { get; set; }
        public string AgentCode { get; set; }
        public string Designation { get; set; }
        public string Region { get; set; }
        public string Location { get; set; }
        public string TypeOfAgent { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Surname { get; set; }
        public DateTime? DateOfAppointment { get; set; }
        public DateTime? DateOfExit { get; set; }
        public string NationalID { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Title { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string Details { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Cellphone { get; set; }
        public string OtherCellphone { get; set; }
        public string WorkEmailAddress { get; set; }
        public string OtherEmailAddress { get; set; } 
        public string Branch { get; set; }
        public string ReportsTo { get; set; }
        public byte Archived { get; set; }
        public DateTime? ArchivedOn { get; set; }
    }
}
