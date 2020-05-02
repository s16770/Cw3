using Cw3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw3.DTOs.Responses
{
    public class PromoteStudentResponse
    {
        public Enrollment enrollment { get; set; }
        public string message { get; set; }
    }
}
