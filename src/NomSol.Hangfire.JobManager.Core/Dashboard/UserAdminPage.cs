using Hangfire.Dashboard;
using Hangfire.Dashboard.Pages;
using NomSol.Hangfire.JobManager.Core.Interfaces;
using NomSol.Hangfire.JobManager.Core.Models.Data.Tables;
using NomSol.Hangfire.JobManager.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace NomSol.Hangfire.JobManager.Core.Dashboard
{
    public class UserAdminPage : RazorPage
    {
        public const string Title = "User Admin";
        public const string PageRoute = "/jobmanager/users";

        private readonly IHangfireJobManagerRepository _repository;
        private readonly IJobManagerAuthorizationService _authService;
        private readonly NomSolJobManagerOptions _options;

        public UserAdminPage(IHangfireJobManagerRepository repository, IJobManagerAuthorizationService authService, NomSolJobManagerOptions options)
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

                if (!_authService.IsAuthorized(Context, "Admin"))
                {
                    WriteLiteral("<div class=\"row\"><div class=\"col-md-12\"><h1 class=\"page-header\">User Management</h1><div class=\"alert alert-danger\">Access Denied. You must be an administrator to view this page.</div></div></div>");
                    return;
                }
            }

            var updateUrl = Url.To(PageRoute + "/update");

            WriteLiteral("\r\n<div class=\"row\">\r\n");
            WriteLiteral("  <div class=\"col-md-12\">\r\n");
            WriteLiteral("    <h1 class=\"page-header\">User Management</h1>\r\n");
            
            WriteLiteral("    <style>\r\n");
            WriteLiteral("      .hf-input-theme { background-color: transparent !important; color: inherit !important; }\r\n");
            WriteLiteral("      .hf-input-theme option { background-color: inherit; color: inherit; }\r\n");
            WriteLiteral("      .user-admin-header { margin-bottom: 20px; padding: 15px; background: rgba(0,0,0,0.05); border-radius: 4px; }\r\n");
            WriteLiteral("    </style>\r\n");

            WriteLiteral("    <div id=\"jm-error-banner\"></div>\r\n");

            WriteLiteral("<script>\r\n");
            WriteLiteral("(function() {\r\n");
            WriteLiteral("  var p = new URLSearchParams(window.location.search);\r\n");
            WriteLiteral("  var e = p.get('error');\r\n");
            WriteLiteral("  if (e) {\r\n");
            WriteLiteral("    var banner = document.getElementById('jm-error-banner');\r\n");
            WriteLiteral("    var div = document.createElement('div');\r\n");
            WriteLiteral("    div.className = 'alert alert-danger';\r\n");
            WriteLiteral("    var strong = document.createElement('strong');\r\n");
            WriteLiteral("    strong.textContent = 'Error: ';\r\n");
            WriteLiteral("    div.appendChild(strong);\r\n");
            WriteLiteral("    div.appendChild(document.createTextNode(decodeURIComponent(e)));\r\n");
            WriteLiteral("    banner.appendChild(div);\r\n");
            WriteLiteral("  }\r\n");
            WriteLiteral("})();\r\n");

            WriteLiteral("function postUserAction(data) {\r\n");
            WriteLiteral("    var f = document.createElement('form');\r\n");
            WriteLiteral("    f.method = 'POST';\r\n");
            WriteLiteral($"    f.action = '{updateUrl}';\r\n");
            WriteLiteral("    f.style.display = 'none';\r\n");
            WriteLiteral("    for (var key in data) {\r\n");
            WriteLiteral("        var i = document.createElement('input');\r\n");
            WriteLiteral("        i.type = 'hidden'; i.name = key; i.value = data[key];\r\n");
            WriteLiteral("        f.appendChild(i);\r\n");
            WriteLiteral("    }\r\n");
            WriteLiteral("    document.body.appendChild(f);\r\n");
            WriteLiteral("    f.submit();\r\n");
            WriteLiteral("}\r\n");

            WriteLiteral("function addUser() {\r\n");
            WriteLiteral("    var u = document.getElementById('new-username').value.trim();\r\n");
            WriteLiteral("    var r = document.getElementById('new-role').value;\r\n");
            WriteLiteral("    var p = document.getElementById('new-password') ? document.getElementById('new-password').value : '';\r\n");
            WriteLiteral("    if (!u) { alert('Username is required'); return; }\r\n");
            WriteLiteral("    postUserAction({ Action: 'Add', Username: u, Role: r, Password: p });\r\n");
            WriteLiteral("}\r\n");

            WriteLiteral("function saveUser(id, btn) {\r\n");
            WriteLiteral("    var tr = btn.closest('tr');\r\n");
            WriteLiteral("    var u = tr.querySelector('.row-username').value.trim();\r\n");
            WriteLiteral("    var r = tr.querySelector('.row-role').value;\r\n");
            WriteLiteral("    var pInput = tr.querySelector('.row-password');\r\n");
            WriteLiteral("    var p = pInput ? pInput.value : '';\r\n");
            WriteLiteral("    if (!u) { alert('Username cannot be empty'); return; }\r\n");
            WriteLiteral("    postUserAction({ Action: 'Update', UserId: id, Username: u, Role: r, Password: p });\r\n");
            WriteLiteral("}\r\n");

            WriteLiteral("function deleteUser(id) {\r\n");
            WriteLiteral("    if (!confirm('Are you sure you want to delete this user?')) return;\r\n");
            WriteLiteral("    postUserAction({ Action: 'Delete', UserId: id });\r\n");
            WriteLiteral("}\r\n");
            WriteLiteral("</script>\r\n");

            WriteLiteral("<div class=\"user-admin-header\">\r\n");
            WriteLiteral("  <h4>Add New User</h4>\r\n");
            WriteLiteral("  <div class=\"form-inline\">\r\n");
            WriteLiteral("    <input type=\"text\" id=\"new-username\" class=\"form-control input-sm hf-input-theme\" placeholder=\"Username\" style=\"width: 200px; margin-right: 10px;\" />\r\n");
            if (_options.UseLocalAuth)
            {
                WriteLiteral("    <input type=\"password\" id=\"new-password\" class=\"form-control input-sm hf-input-theme\" placeholder=\"Password\" style=\"width: 150px; margin-right: 10px;\" />\r\n");
            }
            WriteLiteral("    <select id=\"new-role\" class=\"form-control input-sm hf-input-theme\" style=\"width: 120px; margin-right: 10px;\">\r\n");
            WriteLiteral("      <option value=\"Admin\">Admin</option>\r\n");
            WriteLiteral("      <option value=\"Editor\">Editor</option>\r\n");
            WriteLiteral("    </select>\r\n");
            WriteLiteral("    <button type=\"button\" class=\"btn btn-sm btn-success\" onclick=\"addUser()\">Add User</button>\r\n");
            WriteLiteral("  </div>\r\n");
            WriteLiteral("</div>\r\n");

            var users = _repository.GetAllUsers();

            if (!users.Any())
            {
                WriteLiteral("<div class=\"alert alert-warning\">No users configured. Ensure the default admin is seeded or add one above.</div>\r\n");
            }
            else
            {
                WriteLiteral("<div class=\"table-responsive\">\r\n");
                WriteLiteral("  <table class=\"table\">\r\n");
                WriteLiteral("    <thead>\r\n");
                WriteLiteral("      <tr>\r\n");
                WriteLiteral("        <th>Username</th>\r\n");
                if (_options.UseLocalAuth) { WriteLiteral("        <th>Password</th>\r\n"); }
                WriteLiteral("        <th>Role</th>\r\n");
                WriteLiteral("        <th>Created</th>\r\n");
                WriteLiteral("        <th>Actions</th>\r\n");
                WriteLiteral("      </tr>\r\n");
                WriteLiteral("    </thead>\r\n");
                WriteLiteral("    <tbody>\r\n");
                foreach (var user in users)
                {
                    WriteLiteral($"      <tr>\r\n");
                    WriteLiteral($"        <td><input type=\"text\" class=\"form-control input-sm hf-input-theme row-username\" value=\"{System.Net.WebUtility.HtmlEncode(user.Username)}\" /></td>\r\n");
                    if (_options.UseLocalAuth)
                    {
                        WriteLiteral($"        <td><input type=\"password\" class=\"form-control input-sm hf-input-theme row-password\" placeholder=\"(Unchanged)\" /></td>\r\n");
                    }
                    WriteLiteral($"        <td><select class=\"form-control input-sm hf-input-theme row-role\">\r\n");
                    WriteLiteral($"          <option value=\"Admin\" {(user.Role == "Admin" ? "selected" : "")}>Admin</option>\r\n");
                    WriteLiteral($"          <option value=\"Editor\" {(user.Role == "Editor" ? "selected" : "")}>Editor</option>\r\n");
                    WriteLiteral($"        </select></td>\r\n");
                    WriteLiteral($"        <td>{user.Created_Date:yyyy-MM-dd HH:mm}</td>\r\n");
                    WriteLiteral($"        <td>\r\n");
                    WriteLiteral($"          <button type=\"button\" class=\"btn btn-xs btn-primary\" onclick=\"saveUser({user.PK_User_ID}, this)\">Save</button>\r\n");
                    WriteLiteral($"          <button type=\"button\" class=\"btn btn-xs btn-danger\" onclick=\"deleteUser({user.PK_User_ID})\">Delete</button>\r\n");
                    WriteLiteral($"        </td>\r\n");
                    WriteLiteral($"      </tr>\r\n");
                }
                WriteLiteral("    </tbody>\r\n");
                WriteLiteral("  </table>\r\n");
                WriteLiteral("</div>\r\n");
            }

            WriteLiteral("  </div>\r\n");
            WriteLiteral("</div>\r\n");
        }
    }
}
