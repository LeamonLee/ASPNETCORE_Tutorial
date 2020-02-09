using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASPNETCORE_EmployeeManagement.Models
{
    //Specify ApplicationUser class as the generic argument for the IdentityDbContext class
    //This is how the IdentityDbContext class knows it has to work with our custom user class (in this case 'ApplicationUser' class) 
    //instead of the default built-in IdentityUser class. 
    //But if we just use the built-in IdentityUser class, then we don't need to specify it as generic parameter.
    public class AppDbContext : IdentityDbContext<ApplicationUser> // it inherits from DbContext, but needs to install Microsoft.Aspnetcore.Identity.EntityFramework
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity tables are mapped in OnModelCreating method of IdentityDbContext class
            base.OnModelCreating(modelBuilder);
            modelBuilder.Seed();

            //if you do not want to allow a role to be deleted, 
            //if there are rows in the child table(AspNetUserRoles) which point to a role in the parent table(AspNetRoles).
            //To achieve this, modify foreign keys DeleteBehavior to Restrict. We do this in OnModelCreating() method of AppDbContext class
            //
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
