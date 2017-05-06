using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Threading;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;
using Nop.Plugin.Payments.AliF2FPay.Models;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Services.Logging;
using Nop.Services.Localization;
using Nop.Core.Domain.Payments;
using Com.Alipay.Business;
using Com.Alipay;
using Com.Alipay.Model;
using System.Net.Json;

namespace Nop.Plugin.Payments.AliF2FPay.Controllers
{
    public class PaymentAliF2FPayController : BasePaymentController
    {
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

        private IAlipayTradeService serviceClient;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly AliF2FPayPaymentSettings _aliF2FPayPaymentSettings;
        private readonly PaymentSettings _paymentSettings;

        #endregion

        #region Ctor

        public PaymentAliF2FPayController(ISettingService settingService,
            IPaymentService paymentService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILogger logger,
            ILocalizationService localizationService,
            AliF2FPayPaymentSettings aliF2FPayPaymentSettings,
            PaymentSettings paymentSettings)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._logger = logger;
            this._localizationService = localizationService;
            this._aliF2FPayPaymentSettings = aliF2FPayPaymentSettings;
            this._paymentSettings = paymentSettings;
            this.serviceClient = F2FBiz.CreateClientInstance(ServerUrl, _aliF2FPayPaymentSettings.AppId, _aliF2FPayPaymentSettings.Merchant_private_key, Version,
                             Sign_type, _aliF2FPayPaymentSettings.Alipay_public_key, Charset);
        }

        #endregion

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();

            return paymentInfo;
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();

            return warnings;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel
            {
                AdditionalFee = this._aliF2FPayPaymentSettings.AdditionalFee,
                Alipay_public_key = this._aliF2FPayPaymentSettings.Alipay_public_key,
                AppId = this._aliF2FPayPaymentSettings.AppId,
                Merchant_private_key = this._aliF2FPayPaymentSettings.Merchant_private_key,
                Merchant_public_key = this._aliF2FPayPaymentSettings.Merchant_public_key,
                Pid = this._aliF2FPayPaymentSettings.Pid
            };

            return View("~/Plugins/Payments.AliF2FPay/Views/Configure.cshtml", model);
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/Payments.AliF2FPay/Views/PaymentInfo.cshtml");
        }

        [ValidateInput(false)]
        public ActionResult AliF2FPay(string  paymoney, string payurl, string orderid)
        {
            ViewData["orderid"] = orderid;
            ViewData["AliF2FpayMoney"] = Math.Round(decimal.Parse(paymoney), 2).ToString();
            ViewData["AliF2FpayPayurl"] = payurl;
            
            return View("~/Plugins/Payments.AliF2FPay/Views/AliF2FPay.cshtml");
        }

        [ValidateInput(false)]
        public ActionResult QueryPaystatus(string orderId)
        {
            Thread.Sleep(2000);

            AlipayF2FQueryResult queryResult = new AlipayF2FQueryResult();
            JsonObjectCollection collection = null;
            collection = new JsonObjectCollection();
            collection.Add(new JsonStringValue("Reust", "FAILED"));
            queryResult = serviceClient.tradeQuery(orderId);
            int id;
            if (queryResult != null)
            {
                if (queryResult.Status == ResultEnum.SUCCESS)
                {
                    collection = new JsonObjectCollection();
                    collection.Add(new JsonStringValue("Reust", "SUCCESS"));
                    collection.Add(new JsonStringValue("OrderId", orderId));
                    if (int.TryParse(orderId, out id))
                    {
                        var order = _orderService.GetOrderById(id);

                        if (order != null && _orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            _orderProcessingService.MarkOrderAsPaid(order);
                        }
                    }
                }
            }
            
            return Json(collection.ToString());
        }

        /// <summary>
        /// 轮询
        /// </summary>
        /// <param name="o">订单号</param>
        public void LoopQuery(object o)
        {
            
            AlipayF2FQueryResult queryResult = new AlipayF2FQueryResult();
            int count = 100;
            int interval = 10000;
            string out_trade_no = o.ToString();

            for (int i = 1; i <= count; i++)
            {
                Thread.Sleep(interval);
                queryResult = serviceClient.tradeQuery(out_trade_no);
                if (queryResult != null)
                {
                    if (queryResult.Status == ResultEnum.SUCCESS)
                    {
                        DoSuccessProcess(queryResult);
                        return;
                    }
                }
            }
            DoFailedProcess(queryResult);
        }

        /// <summary>
        /// 请添加支付成功后的处理
        /// </summary>
        private void DoSuccessProcess(AlipayF2FQueryResult queryResult)
        {
            //支付成功，请更新相应单据
            //log.WriteLine("扫码支付成功：外部订单号" + queryResult.response.OutTradeNo);

        }

        /// <summary>
        /// 请添加支付失败后的处理
        /// </summary>
        private void DoFailedProcess(AlipayF2FQueryResult queryResult)
        {
            //支付失败，请更新相应单据
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            this._aliF2FPayPaymentSettings.AdditionalFee = model.AdditionalFee;
            this._aliF2FPayPaymentSettings.Alipay_public_key = model.Alipay_public_key;
            this._aliF2FPayPaymentSettings.AppId = model.AppId;
            this._aliF2FPayPaymentSettings.Merchant_private_key = model.Merchant_private_key;
            this._aliF2FPayPaymentSettings.Merchant_public_key = model.Merchant_public_key;
            this._aliF2FPayPaymentSettings.Pid = model.Pid;
            
            _settingService.SaveSetting(this._aliF2FPayPaymentSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }
    }
}