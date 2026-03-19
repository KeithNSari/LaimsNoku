using LAIMS.Interfaces.Claims;
using LAIMS.Interfaces.Premiums;
using LAIMS.Models.Claims;
using LAIMS.Models.Premiums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LAIMS.Areas.Investments.Pages
{
    public class BuyUnitsModel : PageModel
    {
        [BindProperty]
        public Guid PolicyID { get; set; }
        [BindProperty]
        public Guid PolicyTypeID { get; set; }
        [BindProperty]
        public Guid ProposerID { get; set; }
        [BindProperty]
        public Guid RequestID { get; set; }
        public class CartRow
        {
            public string UnitTrust { get; set; }
            public Guid UnitTrustID { get; set; }
            public decimal UnitCost { get; set; }
            public decimal Quantity { get; set; }
            public decimal TotalRowCost => UnitCost * Quantity;
        }
        public List<CartRow> Cart = new List<CartRow>();
        public void OnGet(Guid id, Guid policyTypeid, Guid policyid, Guid reqid)
        {
            ProposerID = id;
            PolicyTypeID = policyTypeid;
            PolicyID = policyid;
            RequestID = reqid;
            LoadCartRows();
        }
        private void LoadCartRows()
        {
            CartRow cartRow = new()
            {
                UnitTrust = "Unit Trust 1",
                UnitTrustID = Guid.NewGuid(),
                UnitCost = 1.2M
            };
            Cart.Add(cartRow);
            CartRow cartRow2 = new()
            {
                UnitTrust = "Unit Trust 2",
                UnitTrustID = Guid.NewGuid(),
                UnitCost = 1.2M
            };
            Cart.Add(cartRow2);
        }
        public IActionResult OnPostUpdateCart(List<CartRow> cartRows)
        {
            // Update quantities in the shopping cart
            for (var i = 0; i < cartRows.Count; i++)
            {
                var existingRow = Cart.FirstOrDefault(row => row.UnitTrustID == cartRows[i].UnitTrustID);

                if (existingRow != null)
                {

                    existingRow.Quantity = cartRows[i].Quantity;
                }
            }
            return RedirectToAction("Index");
        }
    }
}
