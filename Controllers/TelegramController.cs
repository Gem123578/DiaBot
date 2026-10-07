using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Telegram.Bot;

namespace diabot.Controllers
{
    public class TelegramController : Controller
    {
        private readonly IConfiguration _configuration;

        public TelegramController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public IActionResult DiamondSelling()
        {
            return View();
        }



        [HttpGet]
        public async Task<IActionResult> GetPinnedMessage()
        {
            try
            {

                var token = _configuration["Telegram:BotToken"];

                var chatIdValue = _configuration["Telegram:ChatId"];


                if (string.IsNullOrWhiteSpace(token))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Telegram BotToken is missing."
                    });
                }


                if (string.IsNullOrWhiteSpace(chatIdValue))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Telegram ChatId is missing."
                    });
                }


                if (!long.TryParse(chatIdValue, out long chatId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Telegram ChatId is invalid."
                    });
                }



                var bot = new TelegramBotClient(token);

                var chat = await bot.GetChat(chatId);

                var pinned = chat.PinnedMessage;


                if (pinned == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "No pinned message found."
                    });
                }


                var text = pinned.Text ?? "";


                if (string.IsNullOrWhiteSpace(text))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Pinned message has no text."
                    });
                }


                var specialPackages = new List<object>();



                var weeklyMatch = Regex.Match(
                    text,
                    @"Weekly\s+Pass\s*[-=:]\s*([\d,]+)\s*Ks",
                    RegexOptions.IgnoreCase
                );


                if (weeklyMatch.Success)
                {
                    decimal cost = ParsePrice(
                        weeklyMatch.Groups[1].Value
                    );


                    decimal sellingPrice =
                        GetPackageSellingPrice(
                            "weekly",
                            cost
                        );


                    specialPackages.Add(new
                    {
                        Name = "Weekly Pass",

                        Type = "weekly",

                        CostPrice = cost,

                        SellingPrice = sellingPrice
                    });
                }


                var monthlyMatch = Regex.Match(
                    text,
                    @"Monthly\s+Epic\s+Bundle\s*[-=:]\s*([\d,]+)\s*Ks",
                    RegexOptions.IgnoreCase
                );


                if (monthlyMatch.Success)
                {
                    decimal cost = ParsePrice(
                        monthlyMatch.Groups[1].Value
                    );


                    decimal sellingPrice =
                        GetPackageSellingPrice(
                            "monthly",
                            cost
                        );


                    specialPackages.Add(new
                    {
                        Name = "Monthly Epic Bundle",

                        Type = "monthly",

                        CostPrice = cost,

                        SellingPrice = sellingPrice
                    });
                }



                var eliteMatch = Regex.Match(
                    text,
                    @"Weekly\s+Elite\s+Bundle\s*[-=:]\s*([\d,]+)\s*Ks",
                    RegexOptions.IgnoreCase
                );


                if (eliteMatch.Success)
                {
                    decimal cost = ParsePrice(
                        eliteMatch.Groups[1].Value
                    );


                    decimal sellingPrice =
                        GetPackageSellingPrice(
                            "elite",
                            cost
                        );


                    specialPackages.Add(new
                    {
                        Name = "Weekly Elite Bundle",

                        Type = "elite",

                        CostPrice = cost,

                        SellingPrice = sellingPrice
                    });
                }



                var rechargeEvent = new List<object>();


                var rechargeMatches = Regex.Matches(
                    text,
                    @"Dia\s+([\d,]+)\s*\+\s*([\d,]+)\s*=\s*([\d,]+)\s*Ks",
                    RegexOptions.IgnoreCase
                );


                foreach (Match match in rechargeMatches)
                {
                    int diamond = int.Parse(
                        match.Groups[1].Value.Replace(",", "")
                    );


                    int bonus = int.Parse(
                        match.Groups[2].Value.Replace(",", "")
                    );


                    decimal cost = ParsePrice(
                        match.Groups[3].Value
                    );


                    decimal sellingPrice =
                        GetRechargeSellingPrice(
                            diamond,
                            cost
                        );


                    rechargeEvent.Add(new
                    {
                        Name = $"{diamond} + {bonus}",

                        Diamond = diamond,

                        Bonus = bonus,

                        Total = diamond + bonus,

                        CostPrice = cost,

                        SellingPrice = sellingPrice
                    });
                }


                // =====================================================
                // NORMAL DIAMONDS
                // =====================================================

                var result = new List<object>();


                var diamondMatches = Regex.Matches(
                    text,
                    @"(?:💎\s*)?Dia\s+([\d,]+)\s*-\s*([\d,]+)\s*Ks",
                    RegexOptions.IgnoreCase
                );


                foreach (Match match in diamondMatches)
                {
                    int diamond = int.Parse(
                        match.Groups[1].Value.Replace(",", "")
                    );


                    decimal cost = ParsePrice(
                        match.Groups[2].Value
                    );


                    decimal sellingPrice =
                        GetDiamondSellingPrice(
                            diamond,
                            cost
                        );


                    result.Add(new
                    {
                        Diamond = diamond,

                        CostPrice = cost,

                        SellingPrice = sellingPrice
                    });
                }


                // =====================================================
                // JSON RESPONSE
                // =====================================================

                return Json(new
                {
                    success = true,

                    telegramText = text,

                    specialPackages = specialPackages,

                    rechargeEvent = rechargeEvent,

                    prices = result,

                    count = result.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,

                    message = "Unable to get Telegram price.",

                    error = ex.Message
                });
            }
        }


        // =====================================================
        // SPECIAL PACKAGE SELLING PRICE
        // =====================================================

        private static decimal GetPackageSellingPrice(
            string type,
            decimal cost)
        {
            decimal markup;


            switch (type.ToLower())
            {
                case "weekly":

                    // 6700 -> approximately 6900

                    markup = 0.02985m;

                    break;


                case "monthly":

                    // 19000 -> approximately 19500

                    markup = 0.02632m;

                    break;


                case "elite":

                    // 3600 -> approximately 3700

                    markup = 0.02778m;

                    break;


                default:

                    markup = 0.03m;

                    break;
            }


            decimal sellingPrice =
                cost * (1 + markup);


            return RoundUp100(sellingPrice);
        }


        // =====================================================
        // 2X RECHARGE SELLING PRICE
        // =====================================================

        private static decimal GetRechargeSellingPrice(
            int diamond,
            decimal cost)
        {
            decimal markup;


            /*
             * Recharge ပမာဏနည်းရင်
             * အနည်းငယ်ပိုမြင့်တဲ့ margin
             *
             * Recharge ပမာဏများရင်
             * Competitive ဖြစ်အောင် margin လျှော့ထားခြင်း
             */

            if (diamond <= 50)
            {
                markup = 0.027m;
            }
            else if (diamond <= 150)
            {
                markup = 0.028m;
            }
            else if (diamond <= 250)
            {
                markup = 0.028m;
            }
            else
            {
                markup = 0.029m;
            }


            decimal sellingPrice =
                cost * (1 + markup);


            return RoundUp100(sellingPrice);
        }


        // =====================================================
        // NORMAL DIAMOND SELLING PRICE
        // =====================================================

        private static decimal GetDiamondSellingPrice(
            int diamond,
            decimal cost)
        {
            decimal markup;


            /*
             * Small Diamond
             */

            if (diamond <= 100)
            {
                markup = 0.036m;
            }


            /*
             * Medium Diamond
             */

            else if (diamond <= 600)
            {
                markup = 0.027m;
            }


            /*
             * Large Diamond
             */

            else if (diamond <= 1000)
            {
                markup = 0.030m;
            }


            /*
             * Very Large Diamond
             */

            else if (diamond <= 2500)
            {
                markup = 0.030m;
            }


            /*
             * Huge Diamond
             */

            else
            {
                markup = 0.032m;
            }


            decimal sellingPrice =
                cost * (1 + markup);


            return RoundUp100(sellingPrice);
        }


        // =====================================================
        // ROUND UP TO NEAREST 100 KS
        // =====================================================

        private static decimal RoundUp100(decimal price)
        {
            return Math.Ceiling(price / 100m) * 100m;
        }


        // =====================================================
        // PARSE TELEGRAM PRICE
        // =====================================================

        private static decimal ParsePrice(string value)
        {
            return decimal.Parse(
                value
                    .Replace(",", "")
                    .Trim()
            );
        }
    }
}