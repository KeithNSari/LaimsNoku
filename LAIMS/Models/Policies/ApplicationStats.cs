namespace LAIMS.Models.Policies
{
    public class ApplicationStats
    {
        public int IncompleteApplications { get; set; }
        public int Completed { get; set; }
        public int AddDetails { get; set; }
        public int AddDocuments { get; set; }
        public int Questionnaires { get; set; }
        public int PaymentDetails { get; set; }
        public int Submitted { get; set; }
    }
}
