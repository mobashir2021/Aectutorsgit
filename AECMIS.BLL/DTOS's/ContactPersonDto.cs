using AECMIS.DAL.Domain.Contracts;
using AECMIS.DAL.Domain.Enumerations;

namespace AECMIS.Service.DTO
{
    public class ContactPersonDto
    {
        public int Id { get; set; }
        public string ContactName { get; set; }
        public RelationType Type { get; set; }
        public AddressDto ContactAddress { get; set; }
        public IContact ContactPhone { get; set; }
        public bool IsPrimaryContact { get; set; }
        public TitleTypes Title { get; set; }

    }
}