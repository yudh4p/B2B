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
using System.Data.Entity;

namespace B2B_SCM.Controllers
{
    public class SellerController : Controller
    {
        // GET: Seller
        [Route("seller/index")]
        public ActionResult Index_Seller()
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");

            if (Session["ACCOUNTTYPE"].ToString() == "SCM")
                return RedirectToAction("index", "home");
            else if (Session["ACCOUNTTYPE"].ToString() == "SHIPPER")
                return RedirectToAction("index_shipper", "shipper");
            else if (Session["ACCOUNTTYPE"].ToString() == "FINANCE")
                return RedirectToAction("index_finance", "home");

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
            model.CourierList = Helper.SelectLists.CourierList();
            return View(model);
        }

        [HttpGet]
        public ActionResult load_data_index(string fromDate, string toDate)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                string USERNAME = Session["USERNAME"].ToString();
                string vendorCode = Session["VENDORCODE"].ToString();
                DataTable dt = Helper.LoadQuery("GET_INDEX_SELLER_SHIPPER '" + vendorCode + "','" + fromDate + "','" + toDate + "'");

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
        public ActionResult get_header_trx(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                string USERNAME = Session["USERNAME"].ToString();
                string PurchaseNo = "", TradeQty = "", UnitPrice = "", ShipmentFrom = "", ShipmentTo = "", Origin = "", TradeQty_C = "", UnitPrice_C = "", ContractNo = "", ContractDate = "",
                    Shipper = "", Seller = "", Remark = "", DocRequired = "", PurchaseIDE = "", Origin_Name = "", Shipper_Name = "", Seller_Name = "", 
                    FinalUnitPrice = "", PaymentDueDate = "", PaymentDate = "", Currency = "";

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
                    Origin_Name = Helper.LoadQuery("GET_ORIGIN_SPLIT '" + dt.Rows[0]["PurchaseNo2"].ToString() + "'").Rows[0][0].ToString(); //dt.Rows[0]["Origin_Name"].ToString();
                    Shipper_Name = dt.Rows[0]["Shipper_Name"].ToString();
                    Seller_Name = dt.Rows[0]["Seller_Name"].ToString();
                    TradeQty_C = dt.Rows[0]["TradeQty_C"].ToString();
                    UnitPrice_C = dt.Rows[0]["UnitPrice_C"].ToString();
                    FinalUnitPrice = dt.Rows[0]["FinalUnitPrice"].ToString();
                    PaymentDueDate = dt.Rows[0]["PaymentDueDate"].ToString();
                    PaymentDate = dt.Rows[0]["PaymentDate"].ToString();
                    Currency = dt.Rows[0]["Currency"].ToString();
                }

                DataTable dt2 = Helper.LoadQuery("GET_CONTRACT_INFO '" + PurchaseIDE + "', '" + accType + "'");
                if (dt2 != null && dt2.Rows.Count > 0)
                {
                    ContractNo = dt2.Rows[0]["CONTRACTNO"].ToString();
                    ContractDate = dt2.Rows[0]["CONTRACTDATE"].ToString();
                }


                string CommDiscDate = "", CompDiscDate = "", Confirm = "", ConfirmBy = "", ConfirmDate = "", StatusDesDem = "", LTCDueDate = "", LTCPaymentDueDate = "";
                DataTable dt3 = Helper.LoadQuery("GET_LTCP '" + id + "'");
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
                    FinalUnitPrice = FinalUnitPrice,
                    PaymentDueDate = PaymentDueDate.Replace(" 00:00:00", ""),
                    PaymentDate = PaymentDate.Replace(" 00:00:00", ""),
                    Currency = Currency,
                    LTCDueDate = LTCDueDate,
                    LTCPaymentDueDate = LTCPaymentDueDate,
                    ContractNo = ContractNo,
                    ContractDate = ContractDate,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
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
        public ActionResult _partial_upload_doc(string id = "")
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");

            string accType = Session["ACCOUNTTYPE"].ToString();
            Custom model = new Custom();
            model.Custom_List_Doc_Upload = Helper.SelectLists.DocTypeUpload(id: id, accountType: accType);

            return PartialView("_partial_upload_doc", model);
        }

        public ActionResult download(long Id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");

            B2BSCMDb db = new B2BSCMDb();
            var result = db.TRX_DOC.FirstOrDefault(x => x.Uid == Id && x.Status == true);
            if (result != null)
            {
                return File(result.AttachmentFile, result.AttachmentType, result.AttachmentName);
            }
            else
            {
                return Json(new { Success = false, Message = "File not found!" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult delete_doc(string Id, string purchaseno)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                if (Helper.IsClosed(purchaseno))
                    return Json(new { Success = false, Message = Helper.Msg }, JsonRequestBehavior.AllowGet);

                string username = Session["USERNAME"].ToString();
                Helper.ExecuteQuery("UPDATE_TRX_DOC  '" + Id + "','0','','" + username + "'");
                return Json(new { Success = true, Message = "Document successfully deleted!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Message = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult update_demdes(Models.Custom model, FormCollection frm)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                if (Helper.IsClosed(model.PurchaseIDE3.ToString()))
                    return Json(new { Success = false, Message = Helper.Msg }, JsonRequestBehavior.AllowGet);

                string username = Session["USERNAME"].ToString();

                string dem_or_des = frm["rbDemDes"];
                B2BSCMDb db = new B2BSCMDb();
                var result = db.TRX_SHP.FirstOrDefault(x => x.PurchaseNo2 == model.PurchaseIDE3);
                if (result != null)
                {
                    result.DemRate = 0;
                    result.DesRate = 0;
                    if (dem_or_des == "DEMURAGE")
                        result.DemRate = 1;
                    else
                        result.DesRate = 1;
                    result.UpdDate = DateTime.Now;
                    result.UpdBy = username;

                    db.Entry(result).State = EntityState.Modified;
                    db.SaveChanges();
                }
                
                TempData["Message"] = "Transaction successfully updated!";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error: " + e.Message;
            }
            return RedirectToAction("Index_Seller");
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

        [HttpPost]
        public ActionResult update_fp(string Id, string price)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                if (Helper.IsClosed(Id))
                    return Json(new { Success = false, Message = Helper.Msg }, JsonRequestBehavior.AllowGet);
                string username = Session["USERNAME"].ToString();
                Helper.ExecuteQuery("UPDATE_FINAL_PRICE  '" + Id + "','"+ price + "','" + username + "'");
                return Json(new { Success = true, Message = "Final Price successfully saved!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Message = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult get_item(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string accType = Session["ACCOUNTTYPE"].ToString();
                string username = Session["USERNAME"].ToString();
                string UID = "", QTY = "", ETALOAD = "", ETBLOAD = "", ETDLOAD = "", WHEATNAME = "", FINALQTY = "", LOADPORT = "", ETADISC = "", CURR = "", FINALPRICE = "";

                DataTable dt = Helper.LoadQuery("GET_ITEM '" + id + "', '" + accType + "', '" + username + "'");
                if (dt != null && dt.Rows.Count > 0)
                {
                    UID = dt.Rows[0]["UID"].ToString();
                    WHEATNAME = dt.Rows[0]["WHEATNAME"].ToString();
                    QTY = dt.Rows[0]["QTY"].ToString();
                    FINALQTY = dt.Rows[0]["FINALQTY"].ToString();
                    ETALOAD = dt.Rows[0]["ETALOAD"].ToString();
                    ETBLOAD = dt.Rows[0]["ETBLOAD"].ToString();
                    ETDLOAD = dt.Rows[0]["ETDLOAD"].ToString();
                    LOADPORT = dt.Rows[0]["LOADPORT"].ToString();
                    ETADISC = dt.Rows[0]["ETADISC"].ToString();
                    CURR = dt.Rows[0]["CURRENCY"].ToString();
                    FINALPRICE = dt.Rows[0]["FINALPRICE"].ToString();
                }


                return Json(new
                {
                    success = true,
                    UID = UID,
                    WHEATNAME = WHEATNAME,
                    LOADPORT = LOADPORT,
                    QTY = QTY,
                    FINALQTY = FINALQTY,
                    ETALOAD = ETALOAD.Replace(" 00:00:00", ""),
                    ETBLOAD = ETBLOAD.Replace(" 00:00:00", ""),
                    ETDLOAD = ETDLOAD.Replace(" 00:00:00", ""),
                    ETADISC = ETADISC.Replace(" 00:00:00", ""),
                    CURR = CURR,
                    FINALPRICE = FINALPRICE,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult submit_awb(FormCollection frm)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string PurchaseNo = frm["PurchaseNo"];
                string Courier = frm["Courier"];
                string AWB = frm["AWB"];
                string Remark = frm["Remark"];
                string Uid = frm["Uid"];
                string username = Session["USERNAME"].ToString();
                string DocType = frm["DocType"];

                Helper.ExecuteQuery("INSERT_AWB '" + PurchaseNo + "','" + AWB + "','" + Courier + "','" + Remark + "','" + username + "','" + Uid + "','" + DocType + "'");

                return Json(new { Success = true, Message = string.IsNullOrWhiteSpace(Uid) ? "Data successfully insert!" : "Data successfully update!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}