//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace B2B_SCM.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class MS_BP_CP
    {
        public long Uid { get; set; }
        public string VendorCode { get; set; }
        public string PICName { get; set; }
        public string PICTitle { get; set; }
        public string PICEmail { get; set; }
        public string PICPhone { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdDate { get; set; }
        public string UpdBy { get; set; }
    }
}
