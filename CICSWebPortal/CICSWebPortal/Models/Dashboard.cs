using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace CICSWebPortal.Models
{
    public class Dashboard
    {
        public int TotalClient { get; set; }
        public int TotalAgent { get; set; }
        public int TotalTerminal { get; set; }
        public int TotalTransaction { get; set; }
        public decimal TransctionValue { get; set; }
        public int TotalNotifications { get; set; }

        public Chart Chart { get; set; }
    }
}