namespace diabot.Models
{
    public class PurchaseRequest
    {
        public string UserId { get; set; } = ""; 
        public string ServerId { get; set; } = "";
        public string PackageName { get; set; } = ""; 
        public string Diamond { get; set; } = ""; 
        public decimal SellingPrice { get; set; } 
        public string PaymentMethod { get; set; } = "";
    }
}
