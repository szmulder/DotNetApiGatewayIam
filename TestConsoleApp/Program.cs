using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using DotNetApiGatewayIam;
using System;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            var chain = new CredentialProfileStoreChain();
            var awsCredentials = default(AWSCredentials);
            if (chain.TryGetAWSCredentials("default", out awsCredentials))
            {
                var request = new AwsApiGatewayRequest()
                {
                    RegionName = "ap-southeast-2",
                    Host = "ApiGateway-url",
                    AccessKey = awsCredentials.GetCredentials().AccessKey,
                    SecretKey = awsCredentials.GetCredentials().SecretKey,
                    AbsolutePath = "Segments",
                    JsonData = "data",
                    RequestMethod = "POST"
                };
                var apiRequest = new ApiRequest(request);
                var response = apiRequest.GetPostResponse();
            }
            

            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}