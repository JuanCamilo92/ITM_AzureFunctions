using AzureFunctions.Functions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureFunctions.Functions.Functions
{
    public static class ScheduledFunction
    {
        [FunctionName("ScheduledFunction")]
        public static async Task Consolidated(
            [TimerTrigger("0 */10 * * * *")] TimerInfo myTimer,
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

                log.LogInformation($"Consolidated items at: {DateTime.Now}");
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
