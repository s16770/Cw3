using Cw3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Cw3.DAL
{
    public class MockDbService : IDbService
    {
        //private static IEnumerable<Student> _students;

        private const string  ConString = "Data Source=db-mssql;Initial Catalog=s16770;Integrated Security=True";

        /*static MockDbService()
        {
            _students = new List<Student>
            {
                new Student{IdStudent=1, FirstName="Jan", LastName="Kowalski"},
                new Student{IdStudent=2, FirstName="Anna", LastName="Malewski"},
                new Student{IdStudent=3, FirstName="Andrzej", LastName="Andrzejewicz"}
            };
        }*/

        public IEnumerable<Student> GetStudents()
        {
            var list = new List<Student>();
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "select * from Student";

                con.Open();
                SqlDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    var st = new Student();
                    st.IndexNumber = dr["IndexNumber"].ToString();
                    st.FirstName = dr["FirstName"].ToString();
                    st.LastName = dr["LastName"].ToString();
                    list.Add(st);
                }
            }

            return list;
            // return _students;
        }

        public IEnumerable<Enrollment> GetEnrollment(string id)
        {
            var list = new List<Enrollment>();
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "SELECT e.IdEnrollment,e.Semester,e.IdStudy,e.StartDate FROM Student AS s " +
                    "INNER JOIN Enrollment as e ON s.IdEnrollment = e.IdEnrollment WHERE s.IndexNumber = @id";

         
                com.Parameters.AddWithValue("id", id);

                con.Open();
                SqlDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    var enr = new Enrollment();
                    enr.IdEnrollment = Int32.Parse(dr["IdEnrollment"].ToString());
                    enr.Semester = Int32.Parse(dr["Semester"].ToString());
                    enr.IdStudy = Int32.Parse(dr["IdStudy"].ToString());
                    enr.StartDate = dr["StartDate"].ToString();
                    list.Add(enr);
                }
            }

            return list;
            // return _students;
        }
    }

    
}
