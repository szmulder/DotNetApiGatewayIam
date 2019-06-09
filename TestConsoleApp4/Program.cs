using Amazon;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.Util;
using DotNetApiGatewayIam;
using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace TestConsoleApp4
{
    class Program
    {
        private const string apiEndpoint = "test.execute-api.ap-southeast-2.amazonaws.com";
        private const string apiEndpointStaging = "/Dev/test";

        static void Main(string[] args)
        {
            Console.WriteLine("Start");

            //Ec2IamTest();
            LocalProfileTest();

            Console.WriteLine("End");
            Console.ReadLine();
        }


        private static void Ec2IamTest()
        {
            var iamRoleName = EC2InstanceMetadata.GetData("/iam/security-credentials");
            var iamRole = EC2InstanceMetadata.GetData($"/iam/security-credentials/{iamRoleName}");
            var iamCredentials = JsonConvert.DeserializeObject<IamRoleCredentials>(iamRole);

            Console.WriteLine(iamCredentials.AccessKeyId);
            Console.WriteLine(iamCredentials.Token);

            var request = new AwsApiGatewayRequest()
            {
                RegionName = "ap-southeast-2",
                Host = apiEndpoint,
                AccessKey = iamCredentials.AccessKeyId,
                SecretKey = iamCredentials.SecretAccessKey,
                AbsolutePath = apiEndpointStaging,
                JsonData = "245",
                SessionToken = iamCredentials.Token,
                RequestMethod = HttpMethod.Post
            };
            var apiRequest = new ApiRequest(request);
            var response = apiRequest.GetResponse();

            Console.WriteLine(response.ContentLength);
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
                    RequestMethod = HttpMethod.Post
                };
                var apiRequest = new ApiRequest(request);
                var response = apiRequest.GetResponse();

                Console.WriteLine(response.ContentLength);
            }
        }

        public static Credentials GetTemporaryCredentials(string policy)
        {
            var config = new AmazonSecurityTokenServiceConfig
            {
                RegionEndpoint = RegionEndpoint.APSoutheast2
            };
            var client = new AmazonSecurityTokenServiceClient(config);
            var iamClient = new AmazonIdentityManagementServiceClient(
                RegionEndpoint.APSoutheast2);

            var iamRoleName = EC2InstanceMetadata.GetData("/iam/security-credentials");
            var role = iamClient.GetRole(
                new GetRoleRequest() { RoleName = iamRoleName });
            var assumeRoleRequest = new AssumeRoleRequest()
            {
                RoleArn = role.Role.Arn,
                RoleSessionName = Guid.NewGuid().ToString().Replace("-", ""),
                DurationSeconds = 900
            };

            if (!string.IsNullOrEmpty(policy))
            {
                assumeRoleRequest.Policy = policy;
            }

            var assumeRoleResponse =
                         client.AssumeRole(assumeRoleRequest);
            var credentials = assumeRoleResponse.Credentials;

            return credentials;
        }
    }
}
