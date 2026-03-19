// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using LAIMS.Interfaces.Utilities;
using LAIMS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace LAIMS.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IAuditRepository _auditRepository;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger
            , UserManager<ApplicationUser> userManager, IAuditRepository auditRepository)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _auditRepository = auditRepository;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            string userId = string.Empty;
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user != null)
            {
                userId = user.Id;
            }          
             await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if(!string.IsNullOrEmpty(userId))
            {
              //  var sessionId = HttpContext.Session.GetString("SessionID");
                var auditRecord = new LoginAudit
                {
                    UserID = userId,
                    ActionTime = DateTime.UtcNow,
                    Success = true,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                    FailureReason = string.Empty,
                    Status = "LoggedOut",
                    //SessionID=sessionId

                };
                _auditRepository.SaveLoginAudit(auditRecord);
            }

            //if (returnUrl != null)
            //{
            returnUrl = "~/Index";
             return LocalRedirect(returnUrl);
            //}
            //else
            //{
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                //return RedirectToPage();
            //}
        }
    }
}
