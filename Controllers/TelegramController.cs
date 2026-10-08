using diabot.Models;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Text.RegularExpressions;
using Tesseract;

namespace diabot.Controllers
{
    public class TelegramController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public TelegramController(IConfiguration configuration , IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
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
        [HttpPost]
        public async Task<IActionResult> SubmitOrder(
            [FromForm] PurchaseRequest request,
            IFormFile? paymentScreenshot)
        {
            try
            {
                // ==========================================
                // 1. Validate Order
                // ==========================================

                if (request == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Order data မရရှိပါ။"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Player ID ထည့်ပေးပါ။"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.ServerId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Server ID ထည့်ပေးပါ။"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.PackageName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Package ရွေးပေးပါ။"
                    });
                }

                if (request.SellingPrice <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Price မမှန်ပါ။"
                    });
                }


                // ==========================================
                // 2. Validate Screenshot
                // ==========================================

                if (paymentScreenshot == null ||
                    paymentScreenshot.Length == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Payment Screenshot တင်ပေးပါ။"
                    });
                }

                const long maxFileSize = 5 * 1024 * 1024;

                if (paymentScreenshot.Length > maxFileSize)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Screenshot size သည် 5MB ထက်မကျော်ရပါ။"
                    });
                }

                var allowedTypes = new[]
                {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

                var contentType =
                    paymentScreenshot.ContentType
                        ?.ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(contentType) ||
                    !allowedTypes.Contains(contentType))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "JPG, PNG သို့မဟုတ် WEBP image သာတင်နိုင်ပါသည်။"
                    });
                }


                // ==========================================
                // 3. Telegram Configuration
                // ==========================================

                var token =
                    _configuration["Telegram:BotToken"];

                var chatIdValue =
                    _configuration["Telegram:ChatId"];

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

                if (!long.TryParse(
                        chatIdValue,
                        out long chatId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Telegram ChatId is invalid."
                    });
                }


                // ==========================================
                // 4. Telegram Bot
                // ==========================================

                var bot =
                    new TelegramBotClient(token);


                // ==========================================
                // 5. HTML Safe Values
                // ==========================================

                var safeUserId =
                    System.Net.WebUtility.HtmlEncode(
                        request.UserId);

                var safeServerId =
                    System.Net.WebUtility.HtmlEncode(
                        request.ServerId);

                var safePackage =
                    System.Net.WebUtility.HtmlEncode(
                        request.PackageName);

                var safeDiamond =
                    System.Net.WebUtility.HtmlEncode(
                        request.Diamond ?? "");

                var paymentMethod =
                    string.IsNullOrWhiteSpace(
                        request.PaymentMethod)
                        ? "QR Payment"
                        : request.PaymentMethod;

                var safePayment =
                    System.Net.WebUtility.HtmlEncode(
                        paymentMethod);


                // ==========================================
                // 6. Create Order Message
                // ==========================================

                var message = $"""
🛒 <b>NEW DIAMOND ORDER</b>

👤 <b>Player ID:</b>
<code>{safeUserId}</code>

🖥️ <b>Server ID:</b>
<code>{safeServerId}</code>

📦 <b>Package:</b>
{safePackage}

💎 <b>Diamond:</b>
{safeDiamond}

💰 <b>Price:</b>
<b>{request.SellingPrice:N0} Ks</b>

💳 <b>Payment:</b>
{safePayment}

⏰ <b>Time:</b>
{DateTime.Now:yyyy-MM-dd HH:mm:ss}

📸 <b>Payment Screenshot attached below.</b>
""";


                // ==========================================
                // 7. Send Order Text
                // ==========================================

                await bot.SendMessage(
                    chatId: chatId,
                    text: message,
                    parseMode: ParseMode.Html
                );


                // ==========================================
                // 8. Send Payment Screenshot
                // ==========================================

                await using var stream =
                    paymentScreenshot.OpenReadStream();

                var telegramFile =
                    InputFile.FromStream(
                        stream,
                        paymentScreenshot.FileName
                    );

                await bot.SendPhoto(
                    chatId: chatId,
                    photo: telegramFile,
                    caption:
                        $"📸 <b>Payment Proof</b>\n\n" +
                        $"👤 Player ID: <code>{safeUserId}</code>\n" +
                        $"🖥️ Server ID: <code>{safeServerId}</code>\n" +
                        $"💰 Amount: <b>{request.SellingPrice:N0} Ks</b>",
                    parseMode: ParseMode.Html
                );


                // ==========================================
                // 9. Success
                // ==========================================

                return Json(new
                {
                    success = true,
                    message =
                        "Order နှင့် Payment Screenshot ကို Seller ဆီသို့ ပို့ပြီးပါပြီ။"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"SubmitOrder Error: {ex}");

                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Order ပို့ရာတွင် အမှားရှိပါသည်။",
                    error = ex.Message,
                    innerError =
                        ex.InnerException?.Message
                });
            }
        }

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

       

[HttpPost]
    [Route("Telegram/ValidatePaymentScreenshot")]
    public async Task<IActionResult> ValidatePaymentScreenshot(
IFormFile paymentScreenshot,
[FromForm] string PaymentMethod)
    {
        try
        {
// ==========================================
// Check Payment Method
// ==========================================

    if (string.IsNullOrWhiteSpace(PaymentMethod))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Payment Method မရွေးထားပါ။"
                });
            }


            PaymentMethod =
                PaymentMethod.Trim();


            if (
                !PaymentMethod.Equals(
                    "KPay",
                    StringComparison.OrdinalIgnoreCase
                )
                &&
                !PaymentMethod.Equals(
                    "WavePay",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Payment Method မမှန်ပါ။"
                });
            }


            // ==========================================
            // Check Screenshot
            // ==========================================

            if (
                paymentScreenshot == null ||
                paymentScreenshot.Length == 0
            )
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Payment Screenshot မတွေ့ပါ။"
                });
            }


            // ==========================================
            // Check File Type
            // ==========================================

            var allowedTypes = new[]
            {
        "image/jpeg",
        "image/png",
        "image/jpg"
    };


            if (!allowedTypes.Contains(
                paymentScreenshot.ContentType
            ))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "JPG / PNG Image သာ တင်ပေးပါ။"
                });
            }


            // ==========================================
            // Check File Size
            // ==========================================

            if (paymentScreenshot.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Screenshot size 5MB ထက် မကျော်ရပါ။"
                });
            }


            // ==========================================
            // Copy Image To Memory
            // ==========================================

            using var memoryStream =
                new MemoryStream();

            await paymentScreenshot.CopyToAsync(
                memoryStream
            );

            memoryStream.Position = 0;


            // ==========================================
            // OCR
            // ==========================================

            string tessDataPath =
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "tessdata"
                );


            using var engine =
                new TesseractEngine(
                    tessDataPath,
                    "eng",
                    EngineMode.Default
                );


            using var pix =
                Pix.LoadFromMemory(
                    memoryStream.ToArray()
                );


            using var page =
                engine.Process(pix);


            string extractedText =
                page.GetText();


            extractedText =
                extractedText.ToLowerInvariant();


            // ==========================================
            // Detect Payment
            // ==========================================

            bool isValid = false;


            if (
                PaymentMethod.Equals(
                    "KPay",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                isValid =
                    extractedText.Contains("kpay")
                    ||
                    extractedText.Contains("kbzpay")
                    ||
                    extractedText.Contains("kbz pay")
                    ||
                    extractedText.Contains("kbz");
            }


            else if (
                PaymentMethod.Equals(
                    "WavePay",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                isValid =
                    extractedText.Contains("wavepay")
                    ||
                    extractedText.Contains("wave pay")
                    ||
                    extractedText.Contains("wave money")
                    ||
                    extractedText.Contains("wavemoney");
            }


            // ==========================================
            // Wrong Screenshot
            // ==========================================

            if (!isValid)
            {
                return Ok(new
                {
                    success = false,

                    message =
                        PaymentMethod +
                        " Payment Screenshot မဟုတ်ပါ။ " +
                        "မှန်ကန်သော Screenshot ကို ပြန်ရွေးပေးပါ။",

                    detectedText = extractedText
                });
            }


            // ==========================================
            // Valid Screenshot
            // ==========================================

            return Ok(new
            {
                success = true,

                message =
                    PaymentMethod +
                    " Payment Screenshot မှန်ကန်ပါသည်။",

                paymentMethod = PaymentMethod,

                detectedText = extractedText
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                "ValidatePaymentScreenshot Error: "
                + ex
            );


            return StatusCode(
                500,
                new
                {
                    success = false,

                    message =
                        "Payment Screenshot စစ်ဆေးရာတွင် " +
                        "အမှားဖြစ်နေပါသည်။"
                }
            );
        }


}

}
}