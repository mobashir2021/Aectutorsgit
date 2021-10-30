using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain.DTO;
using AutoMapper;
using AutoMapper.Configuration;

namespace AECMIS.DAL.Domain.Automapper
{
    public class AutomapperBootStrap
    {

        public static void ConfigureMap(MapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PaymentPlan, DiscountedPaymentAmount>().ConvertUsing(new PaymentAmountToDiscountedPaymentAmount());
            cfg.CreateMap<TeacherAttendance, DailyTeacherAttendance>().ConvertUsing(new TeacherAttendanceToDailyTeacherAttendance());
            cfg.CreateMap<List<Student>, List<StudentSearchResultRow>>().ConvertUsing(new StudentsToStudentSearchResultRow());
            cfg.CreateMap<List<SessionRegister>, List<RegisterSearchResultRow>>().ConvertUsing(new SessionRegisterToRegisterSearchResultRow());
            cfg.CreateMap<List<Invoice>, List<InvoiceSearchResultRow>>().ConvertUsing(new InvoiceToInvoiceSearchResultRow());
            cfg.CreateMap<Invoice, InvoiceViewModel>().ConvertUsing(new InvoiceToInvoiceViewModel());
        }

        public static void InitializeMap()
        {
            var cfg = new MapperConfigurationExpression();
            ConfigureMap(cfg);
            Mapper.Initialize(cfg);            

            /*

            cfg.CreateMap<TeacherAttendance, DailyTeacherAttendance>().ConvertUsing(new TeacherAttendanceToDailyTeacherAttendance());
                cfg.CreateMap<List<Student>, List<StudentSearchResultRow>>().ConvertUsing(new StudentsToStudentSearchResultRow());
                cfg.CreateMap<List<SessionRegister>, List<RegisterSearchResultRow>>().ConvertUsing(new SessionRegisterToRegisterSearchResultRow());
                cfg.CreateMap<List<Invoice>, List<InvoiceSearchResultRow>>().ConvertUsing(new InvoiceToInvoiceSearchResultRow());
                cfg.CreateMap<Invoice, InvoiceViewModel>().ConvertUsing(new InvoiceToInvoiceViewModel()*/


        }
    }
}
