using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ELibrary
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Library",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "MainLibrary", action = "GetBooks", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "SearchAutocomplete",
                url: "Home/SearchAutocomplete",
                defaults: new { controller = "Home", action = "SearchAutocomplete" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "HomePage", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Sign_Up",
                url: "Login/Sign_Up",
                defaults: new { controller = "Login", action = "Sign_Up" }
            );
            routes.MapRoute(
                name: "Forgot_password",
                url: "Login/Forgot_password",
                defaults: new { controller = "Login", action = "Forgot_password" }
            );
        }
    }
}
