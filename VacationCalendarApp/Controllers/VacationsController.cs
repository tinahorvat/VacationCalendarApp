using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacationCalendarApp.Authorization;
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
        private readonly IAuthorizationService _authorizationService;
        private readonly UserManager<ApplicationUser> _userManager;
        //private readonly IMapper _mapper; should use Automapper for mappings

        public VacationsController(ApplicationDbContext context, IAuthorizationService authorizationService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _authorizationService = authorizationService;
            _userManager = userManager;
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
            //would use Automapper but gettin identityServer error
            
        }

        // GET: api/Vacations/5        
        [HttpGet("{id}")]
        public async Task<ActionResult<VacationData>> GetVacation(int id)
        {
            var v = await _context.Vacation.FindAsync(id);
            var employee = await _context.Employee.Include(c => c.EmployeeUser)?.ThenInclude(a => a.ApplicationUser).FirstOrDefaultAsync(a => a.Id == v.EmployeeId);
            var vacationData = new VacationData()
            {
                Id = v.Id,
                DateFrom = v.DateFrom,
                DateTo = v.DateTo,
                EmployeeId = employee.Id,
                EmployeeFirstName = employee.FirstName,
                EmployeeLastName = employee.LastName,
                VacationType = v.VacationType,
                UserName = employee.EmployeeUser?.ApplicationUser.UserName,
                rowVersion = v.RowVersion
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
            var employee = await _context.Employee.Include(c => c.EmployeeUser!).ThenInclude(a => a.ApplicationUser).FirstOrDefaultAsync(a => a.Id == id);
            vacation.EmployeeId = employee.Id;
            vacation.EmployeeFirstName = employee.FirstName;
            vacation.EmployeeLastName = employee.LastName;
            vacation.DateFrom = DateTime.Now;
            vacation.DateTo = DateTime.Now;
            vacation.VacationTypeChoices = GetVacationTypeChoices().ToList();
            vacation.UserName = employee.EmployeeUser.ApplicationUser.UserName;
            return Ok(vacation);
        }

        [HttpGet("GetVacationTypes")]
        public  ActionResult<VacationTypeChoice> GetVacationTypes()
        {
            return Ok(GetVacationTypeChoices());
            
        }

        //should use constants..
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
            var vacationEntry = await _context.Vacation.FindAsync(id);

           
            var employee = await _context.Employee.Include(c=>c.EmployeeUser).FirstOrDefaultAsync(c => c.Id == vacationEntry.EmployeeId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, employee, ContactOperations.Update);

            if (!authorizationResult.Succeeded) { return Forbid(); }

            if (!ValidateDates(vacation.DateFrom, vacation.DateTo))
                return ValidationProblem("Start date is beyond end date");
                       
            if (!await VacationOverLap(vacation.EmployeeId, vacation.DateFrom, vacation.DateTo, id))
            {                
                vacationEntry.DateFrom = vacation.DateFrom;
                vacationEntry.DateTo = vacation.DateTo;
                vacationEntry.VacationType = vacation.VacationType;

                _context.Entry(vacationEntry).Property("RowVersion").OriginalValue = vacation.rowVersion;

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
                        return Conflict("Unable to perfom action: item changed"); //TODO: poruka o promjeni podataka
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
            Employee e = await _context.Employee.Include(c=>c.EmployeeUser).FirstOrDefaultAsync(a=>a.Id ==vacation.EmployeeId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, e, ContactOperations.Create);

            if (!authorizationResult.Succeeded) { return Forbid(); }

            if (!ValidateDates(vacation.DateFrom, vacation.DateTo))
                return ValidationProblem("Start date is beyond end date");

            if (!await VacationOverLap(vacation.EmployeeId, vacation.DateFrom, vacation.DateTo))
            {                
                Vacation newVacation = new Vacation() { Employee = e, DateFrom = vacation.DateFrom, DateTo = vacation.DateTo, VacationType = vacation.VacationType };
                _context.Vacation.Add(newVacation);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetVacations), new { id = newVacation.Id }, vacation);
            }
            else
            {
                return ValidationProblem("Vacation dates overlap with existing vacation for the employee", "this", 400, "naslov");
            }
        }

        // DELETE: api/Vacations/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Vacation>> DeleteVacation(int id)
        {
            var vacation = await _context.Vacation.FindAsync(id);
            var employee = await _context.Employee.Include(c => c.EmployeeUser).FirstOrDefaultAsync(a => a.Id == vacation.EmployeeId);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, employee, ContactOperations.Delete);

            if (!authorizationResult.Succeeded) { return Forbid(); }

            if (vacation == null)
            {
                return NotFound();
            }

            _context.Vacation.Remove(vacation);
            await _context.SaveChangesAsync();

            return NoContent();
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
