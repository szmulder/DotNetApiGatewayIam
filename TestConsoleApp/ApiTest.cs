using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.Util;
using DotNetApiGatewayIam;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace TestConsoleApp
{
    public class ApiTest : IApiTest
    {
        private readonly AppSettings _config;

        public ApiTest(IOptions<AppSettings> config)
        {
            _config = config.Value;
        }

        public async Task<bool> Run()
        {
            var postTestResponse = string.Empty;
            var getTestResponse = string.Empty;

            if (IsRunningOnEc2())
            {
                postTestResponse = await Ec2IamPostTest();
                getTestResponse = await Ec2IamGetTest();
            }
            else
            {
                postTestResponse = await LocalProfilePostTest();
                getTestResponse = await LocalProfileGetTest();
            }

            Console.WriteLine("Post Test Response:");
            Console.WriteLine(postTestResponse);
            Console.WriteLine();
            Console.WriteLine("Get Test Response:");
            Console.WriteLine(getTestResponse);

            return true;
        }

        public async Task<string> Ec2IamPostTest()
        {
            var response = string.Empty;

            var iamCredentials = GetEc2Credential();

            var request = new AwsApiGatewayRequest()
            {
                RegionName = _config.AwsRegion,
                Host = _config.ApiEndpoint,
                AccessKey = iamCredentials.AccessKeyId,
                SecretKey = iamCredentials.SecretAccessKey,
                AbsolutePath = _config.ApiEndpointStages,
                JsonData = _config.JsonData,
                SessionToken = iamCredentials.Token,
                RequestMethod = HttpMethod.Post
            };
            if(!string.IsNullOrEmpty(_config.IamAdditionalHeaders))
            {
                request.AdditionalHeaders = _config.IamAdditionalHeaders;
            }

            var apiRequest = new ApiRequest(request);
            response = await apiRequest.GetResponseStringAsync();

            return response;
        }

        public async Task<string> Ec2IamGetTest()
        {
            var response = string.Empty;

            var iamCredentials = GetEc2Credential();

            var request = new AwsApiGatewayRequest()
            {
                RegionName = _config.AwsRegion,
                Host = _config.ApiEndpoint,
                AccessKey = iamCredentials.AccessKeyId,
                SecretKey = iamCredentials.SecretAccessKey,
                RequestMethod = HttpMethod.Get,
                AbsolutePath = $"{_config.ApiEndpointStages}{_config.GetId}"
            };
            if (!string.IsNullOrEmpty(_config.IamAdditionalHeaders))
            {
                request.AdditionalHeaders = _config.IamAdditionalHeaders;
            }

            var apiRequest = new ApiRequest(request);
            response = await apiRequest.GetResponseStringAsync();

            return response;
        }

        public async Task<string> LocalProfilePostTest()
        {
            var response = string.Empty;

            var awsCredentials = GetAwsCredential();
            if (awsCredentials != null)
            {
                var aws = awsCredentials.GetCredentials();

                var request = new AwsApiGatewayRequest()
                {
                    RegionName = _config.AwsRegion,
                    Host = _config.ApiEndpoint,
                    AccessKey = aws.AccessKey,
                    SecretKey = aws.SecretKey,
                    AbsolutePath = _config.ApiEndpointStages,
                    JsonData = _config.JsonData,
                    RequestMethod = HttpMethod.Post
                };
                var apiRequest = new ApiRequest(request);
                response = await apiRequest.GetResponseStringAsync();
            }

            return response;
        }

        public async Task<string> LocalProfileGetTest()
        {
            var response = string.Empty;

            var awsCredentials = GetAwsCredential();
            if (awsCredentials != null)
            {
                var aws = awsCredentials.GetCredentials();

                var request = new AwsApiGatewayRequest()
                {
                    RegionName = _config.AwsRegion,
                    Host = _config.ApiEndpoint,
                    AccessKey = aws.AccessKey,
                    SecretKey = aws.SecretKey,
                    RequestMethod = HttpMethod.Get,
                    AbsolutePath = $"{_config.ApiEndpointStages}{_config.GetId}"
                };
                var apiRequest = new ApiRequest(request);
                response = await apiRequest.GetResponseStringAsync();
            }

            return response;
        }

        private bool IsRunningOnEc2()
        {
            bool isOnEc2 = false;

            try
            {
                var iamRoleName = EC2InstanceMetadata.GetData("/iam/security-credentials");
                isOnEc2 = !string.IsNullOrEmpty(iamRoleName);
            }
            catch (Exception)
            {
            }
            return isOnEc2;
        }

        private AWSCredentials GetAwsCredential()
        {
            var chain = new CredentialProfileStoreChain();
            var awsCredentials = default(AWSCredentials);
            chain.TryGetAWSCredentials(_config.LocaAwsProfile, out awsCredentials);
            return awsCredentials;
        }

        private IamRoleCredentials GetEc2Credential()
        {
            var iamRoleName = EC2InstanceMetadata.GetData("/iam/security-credentials");
            var iamRole = EC2InstanceMetadata.GetData($"/iam/security-credentials/{iamRoleName}");
            var iamCredentials = JsonConvert.DeserializeObject<IamRoleCredentials>(iamRole);

            return iamCredentials;
        }
    }
}
