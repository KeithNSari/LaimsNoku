namespace LAIMS.Models.Commissions
{
    public class PolicyTypeCommission
    {
        public int ID { get; set; }
        public int IntermediaryTypeID { get; set; }
        public Guid? PolicyTypeID { get; set; }
        public Guid ProductID { get; set; }
        public byte FunctionType { get; set; }
        public string FunctionName { get; set; }
        public string Calculation { get; set; }
        public decimal CommissionRate { get; set; }
        public int CPPStarts { get; set; }
        public int CPPEnds { get; set; }
    }
}
