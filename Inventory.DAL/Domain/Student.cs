using System;
using System.Collections.Generic;
using System.Linq;
using AECMIS.DAL.Domain.Contracts;
using AECMIS.DAL.Domain.Enumerations;
using FluentValidation.Results;

namespace AECMIS.DAL.Domain
{
    public class Student:Entity, IPerson
    {        
        public Student()
        {
            Subjects = new List<StudentSubject>();
            Invoices = new List<Invoice>();
            SessionAttendances  = new List<SessionAttendance>();
            Contacts = new List<ContactPerson>();  
            EducationInstitutes = new List<EducationInstitute>();
        }

        public virtual ValidationResult IsValid()
        {
            throw new NotImplementedException();
        }

        public virtual decimal DiscountedPayment()
        {
            if (DiscountAmount > DefaultPaymentPlan.Amount)
                throw new ArgumentException("discount cannot be greater than payment plan amount");
            return DefaultPaymentPlan.Amount - DiscountAmount;
        }

        public virtual string DiscountedPaymentString()
        {
            return DiscountedPayment().ToString("C");
        }

        public virtual decimal? GetPaymentAmount(PaymentPlan paymentPlan, Invoice invoice)
        {
            if (paymentPlan == null) return null;
            if (invoice!=null && invoice.PaymentType == InvoicePaymentType.Daily) return paymentPlan.Amount;

            //if the payment plan paid is the same as students default payment plan 
            //which is eligible for discount then assume discount rate was paid
            return paymentPlan.Id != DefaultPaymentPlan.Id
                       ? paymentPlan.Amount
                       : DiscountedPayment();
        }
        //Changes added
        public virtual List<AttendanceCredit> AttendanceCredits()
        {
            return Invoices.Where(x => x.PaymentType == InvoicePaymentType.Advanced)
                .SelectMany(x => x.PaymentReciept?.AttendanceCredits ?? new List<AttendanceCredit>()).ToList();
        }

        public virtual string StudentNo { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string MiddleName { get; set; }
        public virtual int? Age { get; set; }
        public virtual DateTime DateOfBirth { get; set; }
        public virtual string Nationality { get; set; }
        public virtual string FirstLanguage { get; set; }
        public virtual IList<EducationInstitute> EducationInstitutes { get; set; }
        public virtual bool? SuffersIllness { get; set; }
        public virtual string IllnessDetails { get; set; }
        public virtual bool? AccessToComputer { get; set; }
        public virtual bool? IsMemberOfClubOrSociety { get; set; }
        public virtual string HobbiesAndInterests { get; set; }
        public virtual bool AddressVerified { get; set; }
        public virtual IList<StudentSubject> Subjects { get; set; }
        public virtual IList<ContactPerson> Contacts { get; set; }
        public virtual PaymentPlan DefaultPaymentPlan { get; set; }
        public virtual decimal DiscountAmount { get; set; }
        public virtual Curriculum Curriculum { get; set; }
        public virtual IList<Invoice> Invoices { get; set; }
        public virtual IList<SessionAttendance> SessionAttendances { get; set; }
        public virtual IAddress Address { get; set; }
        public virtual Gender? Gender { get; set; }
        public virtual bool Enabled { get; set; }
        public virtual string StudentImage { get; set; }
        public virtual string ImageType { get; set; }
    }
    
    

}
