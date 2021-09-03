using AzureFunctions.Common.Models;
using AzureFunctions.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzureFunctions.Test.Helpers
{
    public class TestFactory
    {
        public static TimeEntity GetTimeEntity()
        {
            return new TimeEntity
            {
                DateEmployed = DateTime.UtcNow,
                ETag = "*",
                Consolidated = false,
                PartitionKey = "TIME",
                RowKey = Guid.NewGuid().ToString(),
                EntryOrExit = "Entrada",
                EmployedId = 99
            };
        }

        

        public static DefaultHttpRequest CreateHttpRequest(Guid timeId, EntryTime timeRequest)
        {
            string request = JsonConvert.SerializeObject(timeRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{timeId}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(EntryTime timeRequest)
        {
            string request = JsonConvert.SerializeObject(timeRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request)
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid timeId)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{timeId}"
            };
        }
        public static DefaultHttpRequest CreateHttpRequest()
        {

            return new DefaultHttpRequest(new DefaultHttpContext());
        }

        public static EntryTime GetTimeRequest()
        {
            return new EntryTime
            {
                DateEmployed = DateTime.UtcNow,
                Consolidated = false,
                EntryOrExit = "Entrada",
                EmployedId = 99
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



        

        public static ConsolidatedEntity GetConsolidatedEntity()
        {
            return new ConsolidatedEntity
            {
                DateEmployed = DateTime.UtcNow,
                ETag = "*",
                PartitionKey = "CONSOLIDATED",
                RowKey = Guid.NewGuid().ToString(),
                EmployedId = 80,
                Minutes = 30
            };
        }

        public static Consolidated GetConsolidatedRequest()
        {
            return new Consolidated
            {
                DateEmployed = DateTime.UtcNow,
                Minutes = 30,
                EmployedId = 99
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

        public static DefaultHttpRequest CreateHttpRequest(Guid consolidatedId, Consolidated consolidatedRequest)
        {
            string request = JsonConvert.SerializeObject(consolidatedRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{consolidatedId}"
            };
        }
    }
}
