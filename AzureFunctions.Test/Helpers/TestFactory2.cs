using AzureFunctions.Common.Models;
using AzureFunctions.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.IO;

namespace AzureFunctions.Test.Helpers
{
    public class TestFactory2
    {
        public static ConsolidatedEntity GetConsolidatedEntity()
        {
            return new ConsolidatedEntity
            {
                EmployedId = 98,
                DateEmployed = DateTime.UtcNow,
                Minutes = 60,
                ETag = "*",
                PartitionKey = "consolidated",
                RowKey = Guid.NewGuid().ToString()
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid consolidatedId, Consolidated consolidatedRequest)
        {
            string request = JsonConvert.SerializeObject(consolidatedRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{consolidatedId}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Consolidated consolidatedRequest)
        {
            string request = JsonConvert.SerializeObject(consolidatedRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request)
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid consolidatedId)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{consolidatedId}"
            };
        }
        public static DefaultHttpRequest CreateHttpRequest()
        {

            return new DefaultHttpRequest(new DefaultHttpContext());
        }

        public static Consolidated GetConsolidatedRequest()
        {
            return new Consolidated
            {
                DateEmployed = DateTime.UtcNow,
                Minutes = 40,
                EmployedId = 97
            };
        }

        public static Stream GenerateStreamFromString(string stringToConvert)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine(stringToConvert);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.NULL)
        {
            ILogger logger;
            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }
            return logger;
        }
    }
}
