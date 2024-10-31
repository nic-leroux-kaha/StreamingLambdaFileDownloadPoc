using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Constructs;

namespace Cdk
{
    public class StreamingLambdaFileDownloadPocStack : Stack
    {
        internal StreamingLambdaFileDownloadPocStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var lambdaRole = new Role(this, "My Role", new RoleProps
            {
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
            });

            lambdaRole.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaBasicExecutionRole"));

            // Reponse Streaming LLM Lambda
            var fileStreamingLambda = new Function(this, "FileStreamingLambda", new FunctionProps
            {
                Runtime = Runtime.NODEJS_20_X,
                Code = Code.FromAsset("src/fileStreamingLambda"),
                Handler = "index.handler",
                Role = lambdaRole,
                Timeout = Duration.Seconds(60)
            });

            _ = new LogGroup(this, "fileStreamingLambdaLogGroup", new LogGroupProps
            {
                LogGroupName = "/aws/lambda/" + fileStreamingLambda.FunctionName,
                RemovalPolicy = RemovalPolicy.DESTROY,
                Retention = RetentionDays.ONE_WEEK
            });

            var fileStreamingLamdaFunctionUrl = fileStreamingLambda.AddFunctionUrl(new FunctionUrlOptions
            {
                AuthType = FunctionUrlAuthType.NONE,
                InvokeMode = InvokeMode.RESPONSE_STREAM,
                Cors = new FunctionUrlCorsOptions
                {
                    AllowedOrigins = new[] { "*" },
                    AllowCredentials = true,
                    AllowedMethods = new[] { HttpMethod.ALL },
                    AllowedHeaders = new[] { "*" }
                }
            });

            // Output the streaming lambda function url
            _ = new CfnOutput(this, "fileStreamingLambdaUrl", new CfnOutputProps
            {
                Description = "The streaming lambda function url",
                Value = fileStreamingLamdaFunctionUrl.Url
            });
        }
    }
}
