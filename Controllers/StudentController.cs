using School.Attendance.Services;
using School.Attendance.Models;
using Microsoft.AspNetCore.Mvc;
using System;
namespace School.Attendance.Controllers;

public class StudentController : Controller
{
    private readonly CosmosDbService service;

    public StudentController(CosmosDbService _service)
    {
        service = _service;
    }

    
    public async Task<IActionResult> Index(string grade)
    {
        if (grade is null) return View(null);
        
            var result = await service.getStudentsDaily(grade);
            return View(result);
        
        //var lst = await service.GetAllItemsAsync<Student>();
       
    }
    
}

