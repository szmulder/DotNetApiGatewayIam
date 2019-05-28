using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using System.Security.Cryptography;

namespace DotNetApiGatewayIam
{
    public class ApiRequest
    {
        private const string ServiceName = "execute-api";
        private const string Algorithm = "AWS4-HMAC-SHA256";
        private const string ContentType = "application/json";
        private const string SignedHeaders = "content-type;host;x-amz-date;x-api-key";
        private const string DateTimeFormat = "yyyyMMddTHHmmssZ";
        private const string DateFormat = "yyyyMMdd";

        public AwsApiGatewayRequest AwsApiGatewayRequest;

        public ApiRequest(AwsApiGatewayRequest request)
        {
            AwsApiGatewayRequest = request;

            if(string.IsNullOrEmpty(AwsApiGatewayRequest.RequestMethod))
                AwsApiGatewayRequest.RequestMethod = "POST";

            if (string.IsNullOrEmpty(AwsApiGatewayRequest.xApiKey))
                AwsApiGatewayRequest.xApiKey = "";
        }

        public WebResponse GetPostResponse()
        {
            var request = GetPostRequest();
            return request.GetResponse();
        }

        public WebRequest GetPostRequest()
        {
            string hashedRequestPayload = CreateRequestPayload(AwsApiGatewayRequest.JsonData);

            string authorization = Sign(hashedRequestPayload, AwsApiGatewayRequest.RequestMethod, AwsApiGatewayRequest.AbsolutePath, AwsApiGatewayRequest.QueryString);
            string requestDate = DateTime.UtcNow.ToString(DateTimeFormat);

            var webRequest = WebRequest.Create($"https://{AwsApiGatewayRequest.Host}{AwsApiGatewayRequest.AbsolutePath}");

            webRequest.Timeout = AwsApiGatewayRequest.RequestTimeout.HasValue ? AwsApiGatewayRequest.RequestTimeout.Value : 50000;
            webRequest.Method = AwsApiGatewayRequest.RequestMethod;
            webRequest.ContentType = ContentType;
            webRequest.Headers.Add("X-Amz-date", requestDate);
            webRequest.Headers.Add("Authorization", authorization);
            webRequest.Headers.Add("x-amz-content-sha256", hashedRequestPayload);

			if (!string.IsNullOrEmpty(AwsApiGatewayRequest.AdditionalHeaders))
			{
				// parse apart and apply the additional headers
				string[] headers = AwsApiGatewayRequest.AdditionalHeaders.Split(';');
				foreach (string header in headers)
				{
					var headervalue = header.Split('=');
					if (headervalue.Count() == 2)
						webRequest.Headers.Add(headervalue[0], headervalue[1]);
				}
			}

			if (!string.IsNullOrEmpty(AwsApiGatewayRequest.SessionToken))
                webRequest.Headers.Add("X-Amz-Security-Token", AwsApiGatewayRequest.SessionToken);
            webRequest.ContentLength = AwsApiGatewayRequest.JsonData.Length;

            var encoding = new ASCIIEncoding();
            var data = encoding.GetBytes(AwsApiGatewayRequest.JsonData);

			using (var newStream = webRequest.GetRequestStream())
			{
				newStream.Write(data, 0, data.Length);
				newStream.Close();
			}
				
            return webRequest;
        }

        private string CreateRequestPayload(string jsonString)
        {
            return HexEncode(Hash(ToBytes(jsonString)));
        }

        private string Sign(string hashedRequestPayload, string requestMethod, string canonicalUri, string canonicalQueryString)
        {
            var currentDateTime = DateTime.UtcNow;

            var dateStamp = currentDateTime.ToString(DateFormat);
            var requestDate = currentDateTime.ToString(DateTimeFormat);
            var credentialScope = $"{dateStamp}/{AwsApiGatewayRequest.RegionName}/{ServiceName}/aws4_request";

            var headers = new SortedDictionary<string, string> {
                { "content-type", ContentType },
                { "host", AwsApiGatewayRequest.Host },
                { "x-amz-date", requestDate },
                { "x-api-key", AwsApiGatewayRequest.xApiKey }
            };

            var canonicalHeaders = string.Join("\n", headers.Select(x => x.Key.ToLowerInvariant() + ":" + x.Value.Trim())) + "\n";

            // Task 1: Create a Canonical Request For Signature Version 4
            var canonicalRequest = $"{requestMethod}\n{canonicalUri}\n{canonicalQueryString}\n{canonicalHeaders}\n{SignedHeaders}\n{hashedRequestPayload}";
            var hashedCanonicalRequest = HexEncode(Hash(ToBytes(canonicalRequest)));

            // Task 2: Create a String to Sign for Signature Version 4
            var stringToSign = $"{Algorithm}\n{requestDate}\n{credentialScope}\n{hashedCanonicalRequest}";

            // Task 3: Calculate the AWS Signature Version 4
            var signingKey = GetSignatureKey(AwsApiGatewayRequest.SecretKey, dateStamp, AwsApiGatewayRequest.RegionName, ServiceName);
            var signature = HexEncode(HmacSha256(stringToSign, signingKey));

            // Task 4: Prepare a signed request
            // Authorization: algorithm Credential=access key ID/credential scope, SignedHeadaers=SignedHeaders, Signature=signature
            var authorization = $"{Algorithm} Credential={AwsApiGatewayRequest.AccessKey}/{dateStamp}/{AwsApiGatewayRequest.RegionName}/{ServiceName}/aws4_request, SignedHeaders={SignedHeaders}, Signature={signature}";

            return authorization;
        }

        private byte[] GetSignatureKey(string key, string dateStamp, string regionName, string serviceName)
        {
            var kDate = HmacSha256(dateStamp, ToBytes("AWS4" + key));
            var kRegion = HmacSha256(regionName, kDate);
            var kService = HmacSha256(serviceName, kRegion);
            return HmacSha256("aws4_request", kService);
        }

        private byte[] ToBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str.ToCharArray());
        }

        private string HexEncode(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
        }

        private byte[] Hash(byte[] bytes)
        {
            return SHA256.Create().ComputeHash(bytes);
        }

        private byte[] HmacSha256(string data, byte[] key)
        {
            return new HMACSHA256(key).ComputeHash(ToBytes(data));
        }
    }
}