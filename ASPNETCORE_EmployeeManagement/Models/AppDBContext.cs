using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASPNETCORE_EmployeeManagement.Models
{
    public class AppDbContext : IdentityDbContext // it inherits from DbContext, but needs to install Microsoft.Aspnetcore.Identity.EntityFramework
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
        }
    }
}
