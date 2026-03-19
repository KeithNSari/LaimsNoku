namespace LAIMS.Models.Membership
{
    public class ContactCard
    { 
        public string? EmailAddress { get; set; }  
        public string? CellPhone { get; set; }         
        public string? Telephone { get; set; } 
        public string? AddressLine1 { get; set; } 
        public string? AddressLine2 { get; set; } 
        public string? AddressLine3 { get; set; }
        public int CountryID { get; set; }
        public string? CountryName { get; set; }
        public int? City { get; set; }
        public string? CityName { get; set; }  
    }
}
