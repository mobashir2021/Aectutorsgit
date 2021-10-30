using System;
using System.Collections.Generic;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Extensions;
using AECMIS.DAL.Nhibernate.Repositories;
using System.Linq;
using AutoMapper;
using AECMIS.Service.Helpers;

namespace AECMIS.Service
{
    public class TeacherService
    {
        //Can a teacher teach at more than 1 centre
        //
        //private readonly TeacherRepository _teacherRepository;
        private readonly TeacherRepository _teacherRepository;
        private readonly IRepository<TeacherAttendance,int> _teacherAttendanceRepository;

        public TeacherService()
            : this(null, null)
        {
        }

        public TeacherService(TeacherRepository teacherRepository, IRepository<TeacherAttendance, int> teacherAttendanceRepository)
        {
            _teacherRepository = teacherRepository ?? new TeacherRepository();
            _teacherAttendanceRepository = teacherAttendanceRepository ?? new Repository<TeacherAttendance, int>();
        }

        public IEnumerable<Teacher> GetAllTeachers()
        {
            return _teacherRepository.FindAll();
        }

        public List<Teacher> GetTeachersByCentre(int tuitionCentre)
        {
            return _teacherRepository.GetTeachersByCentre(tuitionCentre);
        }

        public IEnumerable<Teacher> GetTeachersByStudent(int studentId)
        {
            return _teacherRepository.GetTeachersByStudent(studentId);
        }

        public void SaveTeacher(Teacher teacher)
        {
            //if teacher with the same name. middle and lastname exists throw exception
            if (_teacherRepository.Exists(x => x.FirstName == teacher.FirstName && x.MiddleName == teacher.MiddleName && x.LastName == teacher.LastName))
            {
                throw new Exception(Constants.DUPLICATE_TEACHER_MESSAGE);
            }

            _teacherRepository.Save(teacher);
        }

        public DailyTeacherAttendance CreateNewTeacherDailyAttendance(DateTime dateTime, List<TeacherSessionAttendance> attendances, Teacher teacher )
        {                      
            return new DailyTeacherAttendance
                       {
                           TotalTime = attendances.Sum(x => (x.Session.To - x.Session.From).TotalHours),
                           TotalLessons = attendances.Count(),
                           StartTime = attendances.Min(x => x.Session.From),
                           EndTime = attendances.Max(x => x.Session.To),
                           Date = dateTime,
                           TeacherId = teacher.Id,
                           TeacherName = teacher.FirstName + teacher.LastName,
                       };            
        }

        public static readonly Func<DateTime, List<TeacherSessionAttendance>, List<TeacherSessionAttendance>>
            GetAttendancesByDate = (time, list) => list.Where(a => a.AttendanceDateTime.Date == time).ToList();

        private static readonly Func<List<TeacherSessionAttendance>, List<TeacherAttendance>, List<DateTime>>
            GetNonExistingDates = (sessionAttendanceList, attendanceList) =>
                                  sessionAttendanceList.TakeWhile(
                                      x => (!attendanceList.Select(ea => ea.Day).Contains(x.AttendanceDateTime))).
                                      Select(x => x.AttendanceDateTime.Date).Distinct().ToList();

        private static readonly Func<IRepository<TeacherAttendance,int>,List<DateTime>,int,List<TeacherAttendance>> GetExistingAttendances = (repository, list, teacherId) => 
            repository.QueryList(x => list.Contains(x.Day)).Where(x=> x.Teacher.Id == teacherId).ToList();

        public IEnumerable<DailyTeacherAttendance> GetTeacherAttendances(DateTime from, DateTime to, Teacher teacher)
        {

            var dailyAttendances = new List<DailyTeacherAttendance>();
            var dates = from.AllDatesBetween(to).ToList();

            //all session attendances for this teacher between the required dates
            var sessionAttendances =
                _teacherRepository.GetTeacherSessionAttendances(from, to).Where(x => x.Teacher.Id == teacher.Id).ToList();

            //existing recorded attendances
            var existingAttendances = GetExistingAttendances(_teacherAttendanceRepository, dates, teacher.Id);            

            var datesofAttendancesNotSaved = GetNonExistingDates(sessionAttendances, existingAttendances);

            datesofAttendancesNotSaved.ForEach(x =>
                                                   {
                                                       var attendance = CreateNewTeacherDailyAttendance(x,
                                                                                                        GetAttendancesByDate
                                                                                                            (x,
                                                                                                             sessionAttendances),
                                                                                                        teacher);
                                                       dailyAttendances.Add(attendance);
                                                   });

            existingAttendances.ForEach(x => dailyAttendances.Add(Mapper.Map<DailyTeacherAttendance>(x)));


            return dailyAttendances;
        }

        public TeacherAttendanceDto GetAllTeachersAttendances(DateTime from, DateTime to)
        {
            List<Teacher> teachers = _teacherRepository.FindAll().ToList();
            var teacherAttendanceRecords = new List<DailyTeacherAttendance>();

            teachers.ForEach(x=> teacherAttendanceRecords.AddRange(GetTeacherAttendances(from,to,x) ) );

            return new TeacherAttendanceDto
                       {
                           Teachers = teachers,
                           From = from,
                           To = to,
                           DailyTeacherAttendances = teacherAttendanceRecords
                       };
        }


        public void Save(List<DailyTeacherAttendance> teacherAttendances)
        {
            var allTeachers = _teacherRepository.FindAll();
            teacherAttendances.ForEach(x =>
                                           {
                                               var attendance = _teacherAttendanceRepository.Get(x.AttendanceId) ??
                                                                new TeacherAttendance();

                                               attendance.Day = x.Date;
                                               attendance.ExtraTimeInMins = x.ExtraTimeInMins;
                                               attendance.ExtraWork = x.ExtraWork;
                                               attendance.NumberOfHoursWorked = x.TotalTime;
                                               attendance.TimeStarted = x.StartTime;
                                               attendance.TimeEnded = x.EndTime;
                                               attendance.Paid = x.Paid;
                                               attendance.NumberOfLessonsWorked = x.TotalLessons;
                                               attendance.Teacher = allTeachers.First(t => x.TeacherId == t.Id);
                                               attendance.TeacherCoveredFor = x.TeacherCoveredFor.HasValue? allTeachers.First(t =>t.Id ==
                                                                                      x.TeacherCoveredFor.
                                                                                          GetValueOrDefault())
                                                                                  : null;
                                               attendance.ExtraTimeInMins = x.ExtraTimeInMins;

                                               _teacherAttendanceRepository.Save(attendance);
                                           }
                );
        }
    }

}
