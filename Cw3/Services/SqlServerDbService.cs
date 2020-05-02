using Cw3.DTOs.Requests;
using Cw3.DTOs.Responses;
using Cw3.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Cw3.Services
{
    public class SqlServerDbService : IStudentDbService
    {
        private const string ConString = "Data Source=db-mssql;Initial Catalog=s16770;Integrated Security=True";
        public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request)
        {   
            EnrollStudentResponse response = new EnrollStudentResponse();

            response.IndexNumber = request.IndexNumber;
            Enrollment e = new Enrollment();

            using (var con = new SqlConnection(ConString))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                var transaction = con.BeginTransaction();

                try
                {
                    com.CommandText = "SELECT IdStudy FROM Studies where Name=@name";
                    com.Parameters.AddWithValue("name", request.Studies);
                    com.Transaction = transaction;

                    var studs = com.ExecuteReader();
                    if (!studs.Read())
                    {
                        response.message = "Podanych studiow nie ma w bazie";
                        return response;
                    }
                    int idstudies = (int)studs["IdStudy"];
                    studs.Close();

                    com.CommandText = "SELECT IdEnrollment FROM Enrollment WHERE IdEnrollment >= "
                        + "(SELECT MAX(IdEnrollment) FROM Enrollment)";
                    var idenr = com.ExecuteReader();
                    if (!idenr.Read()){ }
                    int idEnroll = (int)idenr["IdEnrollment"] + 10;
                    idenr.Close();

                    com.CommandText = "SELECT idEnrollment, StartDate from Enrollment WHERE idStudy=@idStudy AND Semester=1" +
                        "ORDER BY StartDate";
                    com.Parameters.AddWithValue("idStudy", idstudies);

                    DateTime enrollDate;

                    var enrol = com.ExecuteReader();
                    if (!enrol.Read())
                    {
                        response.message = "Brak rozpoczetej rekrutacji";
                        enrollDate = DateTime.Now;
                        com.CommandText = "INSERT INTO Enrollment VALUES(@id, @Semester, @IdStud, @StartDate)";
                        com.Parameters.AddWithValue("id", idEnroll);
                        com.Parameters.AddWithValue("Semester", 1);
                        com.Parameters.AddWithValue("IdStud", idstudies);
                        com.Parameters.AddWithValue("StartDate", enrollDate);
                        enrol.Close();
                        com.ExecuteNonQuery();
                    }
                    else
                    {
                        idEnroll = (int)enrol["IdEnrollment"];
                        enrollDate = (DateTime)enrol["StartDate"];
                        enrol.Close();
                    }

                    e.IdEnrollment = idEnroll;
                    e.Semester = 1;
                    e.IdStudy = idstudies;
                    e.StartDate = enrollDate;

                    response.enrollment = e;

                    com.CommandText = "SELECT IndexNumber FROM Student WHERE IndexNumber=@indexNum";
                    com.Parameters.AddWithValue("indexNum", request.IndexNumber);

                    DateTime bDate = Convert.ToDateTime(request.Birthdate);
                    string formattedDate = bDate.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    try
                    {
                        com.CommandText = "INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) VALUES " +
                        "(@index, @fName, @lName, @birthDate, @idEnrollment)";

                        com.Parameters.AddWithValue("index", request.IndexNumber);
                        com.Parameters.AddWithValue("fName", request.FirstName);
                        com.Parameters.AddWithValue("lName", request.LastName);
                        com.Parameters.AddWithValue("birthDate", formattedDate);
                        com.Parameters.AddWithValue("idEnrollment", idEnroll);

                        response.message = "DONE";
                        com.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch(SqlException ex)
                    {
                        transaction.Rollback();
                        response.message = "Student o takim ID jest juz w bazie. " + ex.Message;
                    } 
                }
                catch (SqlException exc)
                {
                    transaction.Rollback();
                    response.message = exc.Message;
                }
                return response;
            }
        }

        public PromoteStudentResponse PromoteStudents(PromoteStudentRequest request)
        {
            PromoteStudentResponse response = new PromoteStudentResponse();

            using (var con = new SqlConnection(ConString))
            using (var com = new SqlCommand("PromoteStudents", con)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                com.Parameters.AddWithValue("@Studies", request.Studies);
                com.Parameters.AddWithValue("@semester", request.Semester);

                con.Open();
                com.ExecuteNonQuery();
                con.Close();
                response.message = "DONE";

                SqlCommand command = new SqlCommand();
                command.Connection = con;
                con.Open();

                int newSem = request.Semester + 1;

                command.CommandText = "SELECT IdStudy FROM Studies where Name=@name";
                command.Parameters.AddWithValue("name", request.Studies);

                var studs = command.ExecuteReader();
                if (!studs.Read())
                {
                      response.message = "Podanych studiow nie ma w bazie";
                      return response;
                }
                int idstudies = (int)studs["IdStudy"];
                studs.Close();

                Enrollment enrollment = new Enrollment();

                command.CommandText = "SELECT idEnrollment,idStudy,Semester,StartDate FROM Enrollment WHERE " +
                    "idStudy=@idStudy AND Semester=@Semester";
                command.Parameters.AddWithValue("idStudy", idstudies);
                command.Parameters.AddWithValue("Semester", newSem);

                var enr = command.ExecuteReader();
                if (enr.Read())
                {
                    enrollment.IdEnrollment = (int)enr["IdEnrollment"];
                    enrollment.IdStudy = (int)enr["IdStudy"];
                    enrollment.Semester = (int)enr["Semester"];
                    enrollment.StartDate = (DateTime)enr["StartDate"];
                }
                enr.Close();
                response.enrollment = enrollment;

                return response;
            }
        }

        public Student GetStudent(string index)
        {
            using (var con = new SqlConnection(ConString))
            using (var com = new SqlCommand())
            {
                com.Connection = con;

                com.CommandText = "SELECT IndexNumber, FirstName, LastName, BirthDate, IdEnrollment " +
                  "FROM Student WHERE IndexNumber=@IndexNum";
                com.Parameters.AddWithValue("IndexNum", index);
                con.Open();

                Student student = new Student();

                var stdnt = com.ExecuteReader();
                if (stdnt.Read())
                {
                    student.IndexNumber = index;
                    student.FirstName = stdnt["FirstName"].ToString();
                    student.LastName = stdnt["LastName"].ToString();
                    student.Birthdate = (DateTime)stdnt["BirthDate"];
                    student.Studies = stdnt["IdEnrollment"].ToString();
                    return student;
                }
                return null;
            } 
        }
    }
}
