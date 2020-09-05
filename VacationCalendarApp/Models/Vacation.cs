using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VacationCalendarApp.Models
{
    public partial class Vacation
    {
        public int Id { get; set; }

        [Required]
        public DateTime DateFrom { get; set; }

        [Required] 
        public DateTime DateTo { get; set; }

        [Required]
        public string VacationType { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
