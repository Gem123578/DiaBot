
namespace diabot.Models
{
    public class SpecialPackage
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public decimal Price { get; set; }
    }


    public class RechargePackage
    {
        public string Name { get; set; } = "";

        public int Diamond { get; set; }

        public int Bonus { get; set; }

        public int Total { get; set; }

        public decimal Price { get; set; }
    }


    public class DiamondPackage
    {
        public int Diamond { get; set; }

        public int CostPrice { get; set; }

        public decimal SellingPrice { get; set; }
    }


    public class TelegramPriceResponse
    {
        public bool Success { get; set; }

        public string TelegramText { get; set; } = "";

        public List<SpecialPackage> SpecialPackages { get; set; }
            = new();

        public List<RechargePackage> RechargeEvent { get; set; }
            = new();

        public List<DiamondPackage> Prices { get; set; }
            = new();

        public int Count { get; set; }
    }
}