using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace LAIMS.Pages
{ 
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public string ErrorMessage { get; set; }
        public string ReturnUrl { get; set; }

        public void OnGet(string errorMessage, string returnUrl)
        {
            ErrorMessage = errorMessage;
            ReturnUrl = returnUrl;
        }
    }
}