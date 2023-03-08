using ChamsICSLib;
using ChamsICSLib.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChamsICSWebService
{
    public class DataHelper
    {
        public static CICSEntities db = DbInstance.Db();        
    }
}