using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;

namespace Smaprat.Website
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            // Ensure only a single view engine is loads
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            // Removes the X-AspNetWebPages-Version from response headers
            WebPageHttpHandler.DisableWebPagesResponseHeader = true;
        }
    }
}