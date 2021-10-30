using System;
using System.Collections.Generic;
using System.Web;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.Domain.Extensions;

namespace AECMIS.Service.DTO
{
    public class StudentDetailsDto
    {

        public string StudentNo { get; set; }
        public int Id { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public int? Age { get; set; }
        public int MaxYear { get; set; }
        public int MinYear { get; set; }

        public bool Enabled { get; set; }
        public string DateOfBirth { get; set; }
        public string Nationality { get; set; }
        public string FirstLanguage { get; set; }
        public IList<EducationInstituteDto> EducationInstitutes { get; set; }
        public bool? SuffersIllness { get; set; }
        public string IllnessDetails { get; set; }
        public bool? AccessToComputer { get; set; }
        public bool? IsMemberOfClubOrSociety { get; set; }
        public string HobbiesAndInterests { get; set; }
        public bool AddressVerified { get; set; }
        //public IList<SessionSubject> Subjects { get; set; }
        public IList<SessionViewModel> SessionAndSubjects { get; set; }
        public IList<int> Sessions { get; set; }
        public IList<ContactPersonDto> Contacts { get; set; }
        public int? DefaultPaymentPlan { get; set; }
        public decimal? DiscountAmount { get; set; }        
        public Gender? Gender { get; set; }
        public virtual Curriculum? Curriculum { get; set; }
        public string ImageType { get; set; }
        public byte[] Image { get; set; }

        public string SearchStudentsUrl { get; set; }

        public StudentDetailsDto(int minAge, int maxAge,List<SessionViewModel> sessionAndSubjects ):this()
        {
            MaxYear = DateTime.UtcNow.Localize().AddYears(-minAge).Year;
            MinYear = DateTime.UtcNow.Localize().AddYears(-maxAge).Year;
            SessionAndSubjects = sessionAndSubjects;


        }
        public StudentDetailsDto()
        {
            EducationInstitutes = new List<EducationInstituteDto>();
            Contacts = new List<ContactPersonDto>();
            //Subjects = new List<SessionSubject>();
            SessionAndSubjects = new List<SessionViewModel>();
            Enabled = true;
            DiscountAmount = 0;
        }

    }

    
}