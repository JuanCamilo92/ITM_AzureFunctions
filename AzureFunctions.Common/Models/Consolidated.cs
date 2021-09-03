using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Common.Models
{
    public class Consolidated
    {
        public int EmployedId { get; set; }
        public DateTime DateEmployed { get; set; }
        public double Minutes { get; set; }
    }
}
