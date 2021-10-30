using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Nhibernate;
using AECMIS.DAL.Nhibernate.Repositories;
using AECMIS.Service.DTO;
using AECMIS.Service.Helpers;
using NHibernate;
using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AECMIS.Service
{
    public class TimeTableService
    {

        private readonly IRepository<Session, int> _sessionRepository;
        private readonly IRepository<TuitionCentre, int> _tuitionCentreRepository;
        private readonly IRepository<Subject,int> _subjectRepository;
        private readonly ISession Session;

        public TimeTableService() : this(null, null,null,null)
        {
        }

        public TimeTableService(ISession nSession, IRepository<Session, int> sessionRep, IRepository<TuitionCentre, int> tuitionCentreRepo, IRepository<Subject, int> subjectRepo)
        {
            Session = nSession ?? SessionManager.GetSession();
            _sessionRepository = sessionRep ?? new Repository<Session, int>();
            _subjectRepository = subjectRepo ?? new Repository<Subject, int>();
            _tuitionCentreRepository = tuitionCentreRepo ?? new Repository<TuitionCentre, int>();

        }


        public List<Subject> GetSubject(int subjectId)
        {
            return FindSubjects(new SearchSubjectDto() { SubjectId = subjectId });
        }


        public List<Subject> GetAllSubjects()
        {
            return FindSubjects(null);
        }


        public List<Subject> FindSubjects(SearchSubjectDto subjectSearchDto)
        {
            var query = Session.QueryOver<Subject>();

            if (subjectSearchDto != null && subjectSearchDto.Level != null)
            {
                query = query.Where(x=> x.Level == subjectSearchDto.Level);
            }

            if (subjectSearchDto != null && subjectSearchDto.SubjectId != null)
            {
                query = query.Where(x => x.Id == subjectSearchDto.SubjectId);
            }

            if (subjectSearchDto != null && !string.IsNullOrEmpty(subjectSearchDto.Name))
            {
                query.Where(x => x.Name == subjectSearchDto.Name);
            }

            return query.OrderBy(x => x.Name).Desc.List<Subject>().ToList();
        }

        public void CreateSubject(SubjectViewModel subjectViewModel)
        {

            if (_subjectRepository.Exists(x => x.Level == subjectViewModel.Level && x.Name == subjectViewModel.Name))
            {
                throw new Exception(string.Format(Constants.DUPLICATE_SUBJECT_MESSAGE, subjectViewModel.Name, subjectViewModel.Level));
            }

            _subjectRepository.Save(new Subject() { Level = subjectViewModel.Level, Name = subjectViewModel.Name });
        
        }

        public Session GetSession(int sessionId)
        {
            return FindSessions(new SearchSessionDto() { sessionId = sessionId }).FirstOrDefault();
        }

        public List<Session> GetAllSessions()
        {
            return FindSessions(null);
        }


        public List<Session> FindSessions(SearchSessionDto sessionSearchDto)
        {
            Subject sta  = null;
            var query = Session.QueryOver<Session>().
                //.Fetch(  SelectMode.JoinOnly, x=> x.SubjectsTaughtAtSession);
                Left.JoinAlias(x => x.SubjectsTaughtAtSession, () => sta);

            if (sessionSearchDto != null && sessionSearchDto.sessionId != null)
            {
                //set centre id
                query = query.Where(x => x.Id == sessionSearchDto.sessionId);
            }

            if (sessionSearchDto != null && sessionSearchDto.centreId != null)
            {
                //set centre id
                query = query.Where(x => x.Location.Id == sessionSearchDto.centreId);
            }

            if (sessionSearchDto != null && sessionSearchDto.dayOfWeek != null)
            {
                //set day of week
                query = query.Where(x => x.Day == sessionSearchDto.dayOfWeek);
            }

            //return _sessionRepository.FindAll().OrderBy(x => x.Day).ToList();
            return query.OrderBy(x=> x.Day).Desc.TransformUsing(Transformers.DistinctRootEntity).List<Session>().ToList();
        }



        public void SaveSession(SessionViewModel sessionViewModel)
        {
            var centre = _tuitionCentreRepository.Get(sessionViewModel.Location);
            List<Subject> sessionSubjects = null;
            var session = _sessionRepository.Get(sessionViewModel.SessionId);
            var allSubjects = _subjectRepository.FindAll().ToList();

            if (centre == null)
            {
                //throw exception centre unknown
                throw new Exception(Constants.INVALID_CENTRE_MESSAGE);
            }

            //var query = Session.QueryOver<Session>().
            //session time/day has to be unique
            if (sessionViewModel.SessionId < 1 && _sessionRepository.Exists(x => x.Day == sessionViewModel.Day && x.From == sessionViewModel.From && x.To == sessionViewModel.To && x.Location == centre))
            {
                //throw exception if this session already exists
                throw new Exception(Constants.DUPLICATE_SESSION_MESSAGE);
            }

            if (session == null)
            {
                session = new Session()
                {
                    From = sessionViewModel.From,
                    To = sessionViewModel.To,
                    Day = sessionViewModel.Day,
                    Location = centre
                };
            }
            //CLEAR SUBJECTS
            session.SubjectsTaughtAtSession.Clear();

            if (sessionViewModel.Subjects != null && sessionViewModel.Subjects.Count() > 0)
            {
                var newSubjects  = sessionViewModel.Subjects.Where(x => x.SubjectId < 1).ToList();
                ValidateDuplicateSubjects(allSubjects, newSubjects);

                var persistedSubjectIds = sessionViewModel.Subjects.Where(x => x.SubjectId > 0).ToList();
                sessionSubjects = allSubjects.FindAll(x => persistedSubjectIds.Select(p=> p.SubjectId).Contains(x.Id));
                sessionSubjects.ForEach(x => { session.SubjectsTaughtAtSession.Add(x); });
                newSubjects.ForEach(sub => { session.SubjectsTaughtAtSession.Add(new Subject() { Level = sub.Level, Name = sub.Name }); });
            }

            _sessionRepository.Save(session);
        
        }

        private void ValidateDuplicateSubjects(IEnumerable<Subject> subjects, List<SubjectViewModel> newSubjects )
        {
            var duplicateSubjects = subjects.Where(y => newSubjects.Any(z => z.Level == y.Level && z.Name == y.Name));
            if (duplicateSubjects.Count() > 0)
            {
                //throw exception
                //Subjects with Name in Curricum already exist
                var errorMessage = string.Empty;
                duplicateSubjects.ToList().ForEach(x => { errorMessage = string.Format(Constants.DUPLICATE_SUBJECT_MESSAGE, x.Name, x.Level.ToString()); });

                throw new Exception(errorMessage);
            }
        }

        public IEnumerable<TuitionCentre> GetAllCentres()
        {
            return _tuitionCentreRepository.FindAll();
        }


        public TuitionCentre GetCentre(int id)
        {
            return _tuitionCentreRepository.Get(id);
        }
    }
}
