using ASPNETCORE_EmployeeManagement.Models;
using ASPNETCORE_EmployeeManagement.ViewModels.Home;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ASPNETCORE_EmployeeManagement.Controllers
{
    [Route("Home")]
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IHostingEnvironment _hostingEnvironment;

        // Inject IEmployeeRepository using Constructor Injection
        public HomeController(IEmployeeRepository employeeRepository, IHostingEnvironment hostingEnvironment)
        {
            this._employeeRepository = employeeRepository;
            this._hostingEnvironment = hostingEnvironment;
        }

        [Route("/")]
        [Route("")]
        [Route("Index")]
        public ViewResult Index()
        {
            // retrieve all the employees
            var model = _employeeRepository.GetAllEmployees();
            // Pass the list of employees to the view
            return View(model);
        }

        // Retrieve employee name and return
        //public string Index()
        //{
        //    return _employeeRepository.GetEmployee(1).Name;
        //}

        //public JsonResult Index()
        //{
        //    return Json(new { id = 2, name = "pragim" });
        //}

        //public JsonResult Details()
        //{
        //    Employee model = _employeeRepository.GetEmployee(1);
        //    return Json(model);
        //}

        // It looks at the Request Accept Header and if it is set to application/xml, then XML data is returned. 
        // If the Accept header is set to application/json, then JSON data is returned.
        //public ObjectResult Details()
        //{
        //    Employee model = _employeeRepository.GetEmployee(1);
        //    return new ObjectResult(model);
        //}


        // The ? makes id route parameter optional. To make it required remove ?
        [Route("Details/{id?}")]
        // ? makes id method parameter nullable
        public ViewResult Details(int? id)
        {
            throw new Exception("Detail 111111111111111111111111111111111111111111111111111111111111111111111111111 Error");
            Console.WriteLine("Details 11111111111111111111111111111 Details");
            DetailsViewModel homeDetailsViewModel = new DetailsViewModel()
            {
                // If "id" is null use 1, else use the value passed from the route
                Employee = _employeeRepository.GetEmployee(id ?? 1),
                PageTitle = "Employee Details"
            };

            // Pass PageTitle and Employee model to the View using ViewData
            //ViewData["PageTitle"] = "Employee Details";
            //ViewData["Employee"] = model;

            // ViewBag we are using dynamic properties PageTitle and Employee
            //ViewBag.PageTitle = "Employee Details";
            //ViewBag.Employee = model;

            //return View();
            //return View("MyViews/Details.cshtml");

            if (homeDetailsViewModel.Employee == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", id);
            }

            return View(homeDetailsViewModel);
        }

        [Route("Create")]
        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [Route("Create")]
        [HttpPost]
        public IActionResult Create(CreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;

                // If the Photo property on the incoming model object is not null, then the user
                // has selected an image to upload.
                if (model.Photo != null)
                {
                    // The image must be uploaded to the images folder in wwwroot
                    // To get the path of the wwwroot folder we are using the inject
                    // HostingEnvironment service provided by ASP.NET Core
                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                    
                    // To make sure the file name is unique we are appending a new
                    // GUID value and and an underscore to the file name
                    string strFileName = Path.GetFileName(model.Photo.FileName);        // model.Photo.FileName will return a fullpath of image, so we need to parse it to get only the file name.
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + strFileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    // Use CopyTo() method provided by IFormFile interface to
                    // copy the file to wwwroot/images folder
                    model.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
                }

                Employee newEmployee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    // Store the file name in PhotoPath property of the employee object
                    // which gets saved to the Employees database table
                    PhotoPath = uniqueFileName
                };

                _employeeRepository.Add(newEmployee);
                return RedirectToAction("details", new { id = newEmployee.Id });
            }

            return View();
        }

        [Route("Edit")]
        [HttpGet]
        public ViewResult Edit(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            EditViewModel employeeEditViewModel = new EditViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotoPath = employee.PhotoPath
            };
            return View(employeeEditViewModel);
        }

        [Route("Edit")]
        // Through model binding, the action method parameter
        // EmployeeEditViewModel receives the posted edit form data
        [HttpPost]
        public IActionResult Edit(EditViewModel model)
        {
            // Check if the provided data is valid, if not rerender the edit view
            // so the user can correct and resubmit the edit form
            if (ModelState.IsValid)
            {
                // Retrieve the employee being edited from the database
                Employee employee = _employeeRepository.GetEmployee(model.Id);
                // Update the employee object with the data in the model object
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;

                // If the user wants to change the photo, a new photo will be
                // uploaded and the Photo property on the model object receives
                // the uploaded photo. If the Photo property is null, user did
                // not upload a new photo and keeps his existing photo
                if (model.Photo != null)
                {
                    // If a new photo is uploaded, the existing photo must be
                    // deleted. So check if there is an existing photo and delete
                    if (model.ExistingPhotoPath != null)
                    {
                        string filePath = Path.Combine(_hostingEnvironment.WebRootPath,
                            "images", model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath);
                    }
                    // Save the new photo in wwwroot/images folder and update
                    // PhotoPath property of the employee object which will be
                    // eventually saved in the database
                    employee.PhotoPath = ProcessUploadedFile(model);
                }

                // Call update method on the repository service passing it the
                // employee object to update the data in the database table
                Employee updatedEmployee = _employeeRepository.Update(employee);

                return RedirectToAction("index");
            }

            return View(model);
        }

        private string ProcessUploadedFile(CreateViewModel model)
        {
            string uniqueFileName = null;

            if (model.Photo != null)
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                string strFileName = Path.GetFileName(model.Photo.FileName);
                uniqueFileName = Guid.NewGuid().ToString() + "_" + strFileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Photo.CopyTo(fileStream);
                }
            }

            return uniqueFileName;
        }
    }
}
