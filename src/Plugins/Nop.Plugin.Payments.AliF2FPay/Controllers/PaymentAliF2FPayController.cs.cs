using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;
using Nop.Plugin.Payments.AliF2FPay.Models;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Services.Logging;
using Nop.Services.Localization;
using Nop.Core.Domain.Payments;

namespace Nop.Plugin.Payments.AliF2FPay.Controllers
{
    public class PaymentAliF2FPayController : BasePaymentController
    {
        #region Fields

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