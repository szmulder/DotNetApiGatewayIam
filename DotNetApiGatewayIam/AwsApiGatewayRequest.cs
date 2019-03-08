using System;

namespace DotNetApiGatewayIam
{
    public class AwsApiGatewayRequest
    {       
        public string RegionName { get; set; }

        public string Host { get; set; }

        public string AbsolutePath { get; set; }

        public string QueryString { get; set; }

        public string JsonData { get; set; }

        public string SessionToken { get; set; }

        public string xApiKey { get; set; }

        public string AccessKey { get; set; }

        public string SecretKey { get; set; }

        public string RequestMethod { get; set; }

        public int? RequestTimeout { get; set; }
    }
}