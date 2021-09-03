using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Functions.Entities
{
    public class TimeEntity : TableEntity
    {
        public int EmployedId { get; set; }
        public DateTime DateEmployed { get; set; }
        public string EntryOrExit { get; set; }
        public bool Consolidated { get; set; }

    }
}
