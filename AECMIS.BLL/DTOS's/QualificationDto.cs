namespace AECMIS.Service.DTO
{
    public class QualificationDto 
    {
        public int Id { get; set; }
        public virtual string Subject { get; set; }
        public virtual string Result { get; set; }
        public virtual int? Year { get; set; }
    }
}