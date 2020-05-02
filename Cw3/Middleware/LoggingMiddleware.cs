using Cw3.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cw3.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();
            if (context.Request != null)
            {
                string path = context.Request.Path;
                string method = context.Request.Method;
                string queryString = context.Request.QueryString.ToString();
                string bodyString = "";

                using(var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyString = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }

                string logFile = "StudentsApiLog.txt";

                string logText = 
                    "New log: " + Environment.NewLine +
                    "Path: " + path + Environment.NewLine +
                    "Method: " + method + Environment.NewLine +
                    "Query: " + queryString + Environment.NewLine +
                    "Body: " + bodyString + Environment.NewLine +
                    Environment.NewLine;

                if (File.Exists(logFile))
                {
                    File.AppendAllText(logFile, logText);
                }else
                {
                    File.WriteAllText(logFile, logText);
                }
            }

            if (_next != null) await _next(context);
        }
    }
}
