using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VacationCalendarApp.Data;
using VacationCalendarApp.Models;

[assembly: HostingStartup(typeof(VacationCalendarApp.Areas.Identity.IdentityHostingStartup))]
namespace VacationCalendarApp.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => { 
            });
        }
    }
}