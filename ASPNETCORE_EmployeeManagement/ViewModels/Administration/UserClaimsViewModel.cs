using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASPNETCORE_EmployeeManagement.ViewModels.Administration
{
    public class UserClaimsViewModel
    {
        public UserClaimsViewModel()
        {
            Claims = new List<UserClaim>();
        }

        public string UserId { get; set; }
        public List<UserClaim> Claims { get; set; }
    }

    public class UserClaim
    {
        public string ClaimType { get; set; }
        public bool IsSelected { get; set; }
    }

    public static class DummyClaims
    {
        public static List<Claim> AllClaims = new List<Claim>()
    {
        new Claim("Create Role", "Create Role"),
        new Claim("Edit Role","Edit Role"),
        new Claim("Delete Role","Delete Role")
    };
    }
}
