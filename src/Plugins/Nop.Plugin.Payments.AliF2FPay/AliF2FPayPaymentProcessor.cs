using Com.Alipay;
using Com.Alipay.Business;
using Com.Alipay.Domain;
using Com.Alipay.Model;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.AliF2FPay.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using ThoughtWorks.QRCode.Codec;
using System.Web.UI;

namespace Nop.Plugin.Payments.AliF2FPay
{
    /// <summary>
    /// AliF2FPay payment processor
    /// </summary>
    public class AliF2FPayPaymentProcessor : BasePlugin, IPaymentMethod
    {
        //private LogHelper log = new LogHelper(AppDomain.CurrentDomain.BaseDirectory + "/log/log.txt");

        private IAlipayTradeService serviceClient;
        #region Constants

        //支付宝网关
        private const string ServerUrl = "https://openapi.alipay.com/gateway.do";
        private const string MapiUrl = "https://mapi.alipay.com/gateway.do";
        private const string MonitorUrl = "http://mcloudmonitor.com/gateway.do";

        //编码，无需修改
        private const string Charset = "utf-8";
        //签名类型，支持RSA2
        private const string Sign_type = "RSA";

        //版本号，无需修改
        private const string Version = "1.0";

        #endregion

        #region Fields

        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private readonly AliF2FPayPaymentSettings _aliF2FPayPaymentSettings;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public AliF2FPayPaymentProcessor(
            ISettingService settingService,
            IWebHelper webHelper,
            IStoreContext storeContext,
            AliF2FPayPaymentSettings aliF2FPayPaymentSettings,
            ILocalizationService localizationService)
        {
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._storeContext = storeContext;
            this._aliF2FPayPaymentSettings = aliF2FPayPaymentSettings;
            this._localizationService = localizationService;
            this.serviceClient = F2FBiz.CreateClientInstance(ServerUrl, _aliF2FPayPaymentSettings.AppId, _aliF2FPayPaymentSettings.Merchant_private_key, Version,
                             Sign_type, _aliF2FPayPaymentSettings.Alipay_public_key, Charset);
        }

        #endregion

        #region methods

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Pending };

            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            Alipay_RSA_Submit(postProcessPaymentRequest.Order);
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _aliF2FPayPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();

            result.AddError("Capture method not supported");

            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();

            result.AddError("Refund method not supported");

            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();

            result.AddError("Void method not supported");

            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            result.AddError("Recurring payment not supported");

            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();

            result.AddError("Recurring payment not supported");

            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //AliPay is the redirection payment method
            //It also validates whether order is also paid (after redirection) so customers will not be able to pay twice

            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;

            //let's ensure that at least 1 minute passed after order is placed
            return !((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1);
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentAliF2FPay";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.AliF2FPay.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentAliF2FPay";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.AliF2FPay.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentAliF2FPayController);
        }

        protected void Alipay_RSA_Submit(Order order)
        {
            AlipayTradePrecreateContentBuilder builder = BuildPrecreateContent(order.Id.ToString(), this._aliF2FPayPaymentSettings.Pid, order.OrderTotal, order.Id.ToString());
            string out_trade_no = builder.out_trade_no;

            //如果需要接收扫码支付异步通知，那么请把下面两行注释代替本行。
            //推荐使用轮询撤销机制，不推荐使用异步通知,避免单边账问题发生。
            AlipayF2FPrecreateResult precreateResult = serviceClient.tradePrecreate(builder);
            //string notify_url = "http://10.5.21.14/notify_url.aspx";  //商户接收异步通知的地址
            //AlipayF2FPrecreateResult precreateResult = serviceClient.tradePrecreate(builder, notify_url);

            //以下返回结果的处理供参考。
            //payResponse.QrCode即二维码对于的链接
            //将链接用二维码工具生成二维码打印出来，顾客可以用支付宝钱包扫码支付。
            string result = "";

            switch (precreateResult.Status)
            {
                case ResultEnum.SUCCESS:
                    DoWaitProcess(precreateResult);
                    break;
                case ResultEnum.FAILED:
                    result = precreateResult.response.Body;
                    break;

                case ResultEnum.UNKNOWN:
                    if (precreateResult.response == null)
                    {
                        result = "配置或网络异常，请检查后重试";
                    }
                    else
                    {
                        result = "系统异常，请更新外部订单后重新发起请求";
                    }
                    // Response.Redirect("result.aspx?Text=" + result);
                    break;
            }

        }

        /// <summary>
        /// 构造支付请求数据
        /// </summary>
        /// <returns>请求数据集</returns>
        private AlipayTradePrecreateContentBuilder BuildPrecreateContent(string orderid, string pid, decimal totalfee, string subject)
        {
            //线上联调时，请输入真实的外部订单号。
            string out_trade_no = "";
            if (String.IsNullOrEmpty(orderid.Trim()))
            {
                out_trade_no = System.DateTime.Now.ToString("yyyyMMddHHmmss") + "0000" + (new Random()).Next(1, 10000).ToString();
            }
            else
            {
                out_trade_no = orderid.Trim();
            }

            AlipayTradePrecreateContentBuilder builder = new AlipayTradePrecreateContentBuilder();
            //收款账号
            builder.seller_id = pid;
            //订单编号
            builder.out_trade_no = out_trade_no;
            //订单总金额
            builder.total_amount = Math.Round(totalfee, 2).ToString().Trim();
            //参与优惠计算的金额
            //builder.discountable_amount = "";
            //不参与优惠计算的金额
            //builder.undiscountable_amount = "";
            //订单名称
            builder.subject = subject.Trim();
            //自定义超时时间
            builder.timeout_express = "5m";
            //订单描述
            builder.body = "";
            //门店编号，很重要的参数，可以用作之后的营销
            builder.store_id = "test store id";
            //操作员编号，很重要的参数，可以用作之后的营销
            builder.operator_id = "test";

            //传入商品信息详情
            List<GoodsInfo> gList = new List<GoodsInfo>();
            GoodsInfo goods = new GoodsInfo();
            goods.goods_id = "goods id";
            goods.goods_name = "goods name";
            goods.price = "0.01";
            goods.quantity = "1";
            gList.Add(goods);
            builder.goods_detail = gList;

            //系统商接入可以填此参数用作返佣
            //ExtendParams exParam = new ExtendParams();
            //exParam.sysServiceProviderId = "20880000000000";
            //builder.extendParams = exParam;

            return builder;

        }

        /// <summary>
        /// 生成二维码并展示到页面上
        /// </summary>
        /// <param name="precreateResult">二维码串</param>
        private void DoWaitProcess(AlipayF2FPrecreateResult precreateResult)
        {
            //打印出 preResponse.QrCode 对应的条码
            Bitmap bt;
            string enCodeString = precreateResult.response.QrCode;
            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.H;
            qrCodeEncoder.QRCodeScale = 3;
            qrCodeEncoder.QRCodeVersion = 8;
            bt = qrCodeEncoder.Encode(enCodeString, Encoding.UTF8);
            string filename = System.DateTime.Now.ToString("yyyyMMddHHmmss") + "0000" + (new Random()).Next(1, 10000).ToString()
             + ".jpg";
            //bt.Save(Server.MapPath("~/images/") + filename);

            //轮询订单结果
            //根据业务需要，选择是否新起线程进行轮询
            //ParameterizedThreadStart ParStart = new ParameterizedThreadStart(LoopQuery);
            //Thread myThread = new Thread(ParStart);
            //object o = precreateResult.response.OutTradeNo;
            //myThread.Start(o);

        }

        public override void Install()
        {
            //settings
            var settings = new AliF2FPayPaymentSettings
            {
                AdditionalFee = 0,
                Alipay_public_key = @"MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDDI6d306Q8fIfCOaTXyiUeJHkrIvYISRcc73s3vF1ZT7XN8RNPwJxo8pWaJMmvyTn9N4HQ632qJBVHf8sxHi/fEsraprwCtzvzQETrNRwVxLO5jVmRGi60j8Ue1efIlzPXV9je9mkjzOmdssymZkh2QhUrCmZYI/FCEa3/cNMW0QIDAQAB",
                AppId = "2017042406941306",
                Merchant_private_key = @"MIICXQIBAAKBgQDB4GRcG4clcfX/X8et1gR0uNzkSMuD7aj++PbbstMNs97C0LEu
O7US+/rdGHDPJJiiEf3mlbpRDF6jBu7aNJf7DOyWZg6OYLBaccopZ0rYfCkwoLAh
WU7coLbclToAhlHniq1ytqfkwpJl7r3Jz8SNMhf9U9c+tylONbIFHF3NOQIDAQAB
AoGADf7f39JQ6EAYzQ2iAYeQnMh3kbc7kdOHPpjEYUnAeJ3Cd/fOwpKm2K7+BhXs
lteCeTipRosKfy1Qa55lgbUIP4PkZ+BeiOXUnqVsITtvwr1zfB7sWIjoX9nDLrPd
o1kvunyDz05yj064E5B/y9Vx4+48ztr/BunY0uN3BRuRK00CQQD6wAmC7P50BGAD
oFoXJKWkzUoB39F6ieleZYzyY2qmPwZyqQ2lePaVE0YpdV8ZPw4pa9DAa3pL/OdU
Xy9R3GaDAkEAxe+Gg6gfTKMUBcuDQZf8H5XSr3RoC07I1BdjDzRZOeTnCGrIZr2G
z2454AxjuDa+f9bqu6+gqHEHd27Vb0JQkwJAexLc2EVIk1s+YSlIbsmO//e/FnJr
2BBu2eVQK/x98UFIAelWCFz58qu2KU0xsyuO4OfJW1ilezyTsobRrAVYzwJBAKrP
pZmAQGJ2aRUHJ2I3so/fT02yewcnGhBNjmLUnhtj+iw9WmuvKuNfD/rVNkkGlSbl
ZPRK/63cvMDImM/GvpkCQQD3hnZdXA9sz0yOEjCLzB/d9aPvwkDTfb2KGSGqTFS0
MW219UMXNPhz+MF1WEOb6UJVRwxcpfLeGjeDzKg2whby",
                Merchant_public_key = @"MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDB4GRcG4clcfX/X8et1gR0uNzk
SMuD7aj++PbbstMNs97C0LEuO7US+/rdGHDPJJiiEf3mlbpRDF6jBu7aNJf7DOyW
Zg6OYLBaccopZ0rYfCkwoLAhWU7coLbclToAhlHniq1ytqfkwpJl7r3Jz8SNMhf9
U9c+tylONbIFHF3NOQIDAQAB",
                Pid = "2088502894092597"
            };

            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliF2FPay.RedirectionTip", "You will be redirected to AliF2FPay site to complete the order.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliF2FPay.Alipay_public_key", "Alipay_public_key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliF2FPay.Alipay_public_key.Hint", "Enter Alipay_public_key.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliF2FPay.AppId", "AppId");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliF2FPay.AppId.Hint", "Enter AppId.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliF2FPay.Merchant_public_key", "Merchant_public_key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliF2FPay.Merchant_public_key.Hint", "Enter Merchant_public_key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliF2FPay.Merchant_private_key", "Merchant_private_key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliF2FPay.Merchant_private_key.Hint", "Enter Merchant_private_key.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliF2FPay.Pid", "Pid");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliF2FPay.Pid.Hint", "Enter partner.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliF2FPay.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliF2FPay.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliF2FPay.PaymentMethodDescription", "You will be redirected to AliF2FPay site to complete the order.");

            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.AliF2FPay.SellerEmail.RedirectionTip");
            this.DeletePluginLocaleResource("Plugins.Payments.AliF2FPay.Alipay_public_key");
            this.DeletePluginLocaleResource("Plugins.Payments.AliF2FPay.Alipay_public_key.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AliF2FPay.AppId");
            this.DeletePluginLocaleResource("Plugins.Payments.AliF2FPay.AppId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AliF2FPay.Merchant_public_key");
            this.DeletePluginLocaleResource("Plugins.Payments.AliF2FPay.Merchant_public_key.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AliF2FPay.Merchant_private_key");
            this.DeletePluginLocaleResource("Plugins.Payments.AliF2FPay.Merchant_private_key.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AliF2FPay.Pid");
            this.DeletePluginLocaleResource("Plugins.Payments.AliF2FPay.Pid.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AliF2FPay.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.AliF2FPay.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AliF2FPay.PaymentMethodDescription");

            base.Uninstall();
        }

        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription
        {
            get { return _localizationService.GetResource("Plugins.Payments.AliF2FPay.PaymentMethodDescription"); }
        }

        #endregion
    }
}
