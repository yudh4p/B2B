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
    
    public partial class TRX_DOC
    {
        public long Uid { get; set; }
        public long PurchaseNo2 { get; set; }
        public byte[] AttachmentFile { get; set; }
        public string AttachmentName { get; set; }
        public string AttachmentType { get; set; }
        public Nullable<int> AttachmentStatus { get; set; }
        public string Remark { get; set; }
        public Nullable<int> DocType { get; set; }
        public string DocPassword { get; set; }
        public string CreatedUser { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<bool> Process { get; set; }
        public Nullable<System.DateTime> UpdDate { get; set; }
        public string UpdBy { get; set; }
        public string Feedback { get; set; }
        public string ContractNo { get; set; }
        public Nullable<System.DateTime> ContractDate { get; set; }
        public string Guid { get; set; }
        public Nullable<long> RefId { get; set; }
    }
}
