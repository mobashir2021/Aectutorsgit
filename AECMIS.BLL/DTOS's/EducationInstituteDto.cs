using System;
using System.Collections.Generic;
using AECMIS.DAL.Domain.Enumerations;

namespace AECMIS.Service.DTO
{
    public class EducationInstituteDto 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public InstituteType Type { get; set; }
        public AddressDto Address { get; set; }
        public string Teacher { get; set; }
        public string StudentNo { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public IList<QualificationDto> Qualifications { get; set; }  
      
        public EducationInstituteDto()
        {
            Qualifications = new List<QualificationDto>();
        }
    }
}