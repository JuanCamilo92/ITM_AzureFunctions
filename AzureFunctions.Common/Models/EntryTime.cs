using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Common.Models
{
    public class EntryTime
    {
        public int EmployedId { get; set; }
        public DateTime DateEmployed { get; set; }
        public string EntryOrExit { get; set; }
        public bool Consolidated { get; set; }
    }
}
