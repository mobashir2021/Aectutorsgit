using System;
using System.Collections.Generic;
using System.IO;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.Contracts;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Extensions;
using AECMIS.MVC.Helpers;
using AECMIS.Service;
using AECMIS.Service.DTO;
using AutoMapper;
using System.Linq;

namespace AECMIS.MVC.AutoMapper
{
    public class StudentToStudentDetailsDto : ITypeConverter<Student, StudentDetailsDto>
    {

        public static List<SessionViewModel> MapSessions(IEnumerable<StudentSubject> studentSubjects )
        {
            var source = new TimeTableService().GetAllSessions();
            return source.Select(x => new SessionViewModel
            {
                SessionId = x.Id,
                SessionDetails = x.SessionInfo(),
                Subjects = x.SubjectsTaughtAtSession.Select(s => new SubjectViewModel
                {
                    SubjectId = s.Id,
                    Name = s.Name,
                    Level = s.Level,
                    IsSelected =studentSubjects!=null && studentSubjects.Any(p=> p.Subject.Id == s.Id && p.Session.Id == x.Id )
                }).ToList(),
                Location = x.Location.Id,
                Day = x.Day,
                IsSelected = studentSubjects != null && studentSubjects.Any(s => s.Session.Id == x.Id)
                

            }).OrderBy(x=> x.Day).ThenBy(x=> x.Location).ToList();
        }

        private static List<EducationInstituteDto> MapEducationInstitutes(List<EducationInstitute> source)
        {
            var destination = new List<EducationInstituteDto>();
            source.ForEach(
             x => destination.Add(new EducationInstituteDto
             {
                 Name = x.Name,
                 Address = MapAddress(x.Address, null, false),
                 From =   x.From.HasValue?x.From.Value.ToString("dd/MM/yyyy"):"",
                 To = x.To.HasValue ? x.To.Value.ToString("dd/MM/yyyy") : "",
                 Id = x.Id,
                 Qualifications = x.Qualifications.Select(q =>
                         new QualificationDto
                         {
                             Result = q.Result,
                             Subject = q.Subject,
                             Year = q.Year,
                             Id = q.Id
                         }).ToList(),
                 StudentNo = x.StudentNo,
                 Type = x.Type,
                 Teacher = x.Teacher
             }));

            return destination;
        }

        private static List<ContactPersonDto> MapContacts(List<ContactPerson> source)
        {
            var destination = new List<ContactPersonDto>();
            source.ForEach(x => destination.Add(new ContactPersonDto
            {
                Id = x.Id,
                Title = x.Title,
                ContactAddress = MapAddress(x.ContactAddress, null, x.IsPrimaryContact),
                ContactName = x.ContactName,
                ContactPhone = x.ContactPhone,
                Type = x.Type,
                IsPrimaryContact = x.IsPrimaryContact
            }));

            return destination;
        }

        private static AddressDto MapAddress(IAddress source, AddressDto destination, bool adressRequired)
        {
            if (source == null) return null;
            if(destination == null) destination = new AddressDto();

            destination.Id = source.Id;
            destination.AddressLine1 = source.AddressLine1;
            destination.AddressLine2 = source.AddressLine2;
            destination.City = source.City;
            destination.PostCode = source.PostCode;
            destination.AddressRequired = adressRequired;
            return destination;
        }

        public StudentDetailsDto Convert(Student source, StudentDetailsDto destination, ResolutionContext context)
        {
            var studentDetailsDto = new StudentDetailsDto()
            {
                Id = source.Id,
                FirstName = source.FirstName,
                MiddleName = source.MiddleName,
                LastName = source.LastName,
                DateOfBirth = source.DateOfBirth.ToString("dd/MM/yyyy"),
                Gender = source.Gender,
                FirstLanguage = source.FirstLanguage,
                HobbiesAndInterests = source.HobbiesAndInterests,
                IllnessDetails = source.IllnessDetails,
                IsMemberOfClubOrSociety = source.IsMemberOfClubOrSociety,
                Nationality = source.Nationality,
                SuffersIllness = source.SuffersIllness,
                AccessToComputer = source.AccessToComputer,
                AddressVerified = source.AddressVerified,
                Curriculum = source.Curriculum,
                DiscountAmount = source.DiscountAmount,
                DefaultPaymentPlan = source.DefaultPaymentPlan.Id,
                Enabled = source.Enabled,
                Contacts = MapContacts(source.Contacts.ToList()),
                EducationInstitutes = MapEducationInstitutes(source.EducationInstitutes.ToList()),
                SessionAndSubjects = MapSessions(source.Subjects),
                //Subjects =source.Subjects.Select(x =>
                //      new SessionSubject(){SessionId = x.Session.Id, SubjectId = x.Subject.Id}).ToList(),
                MaxYear = DateTime.UtcNow.Localize().AddYears(-Constants.MinAge).Year,
                MinYear = DateTime.UtcNow.Localize().AddYears(-Constants.MaxAge).Year,
                Image = !string.IsNullOrEmpty(source.StudentImage) ? File.ReadAllBytes(source.StudentImage) : null,
                ImageType = source.ImageType
            };





            return studentDetailsDto;

        }
    }

    
}