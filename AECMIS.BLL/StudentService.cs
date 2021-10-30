using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.Contracts;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.Nhibernate;
using AECMIS.DAL.Nhibernate.Repositories;
using AECMIS.Service.DTO;
using NHibernate;
using NHibernate.Linq;
using System.Globalization;
using System.Data;

namespace AECMIS.Service
{
    public class StudentService
    {
        private readonly IRepository<Student, int> _studentRepository;

        private readonly TimeTableService _timeTableService;
        private readonly ISession nSession;

        public StudentService():this(null)
        {
        }

        public StudentService(IRepository<Student,int> studentRepository)
        {
            nSession = SessionManager.GetSession();            
            _studentRepository = studentRepository ?? new Repository<Student, int>();
            _timeTableService = new TimeTableService();
        }

        public List<PaymentPlan> GetAllPaymentPlans()
        {
            return nSession.Query<PaymentPlan>().ToList(); //_paymentPlanRepository.FindAll().ToList();
        }

        public List<Student> GetStudentsByCentre()
        {
            throw new NotImplementedException();
        }

        public Student Get(int studentId)
        {
            return nSession.Get<Student>(studentId); //_studentRepository.Get(studentId);
        }

        public void GetStudentDetails(int? studentId)
        {
            throw new NotImplementedException();
        }

        public void DeleteStudent(int studentId)
        {
            //TODO:RoleCheck when roles in place
            var student = Get(studentId);
            _studentRepository.Delete(student);
        }

        public StudentSearchResultDto SearchStudents(StudentSearchDto studentSearch)
        {
            return new StudentRepository().SearchStudents(studentSearch);
        }

        public List<Student> GetStudentsWithNoCredits(StudentSearchDto studentSearch)
        {
            return new StudentRepository().GetStudentsWithNullCredits(studentSearch);
        }

        public void Save(StudentDetailsDto studentDetails, string imageStoragePath )
        {
            using (var tx = nSession.BeginTransaction())
            {
                var student = studentDetails.Id == 0 ? new Student() : Get(studentDetails.Id);
                if (student == null)
                    throw new InstanceNotFoundException(string.Format("Student with Id {0} does not exist",
                                                                      studentDetails.Id));
                student.FirstName = studentDetails.FirstName;
                student.MiddleName = studentDetails.MiddleName;
                student.LastName = studentDetails.LastName;
                student.DateOfBirth = DateTime.Parse(studentDetails.DateOfBirth.Split('T')[0]);              
                student.Gender = studentDetails.Gender;
                student.FirstLanguage = studentDetails.FirstLanguage;
                student.HobbiesAndInterests = studentDetails.HobbiesAndInterests;
                student.IllnessDetails = studentDetails.IllnessDetails;
                student.IsMemberOfClubOrSociety = studentDetails.IsMemberOfClubOrSociety;
                student.Nationality = studentDetails.Nationality;
                student.SuffersIllness = studentDetails.SuffersIllness;
                student.AccessToComputer = studentDetails.AccessToComputer;
                student.AddressVerified = studentDetails.AddressVerified;
                student.Curriculum = studentDetails.Curriculum.GetValueOrDefault();
                student.DiscountAmount = studentDetails.DiscountAmount.HasValue
                                             ? studentDetails.DiscountAmount.GetValueOrDefault()
                                             : 0;
                student.Enabled = studentDetails.Enabled;
                student.DefaultPaymentPlan =
                    GetAllPaymentPlans().FirstOrDefault(x => x.Id == studentDetails.DefaultPaymentPlan);
                SaveImage(studentDetails, student, imageStoragePath);


                MapContacts(studentDetails.Contacts.ToList(), student);
                MapEducationInstitutes(studentDetails.EducationInstitutes.ToList(), student);
                //MapStudentStudyPlan(studentDetails.Subjects.ToList(),student);
                MapStudentStudyPlan(studentDetails.SessionAndSubjects, student);
                //Changes added -- Don't generate initial invoice
                //GenerateInitialInvoice(student);

                nSession.Save(student);
                tx.Commit();
                studentDetails.Id = student.Id;

            }
        }

        private static void SaveImage(StudentDetailsDto studentDetails, Student student, string imageStoragePath)
        {
            if (studentDetails.Image == null || studentDetails.Image.Length == 0) return;
            if (string.IsNullOrEmpty(studentDetails.ImageType)) return;
            if (string.IsNullOrEmpty(imageStoragePath)) return;

            if (!Directory.Exists(imageStoragePath))
                Directory.CreateDirectory(imageStoragePath);
            var imageType = GetImageType(studentDetails.ImageType);
            if (string.IsNullOrEmpty(imageType)) return;

            var imageName = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(student.StudentImage))
                imageName = Path.GetFileNameWithoutExtension(student.StudentImage);
            var imageFullName = string.Format("{0}.{1}", imageName, imageType);
            var imagePath = string.Format(@"{0}\{1}", imageStoragePath, imageFullName);

            File.WriteAllBytes(imagePath,studentDetails.Image);

            student.StudentImage = imagePath;
            student.ImageType = studentDetails.ImageType;
        }

        private static string GetImageType(string imageTypeDesc)
        {
            try
            {
                var parts = imageTypeDesc.Split(':');
                return parts[1].Split(';')[0].Split('/')[1];
            }catch(Exception e)
            {
            }
            return null;
        }

        private void GenerateInitialInvoice(Student student)
        {
            //if first invoice is already paid then it cannot be modified
            var firstInvoice = student.Invoices.Count > 0 ?student.Invoices.OrderBy(x=> x.DateOfGeneration).First():null;
            if (firstInvoice!=null && firstInvoice.Status == InvoiceStatus.Paid && firstInvoice.PaymentReciept != null) return;
            if (firstInvoice == null)
            {
                firstInvoice = new Invoice {DateOfGeneration = DateTime.UtcNow, Student = student};
                student.Invoices.Add(firstInvoice);
            }

            firstInvoice.TotalAmount =  student.DefaultPaymentPlan.Amount;
            firstInvoice.NumberOfSessionsPayingFor = student.DefaultPaymentPlan.TotalSessions;
            firstInvoice.Status = InvoiceStatus.Pending;
            firstInvoice.DiscountApplied = student.DiscountAmount;
            firstInvoice.TotalAfterDiscount = student.DefaultPaymentPlan.Amount - student.DiscountAmount;
            firstInvoice.PaymentType = student.DefaultPaymentPlan.TotalSessions < 2
                                           ? InvoicePaymentType.Daily
                                           : InvoicePaymentType.Advanced;

        }


        private static IAddress MapAddress(AddressDto source, IAddress destination)
        {
            if (source == null) return null;
            if (destination == null) destination = new Address();

            destination.AddressLine1 = source.AddressLine1;
            destination.AddressLine2 = source.AddressLine2;
            destination.City = source.City;
            destination.PostCode = source.PostCode;
            return destination;
        }

        private static ContactPerson MapContact(ContactPersonDto source, Student student, ContactPerson contactPerson = null)
        {
            if (contactPerson == null) contactPerson = new ContactPerson();
            contactPerson.Title = source.Title;
            contactPerson.ContactAddress = MapAddress(source.ContactAddress, contactPerson.ContactAddress);
            contactPerson.ContactName = source.ContactName;
            contactPerson.ContactPhone = source.ContactPhone;
            contactPerson.Type = source.Type;
            contactPerson.Student = student;
            contactPerson.IsPrimaryContact = source.IsPrimaryContact;

            return contactPerson;
        }

        private static void MapContacts(List<ContactPersonDto> source, Student student)
        {
            source.ToList().ForEach(x =>
            {
                if (x.Id < 1)
                {
                    student.Contacts.Add(MapContact(x, student));
                }
                else
                {
                    var c = student.Contacts.First(c1 => c1.Id == x.Id);
                    MapContact(x, student, c);
                }

            });

            //remove contacts that are deleted
            var deletedContacts = student.Contacts.Where(x => (!source.Select(c => c.Id).Contains(x.Id)));
            deletedContacts.ToList().ForEach(x => { if (!x.IsPrimaryContact)student.Contacts.Remove(x); });

        }

        private static EducationInstitute MapEducationInstitute(EducationInstituteDto source, Student student, EducationInstitute destination = null)
        {
            if (destination == null) destination = new EducationInstitute();
            destination.From = !string.IsNullOrEmpty(source.From)? DateTime.Parse(source.From):(DateTime?)null;
            destination.To = !string.IsNullOrEmpty(source.To) ?DateTime.Parse(source.To):(DateTime?)null;
            destination.Type = source.Type;
            destination.Name = source.Name;
            destination.StudentNo = source.StudentNo;
            destination.Teacher = source.Teacher;
            MapQualifications(source.Qualifications.ToList(), destination);
            destination.Address = MapAddress(source.Address, destination.Address);
            destination.Student = student;

            var deletedQualifications = destination.Qualifications.Where(q => (!source.Qualifications.Select(nq => nq.Id).
                                                                             Contains(q.Id)));
            deletedQualifications.ToList().ForEach(dq => destination.Qualifications.Remove(dq));

            return destination;
        }

        private static void MapQualifications(List<QualificationDto> source, EducationInstitute educationInstitute)
        {
            source.ForEach(x =>
            {
                if (x.Id < 1)
                {
                    educationInstitute.Qualifications.Add(MapQualification(x, educationInstitute));
                }
                else
                {
                    var qualification = educationInstitute.Qualifications.FirstOrDefault(q => q.Id == x.Id);
                    if (qualification == null) throw new Exception(string.Format("Expected qualification with id {0}", x.Id));

                    MapQualification(x, educationInstitute, qualification);
                }
            });

        }

        private static Qualification MapQualification(QualificationDto source, EducationInstitute educationInstitute, Qualification destination = null)
        {
            if (destination == null) destination = new Qualification();
            destination.Result = source.Result;
            destination.Subject = source.Subject;
            destination.Year = source.Year;
            destination.AchievedAtInstitute = educationInstitute;
            return destination;
        }

        private void MapEducationInstitutes(List<EducationInstituteDto> source, Student student)
        {
            source.ForEach(x =>
            {
                if (x.Id < 1)
                {
                    student.EducationInstitutes.Add(MapEducationInstitute(x, student));
                }
                else
                {
                    var institute = student.EducationInstitutes.FirstOrDefault(e => e.Id == x.Id);
                    if (institute == null) throw new Exception();
                    MapEducationInstitute(x, student, institute);
                }
            });

            //remove all places that are removed by user
            var deletedInstitutes = student.EducationInstitutes.Where(x => (!source.Select(e => e.Id).
                                                                     Contains(x.Id)));
            deletedInstitutes.ToList().ForEach(x => student.EducationInstitutes.Remove(x));

        }

        private void MapStudentStudyPlan(IList<SessionViewModel> source, Student student)
        {
            source.Where(x => x.IsSelected).ToList().ForEach(x =>
                                                                     {
                                                                         var studentSubject = student.Subjects.FirstOrDefault(s => s.Session.Id == x.SessionId);
                                                                         var subjectViewModel = x.Subjects.FirstOrDefault(ss => ss.IsSelected);

                                                                         if (subjectViewModel == null)
                                                                             throw new Exception("A selected session must also have a selected subject");

                                                                         var subject = nSession.Query<Subject>().First(s => s.Id == subjectViewModel.SubjectId);
                                                                        
                                                                         if (studentSubject != null)
                                                                         {
                                                                             student.Subjects.Remove(studentSubject);
                                                                             nSession.Flush();
                                                                         }

                                                                         var session = nSession.Query<Session>().First(s => s.Id == x.SessionId);
                                                                         studentSubject = new StudentSubject()
                                                                         {
                                                                             Student = student,
                                                                             Subject = subject,
                                                                             Session = session
                                                                         };
                                                                         student.Subjects.Add(studentSubject);
                                                                     });


            //delete any sessions that are no longer attended
            var deletedSessions = student.Subjects.Where(x => (!source.Where(sm=> sm.IsSelected).Select(s => s.SessionId).
                                                                    Contains(x.Session.Id))).ToList();
            deletedSessions.ToList().ForEach(x => student.Subjects.Remove(x));
        }
        
        private void MapStudentStudyPlan(List<SessionSubject> source, Student student)
        {

            source.ForEach(x =>
            {

                var studentSubject = student.Subjects.FirstOrDefault(s => s.Session.Id == x.SessionId);
                var subject = _timeTableService.GetAllSubjects().First(s => s.Id == x.SubjectId);
                if (studentSubject != null)
                {
                    studentSubject.Subject = subject;
                }
                else
                {
                    var session = _timeTableService.GetAllSessions().First(s => s.Id == x.SessionId);
                    studentSubject = new StudentSubject()
                    {
                        Student = student,
                        Subject = subject,
                        Session = session
                    };
                    student.Subjects.Add(studentSubject);
                }
            });

            //delete any sessions that are no longer attended
            var deletedSessions = student.Subjects.Where(x => (!source.Select(s => s.SessionId).
                                                                    Contains(x.Session.Id)));
            deletedSessions.ToList().ForEach(x => student.Subjects.Remove(x));
        }
    }
}
