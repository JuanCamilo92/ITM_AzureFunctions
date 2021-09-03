using AzureFunctions.Functions.Functions;
using AzureFunctions.Test.Helpers;
using System;
using Xunit;

namespace AzureFunctions.Test.Test
{
    public class ScheduledFunctionTest
    {
        [Fact]
        public void ScheduledFunction_Should_Log_Message()
        {
            //Arrange
            MockCloudTableTimes mockTimes = new MockCloudTableTimes(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            MockCloudTableConsolidated mockConsolidated = new MockCloudTableConsolidated(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            ListLogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);
            //Act
            ScheduledFunction.Consolidated(null, mockTimes, mockConsolidated, logger);
            string message = logger.Logs[0];
            //Assert
            Assert.Contains("Deleting completed", message);
        }
    }
}
