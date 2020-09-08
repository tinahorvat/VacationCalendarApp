using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VacationCalendarApp.Models;

namespace VacationCalendarApp.Authorization
{
    public class UserIsOwnerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Employee>
    {
        UserManager<ApplicationUser> _userManager;

        public UserIsOwnerAuthorizationHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        protected override Task
            HandleRequirementAsync(AuthorizationHandlerContext context,
                                   OperationAuthorizationRequirement requirement,
                                   Employee resource)
        {
            if (context.User == null || resource == null)
            {
                return Task.CompletedTask;
            }

            // If not asking for CRUD permission, return.
            //shoul go to Constants
            if (requirement.Name != Constants.CreateOperationName &&
                requirement.Name != Constants.ReadOperationName &&
                requirement.Name != Constants.UpdateOperationName &&
                requirement.Name != Constants.DeleteOperationName)
            {
                return Task.CompletedTask;
            }
            if ((resource.EmployeeUser?.ApplicationUserId  == _userManager.GetUserId(context.User)))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}

