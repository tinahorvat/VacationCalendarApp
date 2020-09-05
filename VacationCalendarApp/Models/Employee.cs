﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VacationCalendarApp.Models
{
    public partial class Employee
    {
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        public ICollection<Vacation> Vacations { get; set; }

        //public ApplicationUser User { get; set; }
    }

    
}
