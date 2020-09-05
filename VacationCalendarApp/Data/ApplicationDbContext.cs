using VacationCalendarApp.Models;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VacationCalendarApp.Data
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var employeeUserEtb = modelBuilder.Entity<EmployeeUser>();
            employeeUserEtb.HasKey(eu => new { eu.EmployeeId, eu.ApplicationUserId });
            employeeUserEtb.HasIndex(eu => eu.EmployeeId).IsUnique();
            employeeUserEtb.HasIndex(eu => eu.ApplicationUserId).IsUnique();
        }

        public DbSet<VacationCalendarApp.Models.Vacation> Vacation { get; set; }

        public DbSet<VacationCalendarApp.Models.Employee> Employee { get; set; }
    }
}
