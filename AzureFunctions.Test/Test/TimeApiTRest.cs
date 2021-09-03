using AzureFunctions.Common.Models;
using AzureFunctions.Functions.Functions;
using AzureFunctions.Test.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AzureFunctions.Test.Test
{
    public class TimeApiTRest
    {
        public readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void CreateTime_Should_Return_200()
        {
            //Arrange
            MockCloudTableTimes mockTimes = new MockCloudTableTimes(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            EntryTime timeRequest = TestFactory.GetTimeRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(timeRequest);

            //Act
            IActionResult response = await TimeApi.CreateTime(request, mockTimes, logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void UpdateTime_Should_Return_200()
        {
            //Arrange
            MockCloudTableTimes mockTimes = new MockCloudTableTimes(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            EntryTime timeRequest = TestFactory.GetTimeRequest();
            Guid timeId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(timeId, timeRequest);

            //Act
            IActionResult response = await TimeApi.UpdateTime(request, mockTimes, timeId.ToString(), logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void UpdateConsolidated_Should_Return_200()
        {
            //Arrange
            MockCloudTableConsolidated mockConsolidated = new MockCloudTableConsolidated(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Consolidated consolidatedRequest = TestFactory.GetConsolidatedRequest();
            Guid consolidatedId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(consolidatedId, consolidatedRequest);

            //Act
            IActionResult response = await TimeApi.UpdateTime(request, mockConsolidated, consolidatedId.ToString(), logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
    }
}
