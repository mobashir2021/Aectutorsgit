namespace AECMIS.Service.DTO
{
    
    public class AddressDto 
    {
        public int Id { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string PostCode { get; set; }
        public string County { get; set; }
        public string City { get; set; }       
        public bool AddressRequired { get; set; }
    }
}