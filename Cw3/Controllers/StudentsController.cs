using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Cw3.DAL;
using Cw3.DTOs.Requests;
using Cw3.DTOs.Responses;
using Cw3.Models;
using Cw3.ModelsAuto;
using Cw3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cw3.Controllers
{
    [ApiController]
    [Route("api/students")]

    public class StudentsController : ControllerBase
    {
        private readonly IDbService _dbService;
        private readonly IStudentDbService _studentDbService;

        public IConfiguration Configuration { get; }

        public StudentsController(IDbService dbService, IStudentDbService studentDbService, IConfiguration configuration)
        {
            _dbService = dbService;
            _studentDbService = studentDbService;
            Configuration = configuration;
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetStudents(string orderBy)
        {
            //return Ok(_dbService.GetStudents());
            var db = new s16770Context();
            var res = db.Student.ToList();

            return Ok(res);
        }

        [HttpGet("{id}")]
        public IActionResult GetEnrollment(string id)
        {
            return Ok(_dbService.GetEnrollment(id));
        }

        /*[HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            student.IndexNumber = $"s{new Random().Next(1, 20000)}";
            return Ok(student);
        }*/

        [HttpPost]
        [Route("{id}")]
        public IActionResult UpdateStudent(ModelsAuto.Student student, string id)
        {
            var db = new s16770Context();
            var st = new ModelsAuto.Student
            {
                IndexNumber = id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                BirthDate = student.BirthDate,
                Passwd = student.Passwd
            };

            db.Attach(st);
            db.Entry(st).Property("FirstName").IsModified = true;
            db.Entry(st).Property("LastName").IsModified = true;
            db.Entry(st).Property("BirthDate").IsModified = true;
            db.Entry(st).Property("Passwd").IsModified = true;

            db.SaveChanges();

            return Ok("Student zaktualizowany");
        }

        [HttpDelete]
        [Route("{id}")]
        public IActionResult DeleteStudent(string id)
        {
            var db = new s16770Context();
            var st = db.Student.Where(st => st.IndexNumber == id).ToList().First();

            db.Student.Remove(st);
            db.SaveChanges();

            return Ok("Student usuniety");
        }

        [HttpPost]
        public IActionResult Login(LoginRequest request)
        {
            LoginResponse response = _studentDbService.Login(request.Login, request.Haslo);

            switch(response)
            {
                case null:
                    return Unauthorized();
                default:
                    return Ok(response);
            }
            
        }

        [HttpPost]
        [Route("refreshToken/{token}")]
        public IActionResult RefreshToken(string token)
        {
            LoginResponse response = _studentDbService.RefreshToken(token);

            switch (response)
            {
                case null:
                    return Unauthorized();
                default:
                    return Ok(response);
            }
        }

        [HttpPut("{id}")]
        public IActionResult PutStudent(int id)
        {
            return Ok("Aktualizacja ukonczona");
        }

       /*[HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {   
            return Ok("Usuwanie ukonczone");
        }*/
    }
}