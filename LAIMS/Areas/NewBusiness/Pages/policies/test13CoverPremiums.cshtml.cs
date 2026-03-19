using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LAIMS.Areas.NewBusiness.Pages.policies
{
    public class test13CoverPremiumsModel : PageModel
    {
        public class Product
        {
            public Guid ProductID { get; set; }
            public string ProductName { get; set; }
            public string ProductType { get; set; }
            public decimal Cover { get; set; }
            public decimal Premium { get; set; }
        }
        public class Customer
        {
            public Guid CustomerID { get; set; }
            public string CustomerName { get; set; }
        }
        [BindProperty]
        public List<Product> Products { get; set; }

        [BindProperty]
        public Guid CustomerID { get; set; }
        [BindProperty]
        public List<Customer> Customers { get; set; }

        public void OnGet()
        {
            // Prefill the list of customers
            Customers = new List<Customer>
            {
            new Customer { CustomerID = Guid.NewGuid(), CustomerName = "Customer 1" },
            new Customer { CustomerID = Guid.NewGuid(), CustomerName = "Customer 2" },
             };

            Products = new List<Product>
            {
            new Product { ProductID = Guid.NewGuid(), ProductName = "Product 1", ProductType = "Type A", Cover = 1000, Premium = 50 },
            new Product { ProductID = Guid.NewGuid(), ProductName = "Product 2", ProductType = "Type B", Cover = 2000, Premium = 100 },
             };
        }
        public IActionResult OnGetFetchPremium(Guid productId, decimal cover)
        {             
                
            var Premium = cover * 0.05m; // Example calculation
                return new JsonResult(Premium); 
        }
        public void OnPost()
        {
            // Handle form submission
            // You can process the updated Products list here
            foreach (var product in Products)
            {
                // Example processing: log the updated covers
                Console.WriteLine($"ProductID: {product.ProductID}, Cover: {product.Cover}");
            }
        }
    }
}
