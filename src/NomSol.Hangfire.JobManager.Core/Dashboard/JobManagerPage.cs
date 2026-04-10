using Hangfire.Dashboard;
using Hangfire.Dashboard.Pages;
using NomSol.Hangfire.JobManager.Core.Interfaces;
using NomSol.Hangfire.JobManager.Core.Models;
using NomSol.Hangfire.JobManager.Core.Models.Data.Tables;
using System.Collections.Generic;

namespace NomSol.Hangfire.JobManager.Core.Dashboard
{
    public class JobManagerPage : RazorPage
    {
        public const string Title = "Job Manager";
        public const string PageRoute = "/jobmanager";

        private readonly IHangfireJobManagerRepository _repository;
        private readonly IJobManagerAuthorizationService _authService;
        private readonly NomSolJobManagerOptions _options;

        public JobManagerPage(IHangfireJobManagerRepository repository, IJobManagerAuthorizationService authService, NomSolJobManagerOptions options)
        {
            _repository = repository;
            _authService = authService;
            _options = options;
        }

        public override void Execute()
        {
            Layout = new LayoutPage(Title);

            if (_options.EnableUserAdmin)
            {
                var current = _authService.GetCurrentUsername(Context);
                if (_options.UseLocalAuth && (current == null || current == "__SETUP_REQUIRED__"))
                {
                    var httpContext = Context.GetHttpContext();
                    var targetUrl = current == "__SETUP_REQUIRED__" 
                        ? Url.To(UserSetupPage.PageRoute) 
                        : Url.To(UserLoginPage.PageRoute);

                    httpContext.Response.StatusCode = 302;
                    httpContext.Response.Headers["Location"] = targetUrl;
                    return;
                }

                if (!_authService.IsAuthorized(Context, "Viewer"))
                {
                    WriteLiteral("<div class=\"row\"><div class=\"col-md-12\"><h1 class=\"page-header\">Job Manager</h1><div class=\"alert alert-danger\">Access Denied. You do not have permission to view this page.</div></div></div>");
                    return;
                }
            }

            var updateUrl = Url.To(PageRoute + "/update");

            WriteLiteral("<div class=\"row\">\r\n");
            WriteLiteral("  <div class=\"col-md-12\">\r\n");
            WriteLiteral("    <h1 class=\"page-header\">Job Manager</h1>\r\n");
            WriteLiteral("    <style>\r\n");
            WriteLiteral("      .hf-input-theme {\r\n");
            WriteLiteral("          background-color: transparent !important;\r\n");
            WriteLiteral("          color: inherit !important;\r\n");
            WriteLiteral("      }\r\n");
            WriteLiteral("      .hf-input-theme option {\r\n");
            WriteLiteral("          background-color: inherit;\r\n");
            WriteLiteral("          color: inherit;\r\n");
            WriteLiteral("      }\r\n");
            WriteLiteral("    </style>\r\n");
            WriteLiteral("    <div id=\"jm-error-banner\"></div>\r\n");

            var cronstrueUrl = Url.To("/jobmanager/js/cronstrue");
            WriteLiteral($"<script src=\"{cronstrueUrl}\"></script>\r\n");
            WriteLiteral("<script>\r\n");
            // Show error from query string — sanitize via textContent to prevent XSS
            WriteLiteral("(function() {\r\n");
            WriteLiteral("  var p = new URLSearchParams(window.location.search);\r\n");
            WriteLiteral("  var e = p.get('error');\r\n");
            WriteLiteral("  if (e) {\r\n");
            WriteLiteral("    var banner = document.getElementById('jm-error-banner');\r\n");
            WriteLiteral("    var div = document.createElement('div');\r\n");
            WriteLiteral("    div.className = 'alert alert-danger';\r\n");
            WriteLiteral("    var strong = document.createElement('strong');\r\n");
            WriteLiteral("    strong.textContent = 'Validation Error: ';\r\n");
            WriteLiteral("    div.appendChild(strong);\r\n");
            WriteLiteral("    div.appendChild(document.createTextNode(decodeURIComponent(e)));\r\n");
            WriteLiteral("    banner.appendChild(div);\r\n");
            WriteLiteral("  }\r\n");
            WriteLiteral("})();\r\n");
            // Live cron description update
            WriteLiteral("function updateCronDescription(input) {\r\n");
            WriteLiteral("    var s = input.nextElementSibling;\r\n");
            WriteLiteral("    if (!s || s.tagName !== 'SMALL') return;\r\n");
            WriteLiteral("    try { s.innerText = cronstrue.toString(input.value, { throwExceptionOnParseError: true }); }\r\n");
            WriteLiteral("    catch(ex) { s.innerText = 'Invalid Expression'; }\r\n");
            WriteLiteral("}\r\n");
            // Submit handler: validate, then build+submit a real form on document.body
            WriteLiteral($"function saveJob(btn) {{\r\n");
            WriteLiteral("    var tr = btn.closest('tr');\r\n");
            WriteLiteral("    var cronInput = tr.querySelector('[data-field=\"cron\"]');\r\n");
            WriteLiteral("    var argsInput = tr.querySelector('[data-field=\"args\"]');\r\n");
            WriteLiteral("    var statusSelect = tr.querySelector('[data-field=\"status\"]');\r\n");
            WriteLiteral("    var jobId = tr.getAttribute('data-job-id');\r\n");
            WriteLiteral("    var cronVal = cronInput.value.trim();\r\n");
            // Validate
            WriteLiteral("    if (!cronVal) { alert('Cron expression cannot be empty.'); return; }\r\n");
            WriteLiteral("    try { cronstrue.toString(cronVal, { throwExceptionOnParseError: true }); }\r\n");
            WriteLiteral("    catch(ex) { alert('CRON expression is invalid. Please correct it before saving.'); return; }\r\n");
            // Build a real form on document.body and submit it
            WriteLiteral("    var f = document.createElement('form');\r\n");
            WriteLiteral("    f.method = 'POST';\r\n");
            WriteLiteral($"    f.action = '{updateUrl}';\r\n");
            WriteLiteral("    f.style.display = 'none';\r\n");
            WriteLiteral("    function addField(n, v) { var i = document.createElement('input'); i.type='hidden'; i.name=n; i.value=v; f.appendChild(i); }\r\n");
            WriteLiteral("    addField('JobId', jobId);\r\n");
            WriteLiteral("    addField('CronExpression', cronVal);\r\n");
            WriteLiteral("    addField('Arguments', argsInput.value);\r\n");
            WriteLiteral("    addField('Status', statusSelect.value);\r\n");
            WriteLiteral("    document.body.appendChild(f);\r\n");
            WriteLiteral("    f.submit();\r\n");
            WriteLiteral("}\r\n");
            WriteLiteral("</script>\r\n");

            List<JobManager.Core.Models.Data.Tables.JobManager> jobs = _repository.GetAllRecurringJobs();
            if (jobs.Count == 0)
            {
                WriteLiteral("    <div class=\"alert alert-info\">No recurring jobs found.</div>\r\n");
            }
            else
            {
                var isEditor = _authService.IsAuthorized(Context, "Editor");
                if (!isEditor && _authService.IsEnabled)
                {
                    WriteLiteral("    <div class=\"alert alert-warning\"><strong>Read-Only Access:</strong> You do not have permission to modify job configurations.</div>\r\n");
                }

                WriteLiteral("    <div class=\"table-responsive\">\r\n");
                WriteLiteral("      <table class=\"table\">\r\n");
                WriteLiteral("        <thead>\r\n");
                WriteLiteral("          <tr>\r\n");
                WriteLiteral("            <th>Job Name</th>\r\n");
                WriteLiteral("            <th>Cron Expression</th>\r\n");
                WriteLiteral("            <th>Arguments</th>\r\n");
                WriteLiteral("            <th>Status</th>\r\n");
                WriteLiteral("            <th>Actions</th>\r\n");
                WriteLiteral("          </tr>\r\n");
                WriteLiteral("        </thead>\r\n");
                WriteLiteral("        <tbody>\r\n");
                foreach (var job in jobs)
                {
                    WriteLiteral($"          <tr data-job-id=\"{job.PK_Job_ID}\">\r\n");
                    WriteLiteral($"              <td>{System.Net.WebUtility.HtmlEncode(job.JobName)}</td>\r\n");

                    string cronTranslation = string.Empty;
                    try { cronTranslation = CronExpressionDescriptor.ExpressionDescriptor.GetDescription(job.CronExpression); }
                    catch { cronTranslation = "Invalid Expression"; }

                    WriteLiteral($"              <td>\r\n");
                    WriteLiteral($"                  <input autocomplete=\"off\" type=\"text\" class=\"form-control input-sm hf-input-theme\" style=\"font-family: monospace; max-width: 150px;\" data-field=\"cron\" value=\"{System.Net.WebUtility.HtmlEncode(job.CronExpression)}\" oninput=\"updateCronDescription(this)\" {(isEditor ? "" : "readonly")} />\r\n");
                    WriteLiteral($"                  <small class=\"text-muted\">{System.Net.WebUtility.HtmlEncode(cronTranslation)}</small>\r\n");
                    WriteLiteral($"              </td>\r\n");
                    WriteLiteral($"              <td><input type=\"text\" class=\"form-control input-sm hf-input-theme\" style=\"min-width: 550px;\" data-field=\"args\" value=\"{System.Net.WebUtility.HtmlEncode(job.Arguments)}\" {(isEditor ? "" : "readonly")} /></td>\r\n");
                    WriteLiteral($"              <td><select class=\"form-control input-sm hf-input-theme\" style=\"width: auto;\" data-field=\"status\" {(isEditor ? "" : "disabled")}>\r\n");
                    WriteLiteral($"                <option value=\"ACTIVE\" {(job.Status == "ACTIVE" ? "selected" : "")}>Active</option>\r\n");
                    WriteLiteral($"                <option value=\"INACTIVE\" {(job.Status == "INACTIVE" ? "selected" : "")}>Inactive</option>\r\n");
                    WriteLiteral("              </select></td>\r\n");
                    WriteLiteral("              <td>\r\n");
                    if (isEditor)
                    {
                        WriteLiteral("                <button type=\"button\" class=\"btn btn-sm btn-primary\" onclick=\"saveJob(this)\">Save Changes</button>\r\n");
                    }
                    else
                    {
                        WriteLiteral("                <span class=\"label label-default\">Read-Only</span>\r\n");
                    }
                    WriteLiteral("              </td>\r\n");
                    WriteLiteral("          </tr>\r\n");
                }
                WriteLiteral("        </tbody>\r\n");
                WriteLiteral("      </table>\r\n");
                WriteLiteral("    </div>\r\n");
            }

            WriteLiteral("  </div>\r\n");
            WriteLiteral("</div>\r\n");
        }
    }
}
