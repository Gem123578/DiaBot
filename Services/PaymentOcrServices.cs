using Tesseract;

namespace diabot.Services
{
    public class PaymentOcrServices
    {
        public class PaymentOcrResult
        {
            public bool Success { get; set; }
            public bool WalletDetected { get; set; }
            public string DetectedWallet { get; set; }
            public string ExtractedText { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class PaymentOcrService
        {
            private readonly string _tessDataPath;

            public PaymentOcrService(string tessDataPath)
            {
                _tessDataPath = tessDataPath;
            }

            public Task<PaymentOcrResult> ReadScreenshotAsync(
                byte[] imageBytes,
                string paymentMethod)
            {
                return Task.Run(() =>
                {
                    var result = new PaymentOcrResult();

                    try
                    {
                        if (imageBytes == null || imageBytes.Length == 0)
                        {
                            result.ErrorMessage = "Screenshot မရှိပါ။";
                            return result;
                        }

                        string method = (paymentMethod ?? "")
                            .Trim()
                            .ToLowerInvariant();

                        if (method != "kpay" && method != "wavepay")
                        {
                            result.ErrorMessage =
                                "Payment Method မမှန်ကန်ပါ။";
                            return result;
                        }


                        using (var engine = new TesseractEngine(
                            _tessDataPath,
                            "eng+mya",
                            EngineMode.Default))
                        using (var pix = Pix.LoadFromMemory(imageBytes))
                        using (var page = engine.Process(pix))
                        {
                            string text = page.GetText() ?? "";
                            string normalized = (text ?? "").ToLowerInvariant();

                            // OCR က space နဲ့ newline တွေ မှားဖတ်တာကို လျှော့ချရန်
                            string compact = System.Text.RegularExpressions.Regex
                                .Replace(normalized, @"[^a-z0-9]", "");

                            result.Success = true;
                            result.ExtractedText = text;
                            Console.WriteLine("OCR TEXT:");
                            Console.WriteLine(result.ExtractedText);

                            Console.WriteLine("Payment Method: " + method);
                            Console.WriteLine("Wallet Detected: " + result.WalletDetected);

                            if (method == "kpay")
                            {
                                result.WalletDetected =
                                    compact.Contains("kpay") ||
                                    compact.Contains("kbzpay") ||
                                    compact.Contains("kbz");

                                result.DetectedWallet =
                                    result.WalletDetected ? "KPay" : null;
                            }
                            else if (method == "wavepay")
                            {
                                result.WalletDetected =
                                    compact.Contains("May Thin Gyan") ||
                                    compact.Contains("9445293952") ||
                                    compact.Contains("wave");

                                result.DetectedWallet =
                                    result.WalletDetected ? "WavePay" : null;
                            }
                            else
                            {
                                result.Success = false;
                                result.WalletDetected = false;
                                result.DetectedWallet = null;
                                result.ErrorMessage = "Payment Method မမှန်ကန်ပါ။";
                            }
                        }
                    
                    }
                    catch (Exception)
                    {
                        // အသေးစိတ် exception ကို server log မှာသာ သိမ်းပါ။
                        result.Success = false;
                        result.ErrorMessage =
                            "Screenshot ကို OCR ဖတ်၍မရပါ။";
                    }

                    return result;
                });
            }
        }
    }
}
