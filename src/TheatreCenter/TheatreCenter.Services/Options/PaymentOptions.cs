namespace TheatreCenter.Services.Options
{
    public class PaymentOptions
    {
        /// <summary>
        /// mock | real
        /// </summary>
        public string Mode { get; set; } = "mock";

        public string DefaultCurrency { get; set; } = "RUB";

        public string MockBaseUrl { get; set; } = "http://localhost:8081";

        public CloudPaymentsOptions CloudPayments { get; set; } = new();
    }

    public class CloudPaymentsOptions
    {
        public string BaseUrl { get; set; } = "https://api.cloudpayments.ru";
        public string PublicId { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
    }
}
