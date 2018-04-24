# Call API Gateway with ASP.NET 

Making Request to API Gateway with IAM security with ASP.NET SDK

## Instructions

### How to use it
See the TestConsoleApp. Project

Or
var request = new AwsApiGatewayRequest()
{
    RegionName = "ap-southeast-2",
    Host = "ApiGateway-url",
    AccessKey = "Your-AccessKey",
    SecretKey = "Your-SecretKey",
    AbsolutePath = "Segments",
    JsonData = "data"
};
var apiRequest = new ApiRequest(request);
var response = apiRequest.GetPostResponse();