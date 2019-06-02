using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.Util;
using DotNetApiGatewayIam;
using Newtonsoft.Json;
using System;

namespace TestConsoleApp
{
    class Program
    {
        private const string apiEndpoint = "test.execute-api.ap-southeast-2.amazonaws.com";
        private const string apiEndpointStaging = "/Dev/test";

        static void Main(string[] args)
        {
            Console.WriteLine("Start");

            LocalProfileTest();
            //Ec2IamTest();

            Console.WriteLine("End");
            Console.ReadLine();
        }

        private static void Ec2IamTest()
        {
            var iamRoleName = EC2InstanceMetadata.GetData("/iam/security-credentials");
            var iamRole = EC2InstanceMetadata.GetData($"/iam/security-credentials/{iamRoleName}");
            var iamCredentials = JsonConvert.DeserializeObject<IamRoleCredentials>(iamRole);
			var iamAdditionalHeaders = "x-apigw-api-id='TEST';";

            Console.WriteLine(iamCredentials.AccessKeyId);

            var request = new AwsApiGatewayRequest()
            {
                RegionName = "ap-southeast-2",
                Host = apiEndpoint,
                AccessKey = iamCredentials.AccessKeyId,
                SecretKey = iamCredentials.SecretAccessKey,
                RequestMethod = "POST",
                AbsolutePath = apiEndpointStaging,
                JsonData = "245",
                SessionToken = iamCredentials.Token,
				AdditionalHeaders = iamAdditionalHeaders
            };
            var apiRequest = new ApiRequest(request);
            var response = apiRequest.GetPostResponse();
        }

        private static void LocalProfileTest()
        {
            var chain = new CredentialProfileStoreChain();
            var awsCredentials = default(AWSCredentials);
            if (chain.TryGetAWSCredentials("default", out awsCredentials))
            {
                var aws = awsCredentials.GetCredentials();

                var request = new AwsApiGatewayRequest()
                {
                    RegionName = "ap-southeast-2",
                    Host = apiEndpoint,
                    AccessKey = aws.AccessKey,
                    SecretKey = aws.SecretKey,
                    AbsolutePath = apiEndpointStaging,
                    JsonData = "245",
                    RequestMethod = "POST"
                };
                var apiRequest = new ApiRequest(request);
                var response = apiRequest.GetPostResponse();
            }
        }
    }
}