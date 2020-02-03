using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASPNETCORE_EmployeeManagement.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>();

            services.AddDbContextPool<AppDbContext>(
            options => options.UseSqlServer(_configuration.GetConnectionString("EmployeeDBConnection")));

            // Old way 
            services.AddMvc();
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
