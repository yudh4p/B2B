using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace B2B_SCM.Models
{
    public partial class Custom
    {
        public string PortID { get; set; }
        public string PortName { get; set; }
        public string username { get; set; }
        public string usernameU { get; set; }
        public string password { get; set; }
        public string ModeTransaction { get; set; }
        public string ModeTransactionDoc { get; set; }
        public string VendorCode { get; set; }
        public string au_sellercode { get; set; }
        public string au_shippercode { get; set; }
        public string au_sellercodeE { get; set; }
        public string au_shippercodeE { get; set; }
        public SelectList au_sellercode_list { get; set; }
        public SelectList au_shippercode_list { get; set; }
        public string VendorName { get; set; }
        public string VendorType { get; set; }
        public string Address { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool Status { get; set; }
        public SelectList CountryList { get; set; }
        public SelectList PortList { get; set; }
        public SelectList BPList { get; set; }

        public SelectList BPList_Shipper { get; set; }

        public SelectList BPList_Seller { get; set; }

        public SelectList WTList { get; set; }

        public string UID { get; set; }
        public string UID_Chat { get; set; }
        public string PICName { get; set; }
        public string PICTitle { get; set; }
        public string PICEmail { get; set; }
        public string PICPhone { get; set; }

        public string Currency { get; set; }
        public string CurrencyE { get; set; }
        public SelectList CurrencyList { get; set; }
        public SelectList CourierList { get; set; }

        public string GUID { get; set; }
        public string GUID2 { get; set; }
        public string GUID3 { get; set; }
        public string GUID4 { get; set; }
        public string GUID5 { get; set; }


        public string PurchaseNo { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }


        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateFrom { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateTo { get; set; }


        public decimal TradeQty { get; set; }
        public decimal UnitPrice { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ShipmentFrom { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ShipmentTo { get; set; }
        public string Origin { get; set; }
        public string Shipper { get; set; }
        public string Seller { get; set; }
        public string Remark { get; set; }
        public string RequiredDoc { get; set; }
        public string DocTypeRev { get; set; }

        public List<Custom_List_Doc_Type> Custom_List_Doc_Type = new List<Custom_List_Doc_Type>();
        public List<Custom_List_Doc_Type> Custom_List_Doc_Type2 = new List<Custom_List_Doc_Type>();
        public List<Custom_List_Doc_Type> Custom_List_Doc_Type3 = new List<Custom_List_Doc_Type>();
        public List<Custom_List_Doc_Type> Custom_List_Doc_Upload = new List<Custom_List_Doc_Type>();



        public long PurchaseIDE { get; set; }
        public long PurchaseIDE2 { get; set; }
        public long PurchaseIDE3 { get; set; }
        public string PurchaseNoE { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateE { get; set; }

        public decimal TradeQtyE { get; set; }
        public decimal UnitPriceE { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ShipmentFromE { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ShipmentToE { get; set; }
        public string OriginE { get; set; }
        public string ShipperE { get; set; }
        public string SellerE { get; set; }
        public string RemarkE { get; set; }
        public string RequiredDocE { get; set; }

        public List<Custom_List_Doc_Type> Custom_List_Doc_TypeE = new List<Custom_List_Doc_Type>();
        public List<Custom_List_Doc_Type> Custom_List_Doc_Type2E = new List<Custom_List_Doc_Type>();
        public List<Custom_List_Doc_Type> Custom_List_Doc_Type3E = new List<Custom_List_Doc_Type>();

        public List<Custom_List_Chat> Custom_List_Chat = new List<Custom_List_Chat>();
        public List<Custom_List_Chat> Custom_List_Chat_Date = new List<Custom_List_Chat>();
        public List<Custom_List_Notif> Custom_List_Notif = new List<Custom_List_Notif>();

        public string WheatCode { get; set; }
        public string WheatQty { get; set; }
        public string WheatPort { get; set; }

        public string WheatCodeE { get; set; }
        public string WheatQtyE { get; set; }
        public string WheatPortE { get; set; }



        public string VesselName { get; set; }
        public string VesselDetail { get; set; }
        public string DemDes { get; set; }
        public decimal DemRate { get; set; }
        public decimal DesRate { get; set; }
        public decimal DemRate2 { get; set; }
        public decimal DesRate2 { get; set; }
        public string LoadPort { get; set; }
        public string DischargePort { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ETA { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ETD { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ETB { get; set; }

        public string AppointAgent { get; set; }
        public string AWB { get; set; }
        public string RemarkAWB { get; set; }
        public string Courier { get; set; }

        public string ContractNo { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ContractDate { get; set; }
        public string ContractDateString { get; set; }



        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CommDiscDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CompDiscDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CommDiscDateH { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CompDiscDateH { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime PayDueDate { get; set; }



        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime LTCDueDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime LTCPaymentDueDate { get; set; }

        public Nullable<System.DateTime> ETADisc { get; set; }
        public Nullable<System.DateTime> ETBDisc { get; set; }
        public Nullable<System.DateTime> ETCDisc { get; set; }
        public string UID_TrxDtl { get; set; }
        public Nullable<System.DateTime> ETALoad { get; set; }
        public Nullable<System.DateTime> ETBLoad { get; set; }
        public Nullable<System.DateTime> ETDLoad { get; set; }
        public string FinalQty { get; set; }
        public string StatusDesDem { get; set; }
        public string StatusDesDemConfirm { get; set; }
        public string FinalUnitPrice { get; set; }
        public string FinalUnitPriceE { get; set; }
        public string PaymentDueDate { get; set; }
        public string PaymentDate { get; set; }
        public string UID_AWB { get; set; }
        public string FinalAmount { get; set; }


        public string Usr { get; set; }
        public string Pwd { get; set; }
        public string Fn { get; set; }
        public string AccountType { get; set; }
        public bool Locked { get; set; }
        public bool Active { get; set; }
        public bool Admin { get; set; }


        public string UsrE { get; set; }
        public string PwdE { get; set; }
        public string FnE { get; set; }
        public string AccountTypeE { get; set; }
        public string EmailE { get; set; }
        public string PhoneE { get; set; }
        public bool LockedE { get; set; }
        public bool ActiveE { get; set; }
        public bool AdminE { get; set; }





        public string PurchaseNoV { get; set; }
        public string TradeQtyV { get; set; }
        public string UnitPriceV { get; set; }
        public string CurrencyV { get; set; }
        public string ShipmentFromV { get; set; }
        public string ShipmentToV { get; set; }
        public string OriginV { get; set; }
        public string ShipperV { get; set; }
        public string SellerV { get; set; }
        public string ContractNoV { get; set; }
        public string ContractDateStringV { get; set; }
        public string RemarkV { get; set; }
        public string CommDiscDateV { get; set; }
        public string CompDiscDateV { get; set; }
        public string StatusDesDemV { get; set; }
        public string StatusDesDemConfirmV { get; set; }
        public string FinalAmountV { get; set; }
        public string PaymentDueDateV { get; set; }
        public string PaymentDateV { get; set; }
        public string VesselNameV { get; set; }
        public string VesselDetailV { get; set; }
        public string DemDesV { get; set; }
        public string AppointAgentV { get; set; }
        public string PurchaseIDEV { get; set; }
        public string GUIDV { get; set; }

        public string LTCDueDateV { get; set; }
        public string LTCPaymentDueDateV { get; set; }
        public string LTCDueDateH { get; set; }
        public string LTCPaymentDueDateH { get; set; }



        public string LTCDueDate1 { get; set; }

        public string LTCPaymentDueDate1 { get; set; }


        public string LTCDueDate2 { get; set; }

        public string LTCPaymentDueDate2 { get; set; }

        public string AWBCtgryE { get; set; }
        public string AWBCtgryV { get; set; }
    }

    public partial class Custom2
    {
        public List<Custom_List_Notif> Custom_List_Notif = new List<Custom_List_Notif>();
        public int Total_Notif { get; set; }
    }

    public class Custom_List_Doc_Type
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Required { get; set; }
        public bool Secure { get; set; }
        public string ContractNo { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ContractDate { get; set; }
        public string ContractDateString { get; set; }
        public string IsUpload { get; set; }


    }

    public class Custom_List_Chat
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string From { get; set; }
        public string Chat { get; set; }
        public string Odd { get; set; }
        public string Chat_ID { get; set; }
    }


    public class Custom_List_Notif
    {
        public string Message { get; set; }
        public string Date { get; set; }
    }

    public static class OData
    {
        public class OutboundWT
        {
            public OutboundWTD d { get; set; }
        }

        public class OutboundWTD
        {
            public List<OutboundWTResults> results { get; set; }
        }

        public class OutboundWTResults
        {
            public string Matnr { get; set; }
            public string Maktx { get; set; }
        
        }
    }
}