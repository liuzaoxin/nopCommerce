using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.AliF2FPay
{
    /// <summary>
    /// AliF2FPay 支付配置
    /// </summary>
    public class AliF2FPayPaymentSettings: ISettings
    {
        /// <summary>
        /// 支付宝公钥
        /// </summary>
        public string Alipay_public_key { get; set; }

        //这里要配置没有经过的原始私钥

        //开发者私钥
        public string Merchant_private_key { get; set; }

        //开发者公钥
        public string Merchant_public_key { get; set; }

        //应用ID
        public string AppId { get; set; }

        //合作伙伴ID：partnerID
        public string Pid { get; set; }

        //支付宝网关
        public static string serverUrl = "https://openapi.alipay.com/gateway.do";
        public static string mapiUrl = "https://mapi.alipay.com/gateway.do";
        public static string monitorUrl = "http://mcloudmonitor.com/gateway.do";

        //编码，无需修改
        public static string charset = "utf-8";
        //签名类型，支持RSA2（推荐！）、RSA
        public static string sign_type = "RSA";
        //public static string sign_type = "RSA";
        //版本号，无需修改
        public static string version = "1.0";
    }
}
