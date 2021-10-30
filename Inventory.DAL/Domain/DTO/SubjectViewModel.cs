using AECMIS.DAL.Domain.Enumerations;

namespace AECMIS.DAL.Domain.DTO
{
    public class SubjectViewModel
    {
        public string Name { get; set; }
        public int SubjectId { get; set; }
        public virtual Curriculum Level { get; set; }
        public virtual bool IsSelected { get; set; }
    }
}