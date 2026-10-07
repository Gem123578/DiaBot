
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


                // Weekly Pass
                var weeklyMatch = Regex.Match(
                    text,
                    @"Weekly\s+Pass\s*[-=:]\s*([\d,]+)\s*Ks",
                    RegexOptions.IgnoreCase
                );


                if (weeklyMatch.Success)
                {
                    decimal price = ParsePrice(
                        weeklyMatch.Groups[1].Value
                    );


                    specialPackages.Add(new
                    {
                        Name = "Weekly Pass",
                        Type = "weekly",
                        Price = price
                    });
                }


                // Monthly Epic Bundle
                var monthlyMatch = Regex.Match(
                    text,
                    @"Monthly\s+Epic\s+Bundle\s*[-=:]\s*([\d,]+)\s*Ks",
                    RegexOptions.IgnoreCase
                );


                if (monthlyMatch.Success)
                {
                    decimal price = ParsePrice(
                        monthlyMatch.Groups[1].Value
                    );


                    specialPackages.Add(new
                    {
                        Name = "Monthly Epic Bundle",
                        Type = "monthly",
                        Price = price
                    });
                }


                // Weekly Elite Bundle
                var eliteMatch = Regex.Match(
                    text,
                    @"Weekly\s+Elite\s+Bundle\s*[-=:]\s*([\d,]+)\s*Ks",
                    RegexOptions.IgnoreCase
                );


                if (eliteMatch.Success)
                {
                    decimal price = ParsePrice(
                        eliteMatch.Groups[1].Value
                    );


                    specialPackages.Add(new
                    {
                        Name = "Weekly Elite Bundle",
                        Type = "elite",
                        Price = price
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


                    decimal price = ParsePrice(
                        match.Groups[3].Value
                    );


                    rechargeEvent.Add(new
                    {
                        Name = $"{diamond} + {bonus}",

                        Diamond = diamond,

                        Bonus = bonus,

                        Total = diamond + bonus,

                        Price = price
                    });
                }



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


                    int cost = int.Parse(
                        match.Groups[2].Value.Replace(",", "")
                    );


                    decimal sellingPrice;



                    if (diamond <= 44)
                    {
                        sellingPrice = cost * 1.3333m;
                    }
                    else if (diamond <= 1000)
                    {
                        sellingPrice = cost * 1.68m;
                    }
                    else
                    {
                        sellingPrice = cost * 1.60m;
                    }


                    // Round UP to nearest 100
                    sellingPrice =
                        Math.Ceiling(sellingPrice / 100) * 100;


                    result.Add(new
                    {
                        Diamond = diamond,

                        CostPrice = cost,

                        SellingPrice = sellingPrice
                    });
                }



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


        private static decimal ParsePrice(string value)
        {
            return decimal.Parse(
                value.Replace(",", "").Trim()
            );
        }
    }
}