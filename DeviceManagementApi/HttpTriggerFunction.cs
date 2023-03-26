using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using DeviceManagementApi.Models;
using DeviceManagementApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace DeviceManagementApi
{
    public class HttpTriggerFunction
    {
        private readonly ILogger<HttpTriggerFunction> _logger;
        private readonly IInventoryService _inventoryService;
        private readonly ICosmosClientService _cosmosClientService;

        public HttpTriggerFunction(ILogger<HttpTriggerFunction> log, IInventoryService inventoryService, ICosmosClientService cosmosClientService)
        {
            _logger = log;
            _inventoryService = inventoryService;
            _cosmosClientService = cosmosClientService;
        }

        [FunctionName("Register")]
        [OpenApiOperation(operationId: "Run")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody("application/json", typeof(RequestModel), Description = "JSON request body containing a correlation Id and a devices list")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(OkResponseModel), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] RequestModel request)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            if (request.Devices?.Count > 0)
            {
                InventoryDeviceListModel inventory = null;

                var deviceIds = request.Devices.Select(x => x.Id).ToArray();

                try
                {
                    inventory = await _inventoryService.PostAsync(deviceIds);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return new InternalServerErrorResult();
                }

                var records = request.Devices.Join(inventory.Devices,
                    rd => rd.Id,
                    id => id.DeviceId,
                    (rd, id) => new DeviceRecordModel
                    {
                        Id = rd.Id,
                        Name = rd.Name,
                        Location = rd.Location,
                        Type = rd.Type,
                        AssetId = id.AssetId
                    }).ToList();

                var failedDeviceIds = new List<string>();    

                try
                {
                    failedDeviceIds = await _cosmosClientService.StoreDevices(records);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return new InternalServerErrorResult();
                }

                var failureCount = failedDeviceIds.Count;

                var response = new OkResponseModel
                {
                    TotalCount = request.Devices.Count,
                    RegisterSuccessCount = request.Devices.Count - failureCount,
                    RegisterFailureCount = failureCount
                };

                if (failureCount > 0)
                {
                    response.FailedDevices = request.Devices.Where(x => failedDeviceIds.Contains(x.Id)).ToList();
                }

                return new OkObjectResult(response);
            }

            return new BadRequestObjectResult("No devices can be found in the request.");
        }
    }
}

