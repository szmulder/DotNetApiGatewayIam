# Call API Gateway with ASP.NET 

Making Request to API Gateway with IAM security with ASP.NET SDK

## Instructions

### How to use it
See the TestConsoleApp. Project
You just need change the value in the appsettings.json and you should able to do test on your local computer

Or

Http.Post 
```c#
var request = new AwsApiGatewayRequest()
{
    RegionName = "ap-southeast-2",
    Host = "ApiGateway-url",
    AccessKey = "Your-AccessKey",
    SecretKey = "Your-SecretKey",
    AbsolutePath = "Stages",
    JsonData = "data",
    RequestMethod = HttpMethod.Post
};
var apiRequest = new ApiRequest(request);
var response = await apiRequest.GetResponseStringAsync();
```

Http.Get
```c#
var request = new AwsApiGatewayRequest()
{
    RegionName = _config.AwsRegion,
    Host = "ApiGateway-url",,
    AccessKey = "Your-AccessKey",
    SecretKey = "Your-SecretKey",
    RequestMethod = HttpMethod.Get,
    AbsolutePath = $"{Stages}{Your-Id}"
};
var apiRequest = new ApiRequest(request);
response = await apiRequest.GetResponseStringAsync();
```               
                
### Modified OneTechnologies 

This was forked from https://github.com/szmulder/DotNetApiGatewayIam and modified slightly to support additional headers on API call 
which was necessary for AWS Kinesis write (replacement for Couchbase).