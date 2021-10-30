using System;

namespace AECMIS.Service.DTO
{
    public class RegisterDetailViewModel
    {
        public DateTime RegisterDateTime { get; set; }
        public string RegisterDetails { get; set; } //Day (@Date) - Identifier : @From - @To  
        public int SessionId { get; set; }
    }
}