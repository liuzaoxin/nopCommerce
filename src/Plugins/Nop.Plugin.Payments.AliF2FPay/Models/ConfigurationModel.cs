using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.AliF2FPay.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.AliF2FPay.Alipay_public_key")]
        public string Alipay_public_key { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AliF2FPay.Merchant_private_key")]
        public string Merchant_private_key { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AliF2FPay.Merchant_public_key")]
        public string Merchant_public_key { get; set; }


        [NopResourceDisplayName("Plugins.Payments.AliF2FPay.AppId")]
        public string AppId { get; set; }


        [NopResourceDisplayName("Plugins.Payments.AliF2FPay.Pid")]
        public string Pid { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AliF2FPay.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
    }
}