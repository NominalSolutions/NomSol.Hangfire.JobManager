using System.Net;

namespace NomSol.Hangfire.JobManager.Core.Models
{
    public class BaseResponse
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = "Success";
    }
}
