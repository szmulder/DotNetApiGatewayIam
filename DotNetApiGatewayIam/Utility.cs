using System;
namespace DotNetApiGatewayIam
{
    public class Utility
    {
        public const string UriSchemeHttp = "http";
        public const string UriSchemeHttps = "https";

        public static string GetFullHttpsUrl(string url)
        {
            string result = url;

            if (!url.StartsWith(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                if (url.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
                {
                    result = result.Replace(Uri.UriSchemeHttp, UriSchemeHttps);
                }
                else
                    result = string.Concat(Uri.UriSchemeHttps, "://", result);
            }

            return result;
        }

        public static string GetUrlHost(string url)
        {
            var host = url;

            if(url.StartsWith(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) || url.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(url);
                host = uri.Host;
            }

            return host;
        }
    }
}
