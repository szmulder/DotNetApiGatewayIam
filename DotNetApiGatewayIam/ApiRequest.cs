using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DotNetApiGatewayIam
{
    public class ApiRequest
    {
        public PayloadRequest PayloadRequest;

        public ApiRequest(AwsApiGatewayRequest request)
        {
            PayloadRequest = new PayloadRequest(request);
        }

        public async Task<string> GetResponseStringAsync()
        {
            var result = string.Empty;

            var response = await GetResponseAsync();
            using (var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream, Encoding.UTF8);
                result = reader.ReadToEnd();
            };

            return result;
        }

        public async Task<WebResponse> GetResponseAsync()
        {
            var request = PayloadRequest.GetRequest();
            return await request.GetResponseAsync();
        }

        public HttpWebResponse GetResponse()
        {
            var request = PayloadRequest.GetRequest();
            return (HttpWebResponse)(request.GetResponse());
        }

        //Obsolete
        [ObsoleteAttribute("This method will soon be deprecated. Use GetResponse() instead.")]
        public WebResponse GetPostResponse()
        {
            var request = PayloadRequest.GetRequest();
            return request.GetResponse();
        }
    }
}