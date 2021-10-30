using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain.DTO;
using AutoMapper;

namespace AECMIS.DAL.Domain.Automapper
{
    public class TeacherAttendanceToDailyTeacherAttendance : ITypeConverter<TeacherAttendance, DailyTeacherAttendance>
    {
        public DailyTeacherAttendance Convert(TeacherAttendance source, DailyTeacherAttendance destination, ResolutionContext context)
        {
            return new DailyTeacherAttendance
            {
                Date = source.Day,
                StartTime = source.TimeStarted,
                EndTime = source.TimeEnded,
                ExtraTimeInMins = source.ExtraTimeInMins,
                ExtraWork = source.ExtraWork,
                Paid = source.Paid,
                TeacherId = source.Teacher.Id,
                TeacherName = source.Teacher.FirstName + source.Teacher.LastName,
                TeacherCoveredFor = source.TeacherCoveredFor.Id,
                TotalLessons = source.NumberOfLessonsWorked,
                TotalTime = source.NumberOfHoursWorked,
                AttendanceId = source.Id
            };

        }
    }
}
