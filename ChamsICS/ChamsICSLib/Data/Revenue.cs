//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ChamsICSLib.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class Revenue
    {
        public int Id { get; set; }
        public Nullable<int> ClientId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string MDA { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<int> Status { get; set; }
    }
}
