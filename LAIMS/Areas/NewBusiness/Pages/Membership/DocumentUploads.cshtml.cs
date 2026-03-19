using LAIMS.Interfaces.Documents;
using LAIMS.Models.Documents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LAIMS.Areas.NewBusiness.Pages.Membership
{
    public class DocumentUploadsModel : PageModel
    {
        private readonly IMediaUploadRepository _repository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DocumentUploadsModel(IMediaUploadRepository repository, IWebHostEnvironment webHostEnvironment)
        {
            _repository = repository;
            _webHostEnvironment = webHostEnvironment;
        }

        [BindProperty]
        public int MemberId { get; set; }

        [BindProperty]
        public string FilingNo { get; set; }

        [BindProperty]
        public string DocumentNo { get; set; }

        [BindProperty]
        public IFormFile Upload { get; set; }

        public void OnGet()
        {
            // Optional: Any initialization logic for the GET request
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Save the uploaded file to the wwwroot/uploads folder
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Upload.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        Upload.CopyTo(stream);
                    }

                    // Create a MediaUpload object and save it to the database
                    var mediaUpload = new MediaUpload
                    {
                        ID = Guid.NewGuid(),
                        MemberID = MemberId,
                        FilingNo = FilingNo,
                        DocumentNo = DocumentNo,
                        //MimeType = 1, // Assuming MimeType is an int (adjust accordingly)
                        //DocumentsID = 1, // Assuming DocumentsID is an int (adjust accordingly)
                        Data = System.IO.File.ReadAllBytes(filePath),
                        AddedOn = DateTime.Now,
                        AddedBy = "User" // Replace with the actual user or system identifier
                    };

                    _repository.SaveMediaUpload(mediaUpload);

                    return RedirectToPage("/Index"); // Redirect to a success page or homepage
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            // If model state is invalid, redisplay the form with validation errors
            return Page();
        }
    }
}
