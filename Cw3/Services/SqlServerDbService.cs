using Cw3.DTOs.Requests;
using Cw3.DTOs.Responses;
using Cw3.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SimpleCrypto;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Cw3.Services
{
    public class SqlServerDbService : IStudentDbService
    {
        private const string ConString = "Data Source=db-mssql;Initial Catalog=s16770;Integrated Security=True";
        public IConfiguration Configuration { get; set; }
        public SqlServerDbService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /*public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request)
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
        }*/

        public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request)
        {
            EnrollStudentResponse response = new EnrollStudentResponse();
            ModelsAuto.Enrollment e = new ModelsAuto.Enrollment();

            response.IndexNumber = request.IndexNumber;

            var db = new ModelsAuto.s16770Context();

            var studies = db.Studies.Where(s => s.Name == request.Studies);
            if (studies.Count() == 0)
            {
                response.message = "Podanych studiow nie ma w bazie";
                return response;
            }
            int idstudies = studies.First().IdStudy;

            var enrollment = db.Enrollment.Where(e => e.IdStudy == idstudies)
                                          .Where(e => e.Semester == 1)
                                          .OrderBy(e => e.StartDate);

            DateTime enrollDate;
            var newEnrollment = db.Enrollment.OrderBy(e => e.IdEnrollment).Last().IdEnrollment;
            int newEnrollmentId = newEnrollment + 10; 

            if (enrollment.Count() == 0)
            {
                response.message = "Brak rozpoczetej rekrutacji";
                enrollDate = DateTime.Now;

                var enr = new ModelsAuto.Enrollment
                {
                    IdEnrollment = newEnrollmentId,
                    Semester = 1,
                    IdStudy = idstudies,
                    StartDate = enrollDate
                };

                db.Enrollment.Add(enr);
                db.SaveChanges();
            }
            else
            {
                newEnrollmentId = enrollment.Single().IdEnrollment;
                enrollDate = (DateTime)enrollment.Single().StartDate;
            }

            e.IdEnrollment = newEnrollmentId;
            e.Semester = 1;
            e.IdStudy = idstudies;
            e.StartDate = enrollDate;

            response.enrollment = e;

            DateTime bDate = Convert.ToDateTime(request.Birthdate);
            //string formattedDate = bDate.ToString("yyyy-MM-dd HH:mm:ss.fff");

            var student = new ModelsAuto.Student
            {
                IndexNumber = request.IndexNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = bDate,
                IdEnrollment = newEnrollmentId
            };

            db.Student.Add(student);
            try
            {
                db.SaveChanges();
            }
            catch(Exception ex)
            {
                response.message = "Student o takim ID jest juz w bazie. " + ex.Message;
            }
        
            response.message = "DONE";
            return response;
        }

        /*public PromoteStudentResponse PromoteStudents(PromoteStudentRequest request)
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
        }*/

        public PromoteStudentResponse PromoteStudents(PromoteStudentRequest request)
        {
            PromoteStudentResponse response = new PromoteStudentResponse();

            var db = new ModelsAuto.s16770Context();

            var studies = db.Studies.Where(s => s.Name == request.Studies);
            if (studies.Count() == 0)
            {
                response.message = "Podanych studiow nie ma w bazie";
                return response;
            }
            int idstudies = studies.First().IdStudy;

            ModelsAuto.Enrollment enrollment = new ModelsAuto.Enrollment();

            object[] parameters =
            {
                new SqlParameter("@Studies", request.Studies),
                new SqlParameter("@Semester", request.Semester)
            };

            var promotion = db.Database.ExecuteSqlRaw("PromoteStudents @Studies, @Semester", parameters);
            var enr = db.Enrollment.Where(e => e.IdStudy == idstudies)
                                   .Where(e => e.Semester == request.Semester+1);

            int idEnr = enr.Single().IdEnrollment;
            int idStu = enr.Single().IdStudy;
            int sem = enr.Single().Semester;
            DateTime sDate = enr.Single().StartDate;

            enrollment.IdEnrollment = idEnr;
            enrollment.IdStudy = idStu;
            enrollment.Semester = sem;
            enrollment.StartDate = sDate;

            response.enrollment = enrollment;

            return response;
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
        

        public LoginResponse Login(string login, string haslo)
        {
            LoginResponse response = new LoginResponse();
            Student student = new Student();

            ICryptoService crypto = new PBKDF2();

            using (SqlConnection con = new SqlConnection(ConString)) 
            using (SqlCommand com = new SqlCommand())
            {

                com.Connection = con;

                com.CommandText = "SELECT IndexNumber, Passwd, Salt FROM STUDENT WHERE IndexNumber=@index";
                com.Parameters.AddWithValue("index", login);

                string passwd, dbPasswd, dbSalt;

                con.Open();
                SqlDataReader loginRead = com.ExecuteReader();
                if (loginRead.Read())
                {
                    student.IndexNumber = loginRead["IndexNumber"].ToString();
                    passwd = loginRead["Passwd"].ToString();
                    dbSalt = loginRead["Salt"].ToString();
                    dbPasswd = crypto.Compute(haslo, dbSalt);
                    if (!crypto.Compare(passwd, dbPasswd))
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }

                con.Close();

                var claims = new[] {

                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, student.IndexNumber),
                new Claim(ClaimTypes.Role, "student"),
            };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken
                (
                    issuer: "Gakko",
                    audience: "Students",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(10),
                    signingCredentials: creds
                );

                response.accessToken = new JwtSecurityTokenHandler().WriteToken(token);
                response.refreshToken = Guid.NewGuid();

                con.Open();

                com.CommandText = "UPDATE Student SET refToken=@refToken WHERE IndexNumber=@login";
                com.Parameters.AddWithValue("login", login);
                com.Parameters.AddWithValue("refToken", response.refreshToken);
                com.ExecuteNonQuery();

                con.Close();
            }

            return response;
        }

        public LoginResponse RefreshToken(string refreshToken)
        {
            LoginResponse response = new LoginResponse();
            Student student = new Student();

            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "SELECT IndexNumber FROM Student WHERE refToken=@refToken";
                com.Parameters.AddWithValue("refToken", refreshToken);

                con.Open();
                var tokenRead = com.ExecuteReader();
                if (!tokenRead.Read())
                {
                    return null;
                }

                student.IndexNumber = tokenRead["IndexNumber"].ToString();

                con.Close();

                var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, student.IndexNumber),
                new Claim(ClaimTypes.Role, "student"),
            };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken
                (
                    issuer: "Gakko",
                    audience: "Students",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(10),
                    signingCredentials: creds
                );

                response.accessToken = new JwtSecurityTokenHandler().WriteToken(token);
                response.refreshToken = Guid.NewGuid();

                con.Open();

                com.CommandText = "UPDATE Student SET refToken=@refreshedToken WHERE IndexNumber=@index";
                com.Parameters.AddWithValue("index", student.IndexNumber);
                com.Parameters.AddWithValue("refreshedToken", response.refreshToken);

                com.ExecuteNonQuery();

                con.Close();
            }

            return response;
        }
    }
}
