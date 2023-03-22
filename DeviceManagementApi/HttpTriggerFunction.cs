using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DeviceManagementApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace DeviceManagementApi
{
    public class HttpTriggerFunction
    {
        private readonly ILogger<HttpTriggerFunction> _logger;

        public HttpTriggerFunction(ILogger<HttpTriggerFunction> log)
        {
            _logger = log;
        }

        [FunctionName("Register")]
        [OpenApiOperation(operationId: "Run")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody("application/json", typeof(RequestModel), Description = "JSON request body containing a correlation Id and a devices list")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] RequestModel request)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            if (request.Devices?.Count > 0)
            {
                string responseMessage = string.IsNullOrEmpty(request?.CorrelationId)
                    ? "This HTTP triggered function executed successfully. Pass a CorrelationId in the request body for a personalized response."
                    : $"Hello, {request.CorrelationId}. This HTTP triggered function executed successfully.";

                return new OkObjectResult(responseMessage);
            }

            return new BadRequestObjectResult("No devices can be found in the request.");
        }
    }
}

