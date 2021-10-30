using System.Collections.Generic;
using AECMIS.DAL.Domain;

namespace AECMIS.Service.Comparers
{
    public class StudentSubjectComparer : IEqualityComparer<StudentSubject>
    {
        // Products are equal if their names and product numbers are equal. 
        public bool Equals(StudentSubject x, StudentSubject y)
        {

            //Check whether the compared objects reference the same data. 
            if (ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null. 
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal. 
            return x.Subject.Id == y.Subject.Id;
        }

        // If Equals() returns true for a pair of objects  
        // then GetHashCode() must return the same value for these objects. 

        public int GetHashCode(StudentSubject subject)
        {
            //Check whether the object is null 
            if (ReferenceEquals(subject, null)) return 0;

            //Get hash code for the Name field if it is not null. 
            int hashProductName = subject.Subject.Name == null ? 0 : subject.Subject.Name.GetHashCode();

            //Get hash code for the Code field. 
            int hashProductCode = subject.Subject.Id.GetHashCode();

            //Calculate the hash code for the product. 
            return hashProductName ^ hashProductCode;
        }

    }
}