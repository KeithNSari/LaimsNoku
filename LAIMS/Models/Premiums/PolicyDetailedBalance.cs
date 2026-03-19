using System.Runtime.CompilerServices;

namespace LAIMS.Models.Premiums
{
    public class PolicyDetailedBalance
    {
        public string PolicyName { get; set; }
        public string Currency { get; set; }
        public decimal PolicyBalance { get; set; }
        public decimal SuspenseBalance { get; set; }
        public string PremiumPayer { get; set; }
        public string Proposer { get; set; }
    }
}
