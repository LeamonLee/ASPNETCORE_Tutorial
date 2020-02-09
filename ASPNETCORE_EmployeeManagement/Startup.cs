using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASPNETCORE_EmployeeManagement.Models;
using ASPNETCORE_EmployeeManagement.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;           // Leamon
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ASPNETCORE_EmployeeManagement
{
    public class Startup
    {

        private IConfiguration _configuration;

        // Notice we are using Dependency Injection here
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Method1 to configure Password setting
            //services.Configure<IdentityOptions>(options =>
            //{
            //    options.Password.RequiredLength = 10;
            //    options.Password.RequiredUniqueChars = 3;
            //    options.Password.RequireNonAlphanumeric = false;
            //});

            // Method2 to configure Password setting
            services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddEntityFrameworkStores<AppDbContext>();

            // The options parameter type is AuthorizationOptions
            // Use AddPolicy() method to create the policy
            // The first parameter is the name of the policy and the second parameter is the policy itself
            // To satisfy this policy requirements, the logged-in user must have both "Delete Role" and "Create Role" claim
            //
            services.AddAuthorization(options =>
            {
                options.AddPolicy("DeleteRolePolicy",
                    policy => policy.RequireClaim("Delete Role")
                                    .RequireClaim("Create Role"));
                
                // On the ListRoles view, we want to display Edit button ONLY IF the signed-in user has satisfied EditRolePolicy.
                //
                options.AddPolicy("EditRolePolicy", policy => policy.RequireClaim("Edit Role"));

                // If the user is (in the Admin role 
                // AND
                // has Edit Role claim type with a claim value of true
                // AND
                // the logged -in user Id is NOT EQUAL TO the Id of the Admin user being edited)
                // OR
                // (the user is super-admin role)
                options.AddPolicy("AdminEditRolePolicy", policy =>
                                                            policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));

                // By default, all handlers are called, irrespective of what a handler returns(success, failure or nothing).
                // This is because in the other handlers, there might be something else going on besides evaluating requirements, may be logging for example.
                // If you do not want the rest of the handlers to be called, when a failure is returned, 
                // set InvokeHandlersAfterFailure property to false. The default is true.
                options.InvokeHandlersAfterFailure = false;
            });

            // Register the first ManageAdminRolesAndClaimsRequirement handler
            services.AddSingleton<IAuthorizationHandler,
                                    CanEditOnlyOtherAdminRolesAndClaimsHandler>();

            // Register the second ManageAdminRolesAndClaimsRequirement handler
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();

            // To change the default access denied route, modify the code below
            // if we try to access an unauthorized resource, we will be redirected to /Administration/AccessDenied path.
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            });

            services.AddDbContextPool<AppDbContext>(
                options => options.UseSqlServer(_configuration.GetConnectionString("EmployeeDBConnection"))
            );

            // Old way 
            //services.AddMvc();

            // To apply [Authorize] attribute globally on all controllers and controller actions throughout the whole application
            services.AddMvc(config => {
                var policy = new AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });

            //services.AddMvcCore();

            // New Way
            //services.AddRazorPages();

            // New Way. Use along with endpoints.MapControllers()
            //services.AddControllers();

            //services.AddSingleton<IEmployeeRepository, MockEmployeeRepository>();
            services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }
            else
            {
                //app.UseStatusCodePagesWithReExecute("/Error/{0}");
                app.UseExceptionHandler("/Error");
            }

            // Add Default Files Middleware
            // UseDefaultFiles must be called before UseStaticFiles to serve the default file.
            // Specify foo.html as the default document
            //DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();
            //defaultFilesOptions.DefaultFileNames.Clear();
            //defaultFilesOptions.DefaultFileNames.Add("index.html");
            //app.UseDefaultFiles(defaultFilesOptions);

            // Add Static Files Middleware
            // By default, UseStaticFiles() middleware only serves the static files that are in wwwroot folder.
            //app.UseStaticFiles();

            // UseFileServer combines the functionality of UseStaticFiles, UseDefaultFiles and UseDirectoryBrowser middleware.
            //FileServerOptions fileServerOptions = new FileServerOptions();
            //fileServerOptions.DefaultFilesOptions.DefaultFileNames.Clear();
            //fileServerOptions.DefaultFilesOptions.DefaultFileNames.Add("index.html");
            app.UseFileServer();

            // it's important we add authentication middleware before the MVC middleware in the request processing pipeline.
            app.UseAuthentication();

            //app.UseMvcWithDefaultRoute();

            app.Use(async (context, next) =>
            {
                logger.LogInformation("MW1: Incoming Request");
                await next();
                logger.LogInformation("MW1: Outgoing Response");
            });

            app.Use(async (context, next) =>
            {
                logger.LogInformation("MW2: Incoming Request");
                await next();
                logger.LogInformation("MW2: Outgoing Response");
            });

            //app.Use(async (context, next) =>
            //{
            //    await context.Response.WriteAsync("Hello from 1st Middleware");
            //    await next();
            //});

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // Use along with services.AddControllers()
                endpoints.MapControllers();

                //endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");

                //endpoints.MapGet("/", async context =>
                //{
                //    //throw new Exception("Some error processing the request");
                //    string strMessage = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                //    string strKeyFromAppSettings = _configuration["MyKey"];

                //    await context.Response.WriteAsync(strKeyFromAppSettings);
                //    logger.LogInformation("MW3: Request handled and response produced");
                //});
            });
        }
    }
}
