using System.Collections.Generic;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.DTO;
using AECMIS.MVC.Models;
using AECMIS.Service.DTO;
using AutoMapper;
using AutoMapper.Configuration;

namespace AECMIS.MVC.AutoMapper
{
    public class AutoMapperBootStrap
    {
        public static void InitializeMap()
        {
            var cfg = new MapperConfigurationExpression();
            AECMIS.DAL.Domain.Automapper.AutomapperBootStrap.ConfigureMap(cfg);
            cfg.CreateMap<List<PaymentPlan>, List<PaymentPlanViewModel>>().ConvertUsing(new PaymentPlanToPaymentPlanModel());
            cfg.CreateMap<List<Session>, List<SessionViewModel>>().ConvertUsing(new SessionToSessionViewModel());
            cfg.CreateMap<Student, StudentDetailsDto>().ConvertUsing(new StudentToStudentDetailsDto());

            Mapper.Initialize(cfg);
        }
    
   
    }
}