using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacationCalendarApp.Data;
using VacationCalendarApp.Dto;
using VacationCalendarApp.Extensions;
using VacationCalendarApp.Models;

namespace VacationCalendarApp.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class VacationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        //private readonly IMapper _mapper;

        //public VacationsController(ApplicationDbContext context, IMapper mapper)
        //{
        //    _context = context;
        //    _mapper = mapper;
        //}
        public VacationsController(ApplicationDbContext context)
        {
            _context = context;
           
        }

        // GET: api/Vacations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vacation>>> GetVacations()
        {
            var vacations = await _context.Vacation.Include(c=> c.Employee).ToListAsync();
            var vacationsData = new List<VacationData>();
            foreach (var v in vacations)
            {
                vacationsData.Add(new VacationData()
                {
                    Id = v.Id,
                    DateFrom = v.DateFrom,
                    DateTo = v.DateTo,
                    EmployeeFirstName = v.Employee.FirstName,
                    EmployeeLastName = v.Employee.LastName,
                    VacationType = v.VacationType
                });
            }
            return Ok(vacationsData);
            //var vacationsData = _mapper.Map<IEnumerable<Vacation>, IEnumerable<VacationData>>(vacations);
            //return Ok(vacationsData);
        }

        // GET: api/Vacations/5
        //[Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Vacation>> GetVacation(int id)
        {
            var v = await _context.Vacation.Include(c => c.Employee).FirstOrDefaultAsync(c => c.Id == id);
            var vacationData = new VacationData()
            {
                Id = v.Id,
                DateFrom = v.DateFrom,
                DateTo = v.DateTo,
                EmployeeId = v.Employee.Id,
                EmployeeFirstName = v.Employee.FirstName,
                EmployeeLastName = v.Employee.LastName,
                VacationType = v.VacationType
            };
            vacationData.VacationTypeChoices = GetVacationTypeChoices().ToList();
            if (v == null)
            {
                return NotFound();
            }

            return Ok(vacationData);

            //var vacation = await _context.Vacation.Include(c => c.Employee).FirstOrDefaultAsync(c=> c.Id == id);
            //var vacationData = _mapper.Map<Vacation, VacationData>(vacation);
            //vacationData.VacationTypeChoices = GetVacationTypeChoices().ToList();
            //if (vacation == null)
            //{
            //    return NotFound();
            //}

            //return Ok(vacationData);
        }

        [HttpGet("GetVacationTypes")]
        public  ActionResult<VacationTypeChoice> GetVacationTypes()
        {
            return Ok(GetVacationTypeChoices());
            //var vacationTypes = EnumService<VacationTypes>.GetDescriptionValuePairs();
            //IEnumerable<VacationTypeChoice> choices = vacationTypes.Select(c => new VacationTypeChoice() { Value = c.Key.ToString(), Text = c.Value });
            //return Ok(choices);
        }

        private IEnumerable<VacationTypeChoice> GetVacationTypeChoices()
        {
            return new List<VacationTypeChoice>()
            {
                new VacationTypeChoice { Value="Vacation", Text="Vacation Leave" },
                new VacationTypeChoice { Value="Sick", Text="Sick Leave" },
                new VacationTypeChoice { Value="Holiday", Text="Holiday" },
            };
        }

        // PUT: api/Vacations/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVacation(int id, Vacation vacation)
        {
            if (id != vacation.Id)
            {
                return BadRequest();
            }

            if (!ValidateDates(vacation.DateFrom, vacation.DateTo))
                return ValidationProblem("Start date is beyond end date"); 

            if (!await VacationOverLap(vacation.EmployeeId, vacation.DateFrom, vacation.DateTo, id))
            {
                _context.Entry(vacation).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VacationExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; //TODO: poruka o promjeni podataka
                    }
                }

                return NoContent();
            }

            else
            {
                return ValidationProblem("Vacation dates overlap with existing vacation for the employee");
            }
        }

        // POST: api/Vacations
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [Authorize(Roles ="Admin")]
        [HttpPost]
        public async Task<ActionResult<Vacation>> PostVacation(Vacation vacation)
        {
            if (!ValidateDates(vacation.DateFrom, vacation.DateTo))
                return ValidationProblem("Start date is beyond end date");

            if (!await VacationOverLap(vacation.EmployeeId, vacation.DateFrom, vacation.DateTo))
            {
                _context.Vacation.Add(vacation);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetVacation), new { id = vacation.Id }, vacation);
            }
            else
            {
                return ValidationProblem("Vacation dates overlap with existing vacation for the employee");
            }
        }

        // DELETE: api/Vacations/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Vacation>> DeleteVacation(int id)
        {
            var vacation = await _context.Vacation.FindAsync(id);
            if (vacation == null)
            {
                return NotFound();
            }

            _context.Vacation.Remove(vacation);
            await _context.SaveChangesAsync();

            return vacation;
        }

        private bool VacationExists(int id)
        {
            return _context.Vacation.Any(e => e.Id == id);
        }

        private bool ValidateDates(DateTime datefrom, DateTime dateto) => (datefrom.Date <= dateto.Date);


        private Task<bool> VacationOverLap(int employeeId, DateTime dateFrom, DateTime dateTo, int? editItemId = null)
        {
            var vacations = _context.Vacation.Where(c => (c.EmployeeId == employeeId) && c.Id != editItemId);

            return vacations.AnyAsync(c =>
                ((c.DateFrom.Date <= dateFrom.Date) && (dateFrom.Date <= c.DateTo.Date)) ||
                ((c.DateFrom.Date <= dateTo.Date) && (dateTo.Date <= c.DateTo.Date)));

        }
    }
}
