using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace emre_tatliyer_h5190021_yonlendirilmiscalismaproje
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}/{magazaturuid}/{urunid}",
                defaults: new { controller = "Home", action = "Giris", id = UrlParameter.Optional, magazaturuid = UrlParameter.Optional, urunid = UrlParameter.Optional }
            );
        }
    }
    
}
