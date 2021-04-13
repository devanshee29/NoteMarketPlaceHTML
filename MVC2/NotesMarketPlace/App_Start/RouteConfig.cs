using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NotesMarketPlace
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "RegisteredUser", action = "Home", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Login",
                url: "Admin/{id}",
                defaults: new { controller = "Admin", action = "Login", id = UrlParameter.Optional }
            );

        }
    }
}
