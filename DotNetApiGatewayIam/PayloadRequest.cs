using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DotNetApiGatewayIam
{
    public class PayloadRequest
    {
        private AwsApiGatewayRequest AwsApiGatewayRequest;

        private const string ServiceName = "execute-api";
        private const string Algorithm = "AWS4-HMAC-SHA256";
        private const string ContentType = "application/json";
        private const string DateTimeFormat = "yyyyMMddTHHmmssZ";
        private const string DateFormat = "yyyyMMdd";
        private const int DefaultRequestTimeOut = 50000;

        public PayloadRequest(AwsApiGatewayRequest request)
        {
            AwsApiGatewayRequest = request;

            if (string.IsNullOrEmpty(AwsApiGatewayRequest.JsonData))
                AwsApiGatewayRequest.JsonData = "";

            if (string.IsNullOrEmpty(AwsApiGatewayRequest.xApiKey))
                AwsApiGatewayRequest.xApiKey = "";
        }

        public WebRequest GetRequest()
        {
            string hashedRequestPayload = CreateRequestPayload(AwsApiGatewayRequest.JsonData);

            var currentDateTime = DateTime.UtcNow;
            string authorization = Sign(currentDateTime, hashedRequestPayload, AwsApiGatewayRequest.RequestMethod.ToString(), AwsApiGatewayRequest.AbsolutePath, AwsApiGatewayRequest.QueryString);
            string requestDate = currentDateTime.ToString(DateTimeFormat);

            var url = Utility.GetFullHttpsUrl(AwsApiGatewayRequest.Host);
            var webRequest = WebRequest.Create($"{url}{AwsApiGatewayRequest.AbsolutePath}");

            webRequest.Timeout = AwsApiGatewayRequest.RequestTimeout ?? DefaultRequestTimeOut;
            webRequest.Method = AwsApiGatewayRequest.RequestMethod.ToString();
            webRequest.ContentType = ContentType;
            webRequest.Headers.Add("X-Amz-date", requestDate);
            webRequest.Headers.Add("Authorization", authorization);
            if (!string.IsNullOrEmpty(AwsApiGatewayRequest.JsonData))
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

            if (!string.IsNullOrEmpty(AwsApiGatewayRequest.JsonData))
            {
                var encoding = new ASCIIEncoding();
                var data = encoding.GetBytes(AwsApiGatewayRequest.JsonData);
                webRequest.ContentLength = AwsApiGatewayRequest.JsonData.Length;

                using (var newStream = webRequest.GetRequestStream())
                {
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                }
            }

            return webRequest;
        }

        private string CreateRequestPayload(string jsonString)
        {
            return HexEncode(Hash(ToBytes(jsonString)));
        }

        private string Sign(DateTime currentDateTime, string hashedRequestPayload, string requestMethod, string canonicalUri, string canonicalQueryString)
        {
            var dateStamp = currentDateTime.ToString(DateFormat);
            var requestDate = currentDateTime.ToString(DateTimeFormat);
            var credentialScope = $"{dateStamp}/{AwsApiGatewayRequest.RegionName}/{ServiceName}/aws4_request";

            var host = Utility.GetUrlHost(AwsApiGatewayRequest.Host);
            var headers = new SortedDictionary<string, string> {
                { "content-type", ContentType },
                { "host", host },
                { "x-amz-date", requestDate }
            };

            var SignedHeaders = "content-type;host;x-amz-date";

            if (!string.IsNullOrEmpty(AwsApiGatewayRequest.xApiKey))
            {
                headers.Add("x-api-key", AwsApiGatewayRequest.xApiKey);
                SignedHeaders += ";x-api-key";
            }

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
