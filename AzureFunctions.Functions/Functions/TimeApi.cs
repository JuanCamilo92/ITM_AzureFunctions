using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using AzureFunctions.Common.Models;
using AzureFunctions.Common.Responses;
using AzureFunctions.Functions.Entities;
using System.Collections.Generic;
using System.Linq;

namespace AzureFunctions.Functions.Functions
{
    public static class TimeApi
    {
        [FunctionName(nameof(CreateTime))]
        public static async Task<IActionResult> CreateTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Time")] HttpRequest req,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            ILogger log)
        {
            log.LogInformation("Recieved a new time.");

            //string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            EntryTime time = JsonConvert.DeserializeObject<EntryTime>(requestBody);


            if (time?.EmployedId != null && time.EmployedId <= 0)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have a employed."
                });
            }

            TimeEntity timeEntity = new TimeEntity
            {
                DateEmployed = time.DateEmployed,
                ETag = "*",
                Consolidated = false,
                PartitionKey = "time",
                RowKey = Guid.NewGuid().ToString(),
                EntryOrExit = time.EntryOrExit,
                EmployedId = time.EmployedId
            };

            TableOperation addOperation = TableOperation.Insert(timeEntity);
            await timeTable.ExecuteAsync(addOperation);

            string message = "New time stored in table";
            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = timeEntity
            });
        }

        [FunctionName(nameof(UpdateTime))]
        public static async Task<IActionResult> UpdateTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "Time/{id}")] HttpRequest req,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Update for id: {id}. recieved.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            EntryTime time = JsonConvert.DeserializeObject<EntryTime>(requestBody);

            //validate id
            TableOperation findOperation = TableOperation.Retrieve<TimeEntity>("time", id);
            TableResult findResult = await timeTable.ExecuteAsync(findOperation);

            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Time not found."
                });
            }

            //Update
            TimeEntity timeEntity = (TimeEntity)findResult.Result;
            if (!string.IsNullOrEmpty(time.DateEmployed.ToString()))
            {
                timeEntity.DateEmployed = time.DateEmployed;
            }

            TableOperation addOperation = TableOperation.Replace(timeEntity);
            await timeTable.ExecuteAsync(addOperation);

            string message = $"Date: {id}, updated in table.";
            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = timeEntity
            });
        }

        [FunctionName(nameof(GetTimeById))]
        public static async Task<IActionResult> GetTimeById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Time/{id}")] HttpRequest req,
            [Table("time", "time", "{id}", Connection = "AzureWebJobsStorage")] TimeEntity timeEntity,
            string id,
            ILogger log)
        {
            log.LogInformation($"Get time by id: {id}, received");

            if (timeEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Time not found."
                });
            }

            string message = $"Time: {timeEntity.RowKey}, retrieved.";
            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = timeEntity
            });
        }

        [FunctionName(nameof(DeleteTime))]
        public static async Task<IActionResult> DeleteTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Time/{id}")] HttpRequest req,
            [Table("time", "time", "{id}", Connection = "AzureWebJobsStorage")] TimeEntity timeEntity,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Delete time: {id}, received");

            if (timeEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Time not found."
                });
            }

            await timeTable.ExecuteAsync(TableOperation.Delete(timeEntity));
            string message = $"Time: {timeEntity.RowKey}, deleted.";
            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = timeEntity
            });
        }

        [FunctionName(nameof(GetAllTime))]
        public static async Task<IActionResult> GetAllTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Time")] HttpRequest req,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            ILogger log)
        {
            TableQuery<TimeEntity> query = new TableQuery<TimeEntity>();
            TableQuerySegment<TimeEntity> times = await timeTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "NRetrieved all times.";
            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = times
            });
        }

        [FunctionName(nameof(Consolidated))]
        public static async Task<IActionResult> Consolidated(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Consolidated")] HttpRequest req,
           [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
           [Table("consolidated", Connection = "AzureWebJobsStorage")] CloudTable consolidatedTable,
           ILogger log
           )
        {
            try
            {
                log.LogInformation($"Consolidated completed function executed at: {DateTime.Now}");
                string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
                TableQuery<TimeEntity> query = new TableQuery<TimeEntity>().Where(filter);
                TableQuerySegment<TimeEntity> consolidatedTime = await timeTable.ExecuteQuerySegmentedAsync(query, null);

                IEnumerable<TimeEntity> items = (from t in consolidatedTime select t);
 
                TimeEntity timeOld = new TimeEntity();
                foreach (TimeEntity item in items.OrderBy(x => x.EmployedId).ThenBy(y => y.EntryOrExit))
                {
                    
                    if (item.EntryOrExit != "Salida")
                        timeOld = item;
                    else
                    {
                        if (item.EmployedId == timeOld.EmployedId && item.EntryOrExit != timeOld.EntryOrExit)
                        {
                            timeOld.Consolidated = true;
                            item.Consolidated = true;
                            await timeTable.ExecuteAsync(TableOperation.Replace(item));
                            await timeTable.ExecuteAsync(TableOperation.Replace(timeOld));

                            string filterEmploye = TableQuery.GenerateFilterConditionForInt("EmployedId", QueryComparisons.Equal, item.EmployedId);
                            TableQuery<ConsolidatedEntity> consolidatedquery = new TableQuery<ConsolidatedEntity>().Where(filterEmploye);
                            TableQuerySegment<ConsolidatedEntity> consolidated = await consolidatedTable.ExecuteQuerySegmentedAsync(consolidatedquery, null);

                            if (!consolidated.Any())
                            {
                                ConsolidatedEntity consolidatedEntity = new ConsolidatedEntity
                                {
                                    EmployedId = item.EmployedId,
                                    DateEmployed = item.DateEmployed,
                                    Minutes = Convert.ToDouble((item.DateEmployed - timeOld.DateEmployed).TotalMinutes),
                                    ETag = "*",
                                    PartitionKey = "consolidated",
                                    RowKey = Guid.NewGuid().ToString()

                                };
                                await consolidatedTable.ExecuteAsync(TableOperation.Insert(consolidatedEntity));
                            }
                            else
                            {
                                ConsolidatedEntity consolidatedEntity = (from c in consolidated select c)
                                                                         .Where(x => x.EmployedId == item.EmployedId)
                                                                         .FirstOrDefault();
                                consolidatedEntity.Minutes = Convert.ToDouble((item.DateEmployed - timeOld.DateEmployed).TotalMinutes) + (consolidatedEntity.Minutes);

                                await consolidatedTable.ExecuteAsync(TableOperation.Replace(consolidatedEntity));
                            }
                            timeOld = null;
                        }
                    }
                }

                return new OkObjectResult(new Response
                {
                    IsSuccess = true,
                    Message = "",
                    Result = items.OrderBy(x => x.EmployedId).ThenBy(y => y.EntryOrExit)
                });
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [FunctionName(nameof(GetAllConsolidated))]
        public static async Task<IActionResult> GetAllConsolidated(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "FinalConsolidated")] HttpRequest req,
            [Table("consolidated", Connection = "AzureWebJobsStorage")] CloudTable consolidatedTable,
            ILogger log)
        {
            TableQuery<TimeEntity> query = new TableQuery<TimeEntity>();
            TableQuerySegment<TimeEntity> consolidated = await consolidatedTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "NRetrieved all times.";
            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = consolidated
            });
        }
    }
}
