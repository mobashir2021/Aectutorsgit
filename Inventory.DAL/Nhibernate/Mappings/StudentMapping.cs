using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class StudentMapping:BaseMap<Student>
    {
        public StudentMapping()
        {
            Table("student.Student");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.AccessToComputer).Column("HasAccessToComputer");
            Map(x => x.AddressVerified).Column("AddressVerified");
            Map(x => x.Age).Column("Age");
            Map(x => x.DateOfBirth).Column("DateOfBirth").CustomType("date");
            Map(x => x.DiscountAmount).Column("DiscountedAmount").Nullable();
            Map(x => x.FirstLanguage).Column("FirstLanguage");
            Map(x => x.FirstName).Column("FirstName");
            Map(x => x.MiddleName).Column("MiddleName");
            Map(x => x.LastName).Column("LastName");
            Map(x => x.Nationality).Column("Nationality");
            Map(x => x.StudentNo).Column("StudentNo").ReadOnly();
            Map(x => x.SuffersIllness).Column("SuffersIllness");
            Map(x => x.IllnessDetails).Column("IllnessDetails");
            Map(x => x.HobbiesAndInterests).Column("HobbiesAndInterests");
            Map(x => x.IsMemberOfClubOrSociety).Column("IsMemberofClubOrSociety");
            Map(x => x.Gender).Column("Gender");
            Map(x => x.Curriculum).Column("Curriculum");
            Map(x => x.Enabled).Column("Enabled").Not.Nullable();
            Map(x => x.StudentImage).Column("StudentImage").Nullable();
            Map(x => x.ImageType).Column("ImageType").Nullable();
            References<Address>(x => x.Address).Column("AddressId");
            References(x => x.DefaultPaymentPlan).Column("PaymentPlanId");
            HasMany(x => x.EducationInstitutes).KeyColumn("StudentId").Cascade.AllDeleteOrphan().LazyLoad().Inverse();
            HasMany(x => x.Contacts).KeyColumn("StudentId").Cascade.AllDeleteOrphan().LazyLoad().Inverse();
            HasMany(x=> x.Subjects).KeyColumn("StudentId").Cascade.AllDeleteOrphan().Inverse();
            HasMany(x => x.Invoices).KeyColumn("StudentId").Cascade.All().LazyLoad();
            HasMany(x => x.SessionAttendances).KeyColumn("StudentId").Cascade.All().LazyLoad();
        }
    }
}
