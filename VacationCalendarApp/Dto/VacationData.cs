﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VacationCalendarApp.Dto
{
    public class VacationData
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public DateTime DateFrom { get; set; }
        [Required]
        public DateTime DateTo { get; set; }
        [Required]
        public string VacationType { get; set; }

        public string EmployeeFirstName { get; set; }

        public string EmployeeLastName { get; set; }

        public string EmployeeFullName => $"{EmployeeFirstName} {EmployeeLastName}";

        [Required]
        public int EmployeeId { get; set; }

        public List<VacationTypeChoice> VacationTypeChoices { get; set; }
    }

    public class VacationTypeChoice
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
    public enum VacationTypes 
    { 
        [Description("Vacation Leave")]
        Vacation,
        [Description("Sick Leave")]
        Sick,
        [Description("Holiday")]
        Holiday 
    }
}
