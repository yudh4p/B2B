using B2B_SCM.Models;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;

namespace B2B_SCM.Controllers
{
    public class FinanceController : Controller
    {
        [Route("finance/index")]
        public ActionResult Index_Finance()
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");

            if (Session["ACCOUNTTYPE"].ToString() == "SCM")
                return RedirectToAction("index", "home");
            else if (Session["ACCOUNTTYPE"].ToString() == "SELLER")
                return RedirectToAction("index_seller", "seller");
            else if (Session["ACCOUNTTYPE"].ToString() == "SHIPPER")
                return RedirectToAction("index_shipper", "shipper");

            Custom model = new Custom();
            model.CountryList = Helper.SelectLists.CountryList();
            model.WTList = Helper.SelectLists.WTList();
            model.BPList_Seller = Helper.SelectLists.BPListByType(vendorType: "SELLER");
            model.BPList_Shipper = Helper.SelectLists.BPListByType(vendorType: "SHIPPER");
            model.Custom_List_Doc_Type = Helper.SelectLists.DocType(1, 9);
            model.Custom_List_Doc_Type2 = Helper.SelectLists.DocType(10, 18);
            model.Custom_List_Doc_Type3 = Helper.SelectLists.DocType(19, 100);
            model.PortList = Helper.SelectLists.PortList();
            model.DateFrom = DateTime.Now.AddMonths(-1);
            model.DateTo = DateTime.Now;
            return View(model);
        }


        [HttpGet]
        public ActionResult load_data_index(string fromDate, string toDate)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                DataTable dt = Helper.LoadQuery("GET_INDEX '" + fromDate + "','" + toDate + "'");
                DataRow[] rows = dt.Select();
                for (int i = 0; i < rows.Length; i++)
                {
                    rows[i]["ORIGIN_NAME"] = Helper.LoadQuery("GET_ORIGIN_SPLIT '" + rows[i]["PURCHASENO2"].ToString() + "'").Rows[0][0].ToString();
                }
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return null;
            }
        }

        [HttpGet]
        public ActionResult load_modal_data_doc(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                string USERNAME = Session["USERNAME"].ToString();
                string accType = Session["ACCOUNTTYPE"].ToString();
                DataTable dt = Helper.LoadQuery("GET_HEADER_TRX_DOC '" + id + "','" + accType + "','" + USERNAME + "'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return null;
            }
        }

        [HttpGet]
        public ActionResult load_modal_data_item(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                string USERNAME = Session["USERNAME"].ToString();
                DataTable dt = Helper.LoadQuery("GET_HEADER_TRX_ITEM '" + id + "'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return null;
            }
        }

        [HttpGet]
        public ActionResult load_modal_history_doc(string pno, string type, string id, string guid)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                string USERNAME = Session["USERNAME"].ToString();
                DataTable dt = Helper.LoadQuery("GET_HEADER_TRX_DOC_HISTORY '" + pno + "','" + type + "','" + id + "','" + guid + "'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return null;
            }
        }

        [HttpGet]
        public ActionResult get_header_trx(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                string USERNAME = Session["USERNAME"].ToString();
                string PurchaseNo = "", TradeQty = "", UnitPrice = "", ShipmentFrom = "", ShipmentTo = "", Origin = "", TradeQty_C = "", UnitPrice_C = "",
                    Shipper = "", Seller = "", Remark = "", DocRequired = "", PurchaseIDE = "", Origin_Name = "", Shipper_Name = "", Seller_Name = "", Currency = "", 
                    Guid = "", FinalUnitPrice = "", PaymentDueDate = "", PaymentDate = "", FinalAmount = "", FinalAmount_C = "";

                string VendorCode = Session["VENDORCODE"].ToString();
                string accType = Session["ACCOUNTTYPE"].ToString();
                DataTable dt = Helper.LoadQuery("GET_HEADER_TRX '" + id + "', '" + accType + "', '" + VendorCode + "'");
                if (dt != null && dt.Rows.Count > 0)
                {
                    PurchaseIDE = dt.Rows[0]["PurchaseNo2"].ToString();
                    PurchaseNo = dt.Rows[0]["PurchaseNo1"].ToString() + "." + dt.Rows[0]["PurchaseNo2_C"].ToString();
                    TradeQty = dt.Rows[0]["TradeQty"].ToString();
                    UnitPrice = dt.Rows[0]["UnitPrice"].ToString();
                    ShipmentFrom = dt.Rows[0]["ShipmentFrom"].ToString();
                    ShipmentTo = dt.Rows[0]["ShipmentTo"].ToString();
                    Origin = dt.Rows[0]["Origin"].ToString();
                    Shipper = dt.Rows[0]["Shipper"].ToString();
                    Seller = dt.Rows[0]["Seller"].ToString();
                    Remark = dt.Rows[0]["Remark"].ToString();
                    DocRequired = dt.Rows[0]["DocRequired"].ToString();
                    Origin_Name = Helper.LoadQuery("GET_ORIGIN_SPLIT '" + dt.Rows[0]["PurchaseNo2"].ToString() + "'").Rows[0][0].ToString();
                    Shipper_Name = dt.Rows[0]["Shipper_Name"].ToString();
                    Seller_Name = dt.Rows[0]["Seller_Name"].ToString();
                    TradeQty_C = dt.Rows[0]["TradeQty_C"].ToString();
                    UnitPrice_C = dt.Rows[0]["UnitPrice_C"].ToString();
                    Currency = dt.Rows[0]["Currency"].ToString();
                    Guid = id;
                    FinalUnitPrice = dt.Rows[0]["FinalUnitPrice"].ToString();
                    PaymentDueDate = dt.Rows[0]["PaymentDueDate"].ToString();
                    PaymentDate = dt.Rows[0]["PaymentDate"].ToString();
                    FinalAmount = dt.Rows[0]["FinalAmount"].ToString();
                    FinalAmount_C = dt.Rows[0]["FinalAmount_C"].ToString();
                }

                string CommDiscDate = "", CompDiscDate = "", Confirm = "", ConfirmBy = "", ConfirmDate = "", StatusDesDem = "", LTCDueDate = "", LTCPaymentDueDate = "";
                DataTable dt3 = Helper.LoadQuery("GET_LTCP '" + PurchaseIDE + "'");
                if (dt3 != null && dt3.Rows.Count > 0)
                {
                    CommDiscDate = dt3.Rows[0]["LTCCommDiscDate"].ToString();
                    CompDiscDate = dt3.Rows[0]["LTCCompDiscDate"].ToString();
                    Confirm = dt3.Rows[0]["LTCConfirm"].ToString();
                    ConfirmBy = dt3.Rows[0]["LTCConfirmBy"].ToString();
                    ConfirmDate = dt3.Rows[0]["LTCConfirmDate"].ToString();
                    StatusDesDem = dt3.Rows[0]["STATUSDESDEM"].ToString();
                    LTCDueDate = dt3.Rows[0]["LTCDueDate"].ToString();
                    LTCPaymentDueDate = dt3.Rows[0]["LTCPaymentDueDate"].ToString();
                }

                return Json(new
                {
                    success = true,
                    PurchaseIDE = PurchaseIDE,
                    PurchaseNo = PurchaseNo,
                    TradeQty = TradeQty,
                    UnitPrice = UnitPrice,
                    ShipmentFrom = ShipmentFrom,
                    ShipmentTo = ShipmentTo,
                    Origin = Origin,
                    Shipper = Shipper,
                    Seller = Seller,
                    Remark = Remark,
                    DocRequired = DocRequired,
                    Origin_Name = Origin_Name,
                    Shipper_Name = Shipper_Name,
                    Seller_Name = Seller_Name,
                    TradeQty_C = TradeQty_C,
                    UnitPrice_C = UnitPrice_C,
                    CommDiscDate = CommDiscDate.Replace(" 00:00:00", ""),
                    CompDiscDate = CompDiscDate.Replace(" 00:00:00", ""),
                    StatusDesDemConfirm = Confirm,
                    ConfirmBy = ConfirmBy,
                    ConfirmDate = ConfirmDate,
                    StatusDesDem = StatusDesDem,
                    Currency = Currency,
                    Guid = Guid,
                    FinalUnitPrice = FinalUnitPrice,
                    PaymentDueDate = PaymentDueDate.Replace(" 00:00:00", ""),
                    PaymentDate = PaymentDate.Replace(" 00:00:00", ""),
                    FinalAmount = FinalAmount,
                    FinalAmount_C = FinalAmount_C,
                    LTCDueDate = LTCDueDate,
                    LTCPaymentDueDate = LTCPaymentDueDate,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}