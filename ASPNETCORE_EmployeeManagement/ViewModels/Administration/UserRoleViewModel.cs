using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASPNETCORE_EmployeeManagement.ViewModels.Administration
{
    //We could include RoleId property also in the UserRoleViewModel class, 
    //but as far as this view is concerned, there is a one-to-many relationship from Role to Users. 
    //So, in order not to repeat RoleId for each User, we will use ViewBag to pass RoleId from controller to the view.
    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool IsSelected { get; set; }
    }
}
