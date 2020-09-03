using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VacationCalendarApp.Dto;
using VacationCalendarApp.Models;

namespace VacationCalendarApp.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Domain to Dto
            CreateMap<Vacation, VacationData>();            

            // Dto to Domain
            CreateMap<VacationData, Vacation>();
            
        }
    }
}
