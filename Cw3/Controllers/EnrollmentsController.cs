using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Cw3.DTOs.Requests;
using Cw3.DTOs.Responses;
using Cw3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cw3.Controllers
{
    
    [ApiController]

    public class EnrollmentsController : ControllerBase
    {
        private IStudentDbService _service;

        public EnrollmentsController(IStudentDbService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("api/enrollments")]
       // [Authorize(Roles = "employee")]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            var response = _service.EnrollStudent(request);

            switch (response.message)
            {
                case "DONE":
                    return Created(response.message, response);
                default:
                    return BadRequest(response);
            }
        }

        [HttpPost]
        [Route("api/enrollments/promotions")]
        // [Authorize(Roles = "employee")]
        public IActionResult PromoteStudent(PromoteStudentRequest request)
        {   
            var response = _service.PromoteStudents(request);

            switch (response.message)
            {
                case "DONE":
                    return Created(response.message, response);
                default:
                    return NotFound(response);
            }
            
        }

    }
}