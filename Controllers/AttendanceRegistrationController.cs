using Microsoft.AspNetCore.Mvc;
using School.Attendance.Services;
using School.Attendance.Models;
namespace School.Attendance.Controllers;

public class AttendanceRegistrationController : Controller
{
    private readonly CosmosDbService service;
    public AttendanceRegistrationController(CosmosDbService service)
    {
        this.service = service;
    }

    public async Task<IActionResult> Index()
    {
        StudentTabs tabs = new StudentTabs(); 
        tabs.grade1A = await service.getStudentsByGrade("1A");
        tabs.grade1B = await service.getStudentsByGrade("1B");
        tabs.grade1C = await service.getStudentsByGrade("1C");
        return View(tabs);

    }

    [HttpPost]
    public async Task<IActionResult> SubmitAttendance(string studentId, string grade, bool presentToday, string attendanceId)
    {
 
        await service.SubmitAttendance(studentId, grade, presentToday, attendanceId );
        return Ok();
    }

}