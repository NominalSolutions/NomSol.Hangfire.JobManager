using Hangfire.Dashboard;
using Hangfire.Dashboard.Pages;
using System;

namespace NomSol.Hangfire.JobManager.Core.Dashboard
{
    public class UserSetupPage : RazorPage
    {
        public const string Title = "Job Manager Setup";
        public const string PageRoute = "/jobmanager/setup";

        public override void Execute()
        {
            Layout = new LayoutPage(Title);

            var setupUrl = Url.To(PageRoute + "/submit");
            var error = Query("error");

            WriteLiteral("\r\n<div class=\"row\">\r\n");
            WriteLiteral("  <div class=\"col-md-6 col-md-offset-3\">\r\n");
            WriteLiteral("    <h1 class=\"page-header\">Initial Setup Required</h1>\r\n");
            WriteLiteral("    <p class=\"lead text-muted\">No users were found in the Job Manager database. Please create the initial Administrator account to proceed.</p>\r\n");
            
            if (!string.IsNullOrEmpty(error))
            {
                WriteLiteral($"    <div class=\"alert alert-danger\">{System.Net.WebUtility.HtmlEncode(error)}</div>\r\n");
            }

            WriteLiteral($"    <form method=\"POST\" action=\"{setupUrl}\" class=\"well\">\r\n");
            WriteLiteral("      <div class=\"form-group\">\r\n");
            WriteLiteral("        <label>Admin Username</label>\r\n");
            WriteLiteral("        <input type=\"text\" name=\"UserName\" class=\"form-control\" placeholder=\"e.g. admin\" required />\r\n");
            WriteLiteral("      </div>\r\n");
            WriteLiteral("      <div class=\"form-group\">\r\n");
            WriteLiteral("        <label>Admin Password</label>\r\n");
            WriteLiteral("        <input type=\"password\" name=\"Password\" class=\"form-control\" required />\r\n");
            WriteLiteral("      </div>\r\n");
            WriteLiteral("      <div class=\"form-group\">\r\n");
            WriteLiteral("        <label>Confirm Password</label>\r\n");
            WriteLiteral("        <input type=\"password\" name=\"ConfirmPassword\" class=\"form-control\" required />\r\n");
            WriteLiteral("      </div>\r\n");
            WriteLiteral("      <button type=\"submit\" class=\"btn btn-success btn-lg btn-block\">Complete Setup & Create Admin</button>\r\n");
            WriteLiteral("    </form>\r\n");
            WriteLiteral("  </div>\r\n");
            WriteLiteral("</div>\r\n");
        }
    }
}
