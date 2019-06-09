using System.Net.Http;

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
        public HttpMethod RequestMethod { get; set; }
        public int? RequestTimeout { get; set; }
        public string AdditionalHeaders { get; set; }

        public override string ToString()
        {
            return $"Region:{RegionName}, Host:{Host}, Path:{AbsolutePath}, Query:{QueryString}, Data:{JsonData}, RequestMethod:{RequestMethod}";
        }
    }
}