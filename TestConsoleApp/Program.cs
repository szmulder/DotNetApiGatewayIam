using DotNetApiGatewayIam;
using System;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");

            var request = new AwsApiGatewayRequest()
            {
                RegionName = "ap-southeast-2",
                Host = "ApiGateway-url",
                AccessKey = "Your-AccessKey",
                SecretKey = "Your-SecretKey",
                AbsolutePath = "Segments",
                JsonData = "data",
                RequestMethod = "POST"
            };
            var apiRequest = new ApiRequest(request);
            var response = apiRequest.GetPostResponse();

            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}