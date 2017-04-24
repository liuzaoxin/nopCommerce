using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.AliF2FPay
{
    /// <summary>
    /// AliF2FPay payment processor
    /// </summary>
    public class AliF2FPayPaymentProcessor : BasePlugin, IPaymentMethod
    {
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
        }

        #endregion
    }
}
