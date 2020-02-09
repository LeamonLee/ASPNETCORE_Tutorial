using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASPNETCORE_EmployeeManagement.Security
{
    public class CanEditOnlyOtherAdminRolesAndClaimsHandler : AuthorizationHandler<ManageAdminRolesAndClaimsRequirement>
    {
        // Resource property of AuthorizationHandlerContext returns the resource that we are protecting.
        //
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                        ManageAdminRolesAndClaimsRequirement requirement)
        {
            // If AuthorizationFilterContext is NULL, we cannot check if the requirement is met or not, 
            // so we return Task.CompletedTask and the access is not authorised.
            //
            // context.Resource returns the controller action being protected as the AuthorizationFilterContext 
            // and provides access to HttpContext, RouteData, and everything else provided by MVC and Razor Pages.
            //
            var authFilterContext = context.Resource as AuthorizationFilterContext;
            if (authFilterContext == null)
            {
                return Task.CompletedTask;
            }

            string loggedInAdminId =
                context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            string adminIdBeingEdited = authFilterContext.HttpContext.Request.Query["userId"];

            if (context.User.IsInRole("Admin") &&
                context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") &&
                adminIdBeingEdited.ToLower() != loggedInAdminId.ToLower())
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
