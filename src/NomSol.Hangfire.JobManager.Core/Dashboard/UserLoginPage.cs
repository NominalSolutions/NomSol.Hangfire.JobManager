using Hangfire.Dashboard;
using Hangfire.Dashboard.Pages;

namespace NomSol.Hangfire.JobManager.Core.Dashboard
{
    public class UserLoginPage : RazorPage
    {
        public const string Title = "Job Manager Login";
        public const string PageRoute = "/jobmanager/login";

        public override void Execute()
        {
            Layout = new LayoutPage(Title);

            var loginUrl = Url.To(PageRoute + "/submit");
            var error = Query("error");

            WriteLiteral("\r\n<div class=\"row\">\r\n");
            WriteLiteral("  <div class=\"col-md-4 col-md-offset-4\">\r\n");
            WriteLiteral("    <h1 class=\"page-header\">Job Manager Login</h1>\r\n");
            
            if (!string.IsNullOrEmpty(error))
            {
                WriteLiteral($"    <div class=\"alert alert-danger\">{System.Net.WebUtility.HtmlEncode(error)}</div>\r\n");
            }

            WriteLiteral($"    <form method=\"POST\" action=\"{loginUrl}\">\r\n");
            WriteLiteral("      <div class=\"form-group\">\r\n");
            WriteLiteral("        <label>Username</label>\r\n");
            WriteLiteral("        <input type=\"text\" name=\"UserName\" class=\"form-control\" required />\r\n");
            WriteLiteral("      </div>\r\n");
            WriteLiteral("      <div class=\"form-group\">\r\n");
            WriteLiteral("        <label>Password</label>\r\n");
            WriteLiteral("        <input type=\"password\" name=\"Password\" class=\"form-control\" required />\r\n");
            WriteLiteral("      </div>\r\n");
            WriteLiteral("      <button type=\"submit\" class=\"btn btn-primary btn-block\">Login</button>\r\n");
            WriteLiteral("    </form>\r\n");
            WriteLiteral("  </div>\r\n");
            WriteLiteral("</div>\r\n");
        }
    }
}
