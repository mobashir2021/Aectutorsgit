using System;
using System.Collections.Generic;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.Validation;

namespace AECMIS.DAL.Domain
{
    public class PaymentReciept : Entity
    {
        public virtual Invoice Invoice { get; set; }
        //public virtual decimal AmountPaid { get; set; }
        public virtual DateTime GeneratedDate { get; set; }
        public virtual InvoicePayment InvoicePayment { get; set; }

        public virtual void IsValid()
        {
            new InvoicePaymentRecieptValidator().ValidateAndThrow(this);
        }
        public virtual IList<AttendanceCredit> AttendanceCredits { get; set; }


        //1ST APPROACH
        //fill in register and save

        //??what happens if this step is missed
        //validate existing invoices either set invoices to payed or defered

        //??what happens if this step is missed
        //every day validate registers and process
        //at this point debit advance invoices generate new invoice if no credits exist e.t.c 

        //2ND APPROACH
        //fill in register and save, saves all data without any processing
        //As admin validate register and validate invoices and click "process payments and register" 
        //at this point debit advance invoices generates new invoice if no credits exist e.t.c 
        //for each student get all unpaid attendances 
        //

    }
    }