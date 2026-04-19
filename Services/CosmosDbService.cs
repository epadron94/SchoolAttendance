using cosmosdb=Microsoft.Azure.Cosmos;
using School.Attendance.Models;
namespace School.Attendance.Services;

public class CosmosDbService
{
    private readonly cosmosdb.CosmosClient client;
    private readonly cosmosdb.Container StudentContainer;
    private readonly cosmosdb.Container AttendanceContainer;

    public CosmosDbService(cosmosdb.CosmosClient _client)
    {
        client = _client;
        AttendanceContainer = client.GetContainer("SchoolAttendance", "Attendance");
        StudentContainer = client.GetContainer("SchoolAttendance","Students");

    }
    public async Task<bool> RegisterAttendace()
    {
        return true;

    }
    public async Task<List<T>> GetAllItemsAsync<T>()
    {
        List<T> result = new List<T>(); 
        try
        {
            var iterator = StudentContainer.GetItemQueryIterator<T>("SELECT * FROM C");
            var response = await iterator.ReadNextAsync();
            result.AddRange(response.Resource);
            return result;
        }
        catch(Exception ex)
        {
            return null;
        }

    }

    public async Task<Student> getStudentAsync (string studentId)
    {
        Student result;
        try
        {
            var item = await StudentContainer.ReadItemAsync<Student>(studentId, new cosmosdb.PartitionKey(studentId));
            result = item.Resource;
        }
        catch(Exception ex)
        {
            return null;
        }
        return result;
    }
    [Obsolete]
    public async Task<List<StudentAttendance>> getStudentsDaily(string grado)
    {
        List<Student> result = new List<Student>();
        try
        {
           
            //GET attendance
            var query = new cosmosdb.QueryDefinition("SELECT * FROM Attendance c Where c.grade=@grado and c.attendanceDate = @fechahoy")
                                                    .WithParameter("@grado", grado)
                                                    .WithParameter("@fechahoy", DateTime.UtcNow.Date.ToString("yyyy-MM-dd 00:00:00"));
            var opts =  new cosmosdb.QueryRequestOptions{PartitionKey = new cosmosdb.PartitionKey(grado)};
            var iterator = AttendanceContainer.GetItemQueryIterator<Student>(query,null, opts);
            var response = await iterator.ReadNextAsync();
            result.AddRange(response.Resource);
            //var res = result.Where(t => t.)
            var queryLstStudents = new cosmosdb.QueryDefinition("SELECT * FROM Students c where c.grade = @grado")
                                                                .WithParameter("@grado", grado);
            var optsStudents = new cosmosdb.QueryRequestOptions{PartitionKey = new cosmosdb.PartitionKey(grado)};
            var iteratorStudents = StudentContainer.GetItemQueryIterator<Student>(queryLstStudents, null, optsStudents);
            var StudentsLst = await iteratorStudents.ReadNextAsync();


            var model = StudentsLst.Select(t => new StudentAttendance
            {
                Student = t,
                PresentToday = result.Any(l => l.id.Equals(t.id))
            }).ToList();

            return model;   
        }
        catch(Exception ex)
        {
            return null;
        }
    }

    public async Task<List<StudentAttendance>> getStudentsByGrade(string grade)
    {
        List<Student> attendance = new List<Student>();
        var queryStd = new cosmosdb.QueryDefinition("SELECT * FROM Attendance c WHERE c.grade=@grado AND c.date = @fechahoy")
                            .WithParameter("@grado", grade)
                            .WithParameter("@fechahoy", DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));
        var optsStd = new cosmosdb.QueryRequestOptions{PartitionKey = new cosmosdb.PartitionKey(grade)};
        var iteratorStd = AttendanceContainer.GetItemQueryIterator<Student>(queryStd, null, optsStd);
        var responseStd = await iteratorStd.ReadNextAsync();
        attendance.AddRange(responseStd.Resource);
        


        List<StudentAttendance> result = new List<StudentAttendance>();
        List<Student> students = new List<Student>();
        var query = new cosmosdb.QueryDefinition("SELECT * from Students c WHERE c.grade = @grado")
                                .WithParameter("@grado", grade);
        var opts = new cosmosdb.QueryRequestOptions{PartitionKey = new cosmosdb.PartitionKey(grade)};
        var iterator = StudentContainer.GetItemQueryIterator<Student>(query, null, opts);
        var response = await iterator.ReadNextAsync();
        students.AddRange(response.Resource);

        students.ForEach(t =>
        {
              var studentToday = attendance.Where(x => x.studentid.Equals(t.id)).FirstOrDefault();
              result.Add(new StudentAttendance
              {
                  id = studentToday is null ? "" : studentToday.id,
                  Student = t,
                  PresentToday = studentToday is null ? false : studentToday.present
              });
        });

        return result;

    }

    public async Task SubmitAttendance(string studentId, string grade, bool present, string attendanceId)
    {
        if(attendanceId is not null)
        {
            await PatchAttendanceAsync(attendanceId, present, grade);
        }
        else
        {
            AttendanceRegistration attendance = new AttendanceRegistration
            {
                id = Guid.NewGuid().ToString(),
                studentId = studentId,
                grade = grade,
                date = DateTime.Now.ToString("yyyy-MM-dd 00:00:00"),
                present = present
            };
            await SubmitAttendance(attendance);            
        }

        
    }

    private async Task SubmitAttendance(AttendanceRegistration attendance)
    {
        _ = await AttendanceContainer.CreateItemAsync(attendance, new cosmosdb.PartitionKey(attendance.grade));
    }

    private async Task PatchAttendanceAsync(string attendanceId,bool present, string grade)
    {

        var patchOp = new List<cosmosdb.PatchOperation>
        {
            cosmosdb.PatchOperation.Replace("/present", present)
        };
        cosmosdb.PatchItemRequestOptions opts = new cosmosdb.PatchItemRequestOptions
        {
            EnableContentResponseOnWrite = false // do no return the entire document
        };
        var response = await AttendanceContainer.PatchItemAsync<AttendanceRegistration>
                                                (attendanceId,
                                                new cosmosdb.PartitionKey(grade),
                                                patchOp,
                                                opts);
    }
}
