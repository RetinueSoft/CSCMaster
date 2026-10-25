using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CSCMasterAPI.Utils
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                File.AppendAllText("Exceptiolog.txt", "\n" + "Date:" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + ":" + ex.Message);
                if (ex.InnerException != null)
                    File.AppendAllText("Exceptiolog.txt", "Inner:" + ex.InnerException);
                File.AppendAllText("Exceptiolog.txt", "\n" + ex.StackTrace);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                var errorResponse = new ActionResponse();
                errorResponse.SucessValue = false;
                errorResponse.ErrorMessage.Add(new ErrorItem() { Message = ex.Message });
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                };
                var json = System.Text.Json.JsonSerializer.Serialize(errorResponse, options);
                await context.Response.WriteAsync(json);
            }
        }
    }
}