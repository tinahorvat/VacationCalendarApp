﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacationCalendarApp.Data;
using VacationCalendarApp.Models;

namespace VacationCalendarApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class VacationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VacationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Vacations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vacation>>> GetVacations()
        {
            return await _context.Vacation.ToListAsync();
        }

        // GET: api/Vacations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Vacation>> GetVacation(int id)
        {
            var vacation = await _context.Vacation.FindAsync(id);

            if (vacation == null)
            {
                return NotFound();
            }

            return vacation;
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
                return Problem("Start date is beyond end date");

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
                return Problem("Vacation dates overlap with existing vacation for the employee");
            }
        }

        // POST: api/Vacations
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Vacation>> PostVacation(Vacation vacation)
        {
            if (!ValidateDates(vacation.DateFrom, vacation.DateTo))
                return Problem("Start date is beyond end date");

            if (!await VacationOverLap(vacation.EmployeeId, vacation.DateFrom, vacation.DateTo))
            {
                _context.Vacation.Add(vacation);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetVacation), new { id = vacation.Id }, vacation);
            }
            else
            {
                return Problem("Vacation dates overlap with existing vacation for the employee");
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