namespace LAIMS.Models.Premiums
{
    public class SuspenseProcessingMessage
    {
        public bool Successful = false;
        public Dictionary<int, decimal> DeductedAmounts { get; set; }
        public string ProcessingMessage { get; set; }
    }
}
