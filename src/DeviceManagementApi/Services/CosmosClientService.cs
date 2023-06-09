﻿using DeviceManagementApi.Models;
using DeviceManagementApi.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceManagementApi.Services
{
    public class CosmosClientService : ICosmosClientService
    {
        private readonly ILogger<CosmosClientService> _logger;
        private readonly CosmosDbOptions _serviceOptions;
        private Database _db;

        public CosmosClientService(ILogger<CosmosClientService> logger, IOptions<AppOptions> appOptions)
        {
            _logger = logger;
            _serviceOptions = appOptions?.Value.CosmosDbOptions ?? throw new ArgumentNullException(nameof(appOptions));
        }

        public async Task<List<string>> StoreDevices(List<DeviceRecordModel> devices)
        {
            _db = new CosmosClient(_serviceOptions.Uri, _serviceOptions.Key)?.GetDatabase(_serviceOptions.DatabaseName);

            var container = await GetContainer();

            var failedDevices = new List<string>();

            foreach (var device in devices)
            {
                try
                {
                    await container.UpsertItemAsync<DeviceRecordModel>(device, new PartitionKey(device.Id));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, device.Id);
                    failedDevices.Add(device.Id);
                }
            }

            return failedDevices;
        }

        private async Task<Container> GetContainer()
        {
            return await _db.CreateContainerIfNotExistsAsync(_serviceOptions.ContainerName, "/id", 400);
        }
    }
}
