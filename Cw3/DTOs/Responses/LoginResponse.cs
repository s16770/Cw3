using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw3.DTOs.Responses
{
    public class LoginResponse
    {
        public string accessToken { get; set; }
        public Guid refreshToken { get; set; }
    }
}
