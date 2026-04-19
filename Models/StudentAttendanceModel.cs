
namespace School.Attendance.Models;
public class StudentAttendance
{
    public string id {get;set;}
    public Student Student {get; set;}
    public bool PresentToday {get; set;}
}