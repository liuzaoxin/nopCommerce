using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.AliF2FPay
{
    public partial class RouteProvider: IRouteProvider
    {
        #region Methods

        public void RegisterRoutes(RouteCollection routes)
        {
            //AliF2FPay
            routes.MapRoute("Plugin.Payments.AliF2FPay.AliF2FPay",
                 "Plugins/PaymentAliF2FPay/AliF2FPay",
                 new { controller = "PaymentAliF2FPay", action = "AliF2FPay" },
                 new[] { "Nop.Plugin.Payments.AliF2FPay.Controllers" }
            );

            //Return
            routes.MapRoute("Plugin.Payments.AliF2FPay.Return",
                 "Plugins/PaymentAliF2FPay/Return",
                 new { controller = "PaymentAliF2FPay", action = "Return" },
                 new[] { "Nop.Plugin.Payments.AliF2FPay.Controllers" }
            );
        }

        #endregion

        #region Properties

        public int Priority
        {
            get
            {
                return 0;
            }
        }

        #endregion 
    }
}