using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ChamsICSLib.Data;

namespace ChamsICSWebService.Model
{
    public class DashboardRes: Response
    {
        public int TotalClient { get; set; }
        public int TotalAgent { get; set; }
        public int TotalTerminal { get; set; }
        public int TotalTransaction { get; set; }
        public decimal TransctionValue { get; set; }
        public int TotalNotifications { get; set; }
    }

    public class DashboardReq
    {
        public int RoleId { get; set; }
        public int UserId { get; set; }
    }
}
