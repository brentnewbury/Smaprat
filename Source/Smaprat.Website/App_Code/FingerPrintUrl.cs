using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

/* 
 * Adapted from technique developed by Mads Kristensen
 * http://madskristensen.net/post/cache-busting-in-aspnet
 */

public class FingerPrintUrl
{
    private static string CdnUri = ConfigurationManager.AppSettings["site:CdnUri"];

    public static string Create(string url)
    {
        if (String.IsNullOrWhiteSpace(url))
            return String.Empty;

        if (HttpRuntime.Cache[url] == null)
        {
            string abolutePath = VirtualPathUtility.ToAbsolute("~" + url);
            string physicalPath = HostingEnvironment.MapPath(abolutePath);

            string result = url;

            if (File.Exists(physicalPath))
            {
                DateTime date = File.GetLastWriteTime(physicalPath);
                result += "?v=" + date.Ticks;
            }

            if (!HttpContext.Current.Request.Url.IsLoopback &&
                !String.IsNullOrWhiteSpace(CdnUri))
                result = result.Insert(0, CdnUri);

            HttpRuntime.Cache.Insert(url, result, new CacheDependency(physicalPath));
        }

        return HttpRuntime.Cache[url] as string;
    }
}