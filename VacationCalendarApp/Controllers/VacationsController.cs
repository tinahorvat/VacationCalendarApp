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
    [Authorize(Roles = "Admin, Employee")]
    [ApiController]
    public class VacationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        //private readonly IMapper _mapper; should use Automapper for mappings

        public VacationsController(ApplicationDbContext context)
        {
            _context = context;
           
        }

        // GET: api/Vacations
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VacationData>>> GetVacations()
        {
            var vacations = await _context.Vacation.Include(c=> c.Employee).ThenInclude(u => u.EmployeeUser!).ThenInclude(a=>a.ApplicationUser).ToListAsync();
            
            var vacationsData = new List<VacationData>();
            foreach (var v in vacations)
            {
                vacationsData.Add(new VacationData()
                {
                    Id = v.Id,
                    DateFrom = v.DateFrom,
                    DateTo = v.DateTo,
                    EmployeeId = v.EmployeeId,
                    EmployeeFirstName = v.Employee.FirstName,
                    EmployeeLastName = v.Employee.LastName,
                    UserName = v.Employee.EmployeeUser?.ApplicationUser.UserName,
                    VacationType = v.VacationType
                });
            }
            return Ok(vacationsData);
            //would use Automapper bu gettin identityServer error
            //var vacationsData = _mapper.Map<IEnumerable<Vacation>, IEnumerable<VacationData>>(vacations);
            //return Ok(vacationsData);
        }

        [HttpGet("GetEmployeesVacations")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EmployeeVacationData>>> GetEmployeesVacations()
        {
            var employees = await _context.Employee.Include(c => c.Vacations).Include(u => u.EmployeeUser!).ThenInclude(a => a.ApplicationUser).ToListAsync();

            var employeesData = new List<EmployeeVacationData>();
            foreach (var v in employees)
            {
                employeesData.Add(new EmployeeVacationData()
                {
                    EmployeeId = v.Id,
                    EmployeeFirstName = v.FirstName,
                    EmployeeLastName = v.LastName,
                    UserName = v.EmployeeUser?.ApplicationUser.UserName,
                    Vacations = v.Vacations.Select(c=> new VacationDto {  Id=c.Id, DateFrom=c.DateFrom, DateTo=c.DateTo, VacationType = c.VacationType }).ToList()
                });
            }
            return Ok(employeesData);
            //would use Automapper bu gettin identityServer error
            //var vacationsData = _mapper.Map<IEnumerable<Vacation>, IEnumerable<VacationData>>(vacations);
            //return Ok(vacationsData);
        }

        // GET: api/Vacations/5        
        [HttpGet("{id}")]
        public async Task<ActionResult<VacationData>> GetVacation(int id)
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

        }

        [HttpGet("GetCreateValues/{id}")]
        public async Task<ActionResult<VacationData>> GetCreateValues(int id)
        {
            var vacation = new VacationData();
            var employee = await _context.Employee.FindAsync(id);
            vacation.EmployeeId = employee.Id;
            vacation.EmployeeFirstName = employee.FirstName;
            vacation.EmployeeLastName = employee.LastName;
            vacation.VacationTypeChoices = GetVacationTypeChoices().ToList();

            return Ok(vacation);
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
        public async Task<IActionResult> PutVacation(int id, VacationData vacation)
        {
            if (id != vacation.Id)
            {
                return BadRequest();
            }

            if (!ValidateDates(vacation.DateFrom, vacation.DateTo))
                return ValidationProblem("Start date is beyond end date"); 

            if (!await VacationOverLap(vacation.EmployeeId, vacation.DateFrom, vacation.DateTo, id))
            {
                var vacationEntry = await _context.Vacation.FindAsync(id);
                vacationEntry.DateFrom = vacation.DateFrom;
                vacationEntry.DateTo = vacation.DateTo;
                vacationEntry.VacationType = vacation.VacationType;
                //_context.Entry(vacation).State = EntityState.Modified;

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
        [HttpPost]
        public async Task<ActionResult<VacationData>> PostVacation(VacationData vacation)
        {
            if (!ValidateDates(vacation.DateFrom, vacation.DateTo))
                return ValidationProblem("Start date is beyond end date");

            if (!await VacationOverLap(vacation.EmployeeId, vacation.DateFrom, vacation.DateTo))
            {
                Employee e = await _context.Employee.FindAsync(vacation.EmployeeId);
                Vacation newVacation = new Vacation() { Employee = e, DateFrom = vacation.DateFrom, DateTo = vacation.DateTo, VacationType = vacation.VacationType };
                _context.Vacation.Add(newVacation);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetVacations), new { id = newVacation.Id }, vacation);
            }
            else
            {
                return ValidationProblem("Vacation dates overlap with existing vacation for the employee");
            }
        }

        // DELETE: api/Vacations/5
        [Authorize(Roles = "Admin, Employee")]
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

        /// <summary>
        /// Check if vacation overlaps with another for the given employee
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="editItemId">vacation id if editing, null when creating</param>
        /// <returns></returns>
        private Task<bool> VacationOverLap(int employeeId, DateTime dateFrom, DateTime dateTo, int? editItemId = null)
        {
            var vacations = _context.Vacation.Where(c => (c.EmployeeId == employeeId) && c.Id != editItemId);

            return vacations.AnyAsync(c =>
                ((c.DateFrom.Date <= dateFrom.Date) && (dateFrom.Date <= c.DateTo.Date)) ||
                ((c.DateFrom.Date <= dateTo.Date) && (dateTo.Date <= c.DateTo.Date)));

        }
    }
}
