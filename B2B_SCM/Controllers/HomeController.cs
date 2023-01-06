using B2B_SCM.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace B2B_SCM.Controllers
{
    public class HomeController : Controller
    {
        [Route("login")]
        public ActionResult Login()
        {
            if (Session["USERNAME"] != null)
            {
                string accType = Session["ACCOUNTTYPE"].ToString();
                if (accType == "SELLER")
                    return RedirectToAction("index_seller", "seller");
                if (accType == "SHIPPER")
                    return RedirectToAction("index_shipper", "shipper");
                if (accType == "FINANCE")
                    return RedirectToAction("index_finance", "finance");
                if (accType == "SCM")
                    return RedirectToAction("index", "home");
            }
            Custom model = new Custom();
            model.password = "";
            return View(model);
        }

        public ActionResult logout()
        {
            Session.Clear();
            Session.RemoveAll();
            return RedirectToAction("login");
        }

        [HttpPost]
        public ActionResult Login(Custom field)
        {
            try
            {
                B2BSCMDb db = new B2BSCMDb();
                var check_1 = db.MS_USER.FirstOrDefault(x => x.Usr == field.username);
                if (check_1 != null)
                {

                    string password = WMS.Library.Security.EncryptData("Q!W@E#R$", field.password);
                    if (check_1.Pwd != password)
                    {
                        int failed = 0;
                        if (check_1.Failed == null)
                            failed = 0;
                        else
                            failed = (int)check_1.Failed;
                        failed++;
                        check_1.Failed = failed;
                        if (check_1.Failed == 3)
                            check_1.Locked = true;

                        db.Entry(check_1).State = EntityState.Modified;
                        db.SaveChanges();

                        TempData["Message"] = check_1.Failed < 3 ? "Error: The username or password not match!" : "Error: Your account has been locked due to multiple failed login attempts!";
                        return RedirectToAction("login");
                    }

                    if (check_1.Locked == true)
                    {
                        TempData["Message"] = "Error: Your account has been locked due to multiple failed login attempts!";
                        return RedirectToAction("login");
                    }

                    if (check_1.Active == false)
                    {
                        TempData["Message"] = "Error: The account is not active!";
                        return RedirectToAction("login");
                    }
                    Session["USERNAME"] = field.username;
                    Session["FULLNAME"] = check_1.Fn;
                    Session["ACCOUNTTYPE"] = check_1.AccountType;
                    Session["ADMIN"] = check_1.Admin;
                    Session["VENDORCODE"] = check_1.VendorCode == null ? "" : check_1.VendorCode;
                    Session["ACC_INFO"] = "(" + field.username + ") " + check_1.Fn;

                    check_1.Failed = 0;

                    db.Entry(check_1).State = EntityState.Modified;
                    db.SaveChanges();

                    if (check_1.AccountType == "SELLER")
                    {
                        var vndor = db.MS_BP.FirstOrDefault(x => x.VendorCode == check_1.VendorCode);
                        Session["ACC_INFO"] = "(" + field.username + ") " + vndor.VendorName;
                        return RedirectToAction("index_seller", "seller");

                    }
                    else if (check_1.AccountType == "SHIPPER")
                    {
                        var vndor = db.MS_BP.FirstOrDefault(x => x.VendorCode == check_1.VendorCode);
                        Session["ACC_INFO"] = "(" + field.username + ") " + vndor.VendorName;
                        return RedirectToAction("index_shipper", "shipper");

                    }

                    else if (check_1.AccountType == "SCM")
                        return RedirectToAction("index", "home");
                    else if (check_1.AccountType == "FINANCE")
                        return RedirectToAction("index_finance", "finance");
                    else
                        return RedirectToAction("login", "home");
                }
                else
                {
                    TempData["Message"] = "Error: The username or password not match!";
                    return RedirectToAction("login");
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error: " + ex.Message;
                return RedirectToAction("login");
            }
        }

        public ActionResult unlock()
        {
            return View();
        }


        public ActionResult Index()
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");


            if (Session["ACCOUNTTYPE"].ToString() == "SELLER")
                return RedirectToAction("index_seller", "seller");
            else if (Session["ACCOUNTTYPE"].ToString() == "SHIPPER")
                return RedirectToAction("index_shipper", "shipper");
            else if (Session["ACCOUNTTYPE"].ToString() == "FINANCE")
                return RedirectToAction("index_finance", "home");

            Custom model = new Custom();
            model.CountryList = Helper.SelectLists.CountryList(withEmptyValue: true);
            model.WTList = Helper.SelectLists.WTList();
            model.BPList_Seller = Helper.SelectLists.BPListByType(withEmptyValue: true, vendorType: "SELLER");
            model.BPList_Shipper = Helper.SelectLists.BPListByType(withEmptyValue: true, vendorType: "SHIPPER");
            model.PortList = Helper.SelectLists.PortList();
            model.CurrencyList = Helper.SelectLists.CurrencyList();
            //model.Custom_List_Doc_Type = Helper.SelectLists.DocType(1, 9);
            //model.Custom_List_Doc_Type2 = Helper.SelectLists.DocType(10, 18);
            //model.Custom_List_Doc_Type3 = Helper.SelectLists.DocType(19, 25);
            model.PortList = Helper.SelectLists.PortList(withEmptyValue: true);

            model.CommDiscDate = DateTime.Now;
            model.CompDiscDate = DateTime.Now;
            model.PayDueDate = DateTime.Now;
            model.LTCDueDate = DateTime.Now;
            model.LTCPaymentDueDate = DateTime.Now;

            model.DateFrom = DateTime.Now.AddMonths(-1);
            model.DateTo = DateTime.Now;

            return View(model);
        }


        [HttpPost]
        public ActionResult submit_new_trx(Models.Custom model, FormCollection frm)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();

                string origin = frm["Origin"];

                B2BSCMDb db = new B2BSCMDb();
                TRX data = new TRX();
                data.PurchaseNo1 = "P" + DateTime.Now.ToString("yy");
                data.Date = DateTime.Now;
                data.TradeQty = model.TradeQty;
                data.UnitPrice = model.UnitPrice;
                data.ShipmentFrom = model.ShipmentFrom;
                data.ShipmentTo = model.ShipmentTo;
                data.Origin = origin;
                data.Shipper = model.Shipper;
                data.Seller = model.Seller;
                data.Remark = model.Remark;
                data.CreatedBy = username;
                data.CreatedDate = DateTime.Now;
                data.Currency = model.Currency;
                data.Guid = Guid.NewGuid();

                string required = "";
                model.Custom_List_Doc_Type = Helper.SelectLists.DocType();
                foreach (var itm in model.Custom_List_Doc_Type)
                {
                    if (itm.Required)
                    {
                        required += itm.Id.ToString() + ",";
                    }
                    else
                    {
                        bool isChecked = frm["txtChk" + itm.Id.ToString()] != null ? true : false;
                        if (isChecked)
                        {
                            required += itm.Id.ToString() + ",";
                        }
                    }
                }
                data.RequiredDoc = required.Remove(required.Length - 1, 1);
                data.Status = true;

                db.TRX.Add(data);
                db.SaveChanges();

                Helper.ExecuteQuery("INSERT_TRX_DOC_PIVOT '" + data.PurchaseNo2 + "'");

                string purchaseno2 = "";
                if (data.PurchaseNo2 < 10)
                    purchaseno2 = "00" + data.PurchaseNo2.ToString();
                else if (data.PurchaseNo2 > 9 && data.PurchaseNo2 < 100)
                    purchaseno2 = "0" + data.PurchaseNo2.ToString();
                else if (data.PurchaseNo2 > 99 && data.PurchaseNo2 < 1000)
                    purchaseno2 = data.PurchaseNo2.ToString();
                else
                    purchaseno2 = data.PurchaseNo2.ToString();

                string new_line = "<br/>";
                string message = "Dear Mr/Mrs," + new_line + new_line;
                message += "You have a new transaction " + data.PurchaseNo1.ToString() + "." + purchaseno2 + " for wheat purchase. Details: " + new_line;
                message += "<table><tr><td>Shipment Periode</td><td>From " + data.ShipmentFrom.Value.ToString("dd/MM/yyyy") + " To " + data.ShipmentTo.Value.ToString("dd/MM/yyyy") + "</td></tr><tr><td>Wheat & Qty</td><td>Waiting update from SCM</td></tr></table>" + new_line + new_line;
                message += "Thanks," + new_line;
                message += "This is an automated message, please do not reply" + new_line;

                Helper.SendEmail(Helper.LoadQuery("GET_EMAIL_SELLER_SHIPPER '" + data.Seller + "','" + data.Shipper + "'").Rows[0][0].ToString(), "NEW PURCHASE NUMBER " + data.PurchaseNo1.ToString() + "." + purchaseno2, message);
                TempData["Message"] = "Transaction successfully created!";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error: " + e.Message;
            }
            return RedirectToAction("index");
        }

        [HttpPost]
        public ActionResult submit_edit_trx(Models.Custom model, FormCollection frm)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                string origin = frm["OriginE"];

                if (Helper.IsClosed(model.PurchaseIDE.ToString()))
                {
                    TempData["Message"] = "Error: " + Helper.Msg;
                    return RedirectToAction("index");
                }

                B2BSCMDb db = new B2BSCMDb();
                TRX data = db.TRX.FirstOrDefault(x => x.PurchaseNo2 == model.PurchaseIDE);
                data.TradeQty = model.TradeQtyE;
                data.UnitPrice = model.UnitPriceE;
                data.ShipmentFrom = model.ShipmentFromE;
                data.ShipmentTo = model.ShipmentToE;
                data.Origin = origin;
                data.Shipper = model.ShipperE;
                data.Seller = model.SellerE;
                data.Remark = model.RemarkE;
                data.UpdBy = username;
                data.UpdDate = DateTime.Now;
                data.Currency = model.CurrencyE;

                string required = "";
                model.Custom_List_Doc_Type = Helper.SelectLists.DocType();
                foreach (var itm in model.Custom_List_Doc_Type)
                {
                    if (itm.Required)
                    {
                        required += itm.Id.ToString() + ",";
                    }
                    else
                    {
                        bool isChecked = frm["txtChkE" + itm.Id.ToString()] != null ? true : false;
                        if (isChecked)
                        {
                            required += itm.Id.ToString() + ",";
                        }
                    }
                }
                data.RequiredDoc = required.Remove(required.Length - 1, 1);

                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Message"] = "Transaction successfully updated!";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error: " + e.Message;
            }
            return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult load_data_index(string fromDate, string toDate)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                if (fromDate == null)
                    fromDate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
                if (toDate == null)
                    toDate = DateTime.Now.ToString("yyyy-MM-dd");
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
        public ActionResult get_header_trx(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string accType = Session["ACCOUNTTYPE"].ToString();
                string username = Session["USERNAME"].ToString();
                string PurchaseNo = "", TradeQty = "", UnitPrice = "", ShipmentFrom = "", ShipmentTo = "", Origin = "", TradeQty_C = "", UnitPrice_C = "",
                    Shipper = "", Seller = "", Remark = "", DocRequired = "", PurchaseIDE = "", ContractNo = "", ContractDate = "", StatusDesDem = "", Closed = "",
                    Currency = "", Guid = "", FinalUnitPrice = "", PaymentDueDate = "", PaymentDate = "", FinalAmount = "", FinalAmount_C = "", Seller_Name = "", Shipper_Name = "";

                DataTable dt = Helper.LoadQuery("GET_HEADER_TRX '" + id + "', '" + accType + "', '" + username + "'");
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
                    TradeQty_C = dt.Rows[0]["TradeQty_C"].ToString();
                    UnitPrice_C = dt.Rows[0]["UnitPrice_C"].ToString();
                    Currency = dt.Rows[0]["Currency"].ToString();
                    Guid = id;
                    FinalUnitPrice = dt.Rows[0]["FinalUnitPrice"].ToString();
                    PaymentDueDate = dt.Rows[0]["PaymentDueDate"].ToString();
                    PaymentDate = dt.Rows[0]["PaymentDate"].ToString();
                    Closed = Convert.ToBoolean(dt.Rows[0]["Closed"]) ? "1" : "0";
                    FinalAmount = dt.Rows[0]["FinalAmount"].ToString();
                    FinalAmount_C = dt.Rows[0]["FinalAmount_C"].ToString();
                }

                DataTable dt2 = Helper.LoadQuery("GET_CONTRACT_INFO '" + PurchaseIDE + "', '" + accType + "'");
                if (dt2 != null && dt2.Rows.Count > 0)
                {
                    ContractNo = dt2.Rows[0]["CONTRACTNO"].ToString();
                    ContractDate = dt2.Rows[0]["CONTRACTDATE"].ToString();
                }

                string CommDiscDate = "", CompDiscDate = "", Confirm = "", ConfirmBy = "", ConfirmDate = "", LTCDueDate = "", LTCPaymentDueDate = "";
                DataTable dt3 = Helper.LoadQuery("GET_LTCP '" + PurchaseIDE + "'");
                if (dt3 != null && dt3.Rows.Count > 0)
                {
                    CommDiscDate = dt3.Rows[0]["LTCCommDiscDate"].ToString();
                    CompDiscDate = dt3.Rows[0]["LTCCompDiscDate"].ToString();
                    //PaymentDueDate = dt3.Rows[0]["PaymentDueDate"].ToString();
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
                    TradeQty_C = TradeQty_C,
                    UnitPrice_C = UnitPrice_C,
                    ContractNo = ContractNo,
                    ContractDate = ContractDate,
                    CommDiscDate = CommDiscDate.Replace(" 00:00:00", ""),
                    CompDiscDate = CompDiscDate.Replace(" 00:00:00", ""),
                    StatusDesDemConfirm = Confirm,
                    ConfirmBy = ConfirmBy,
                    ConfirmDate = ConfirmDate,
                    StatusDesDem = StatusDesDem,
                    FinalAmount = FinalAmount,
                    Currency = Currency,
                    Guid = Guid,
                    FinalUnitPrice = FinalUnitPrice,
                    PaymentDueDate = PaymentDueDate.Replace(" 00:00:00", ""),
                    PaymentDate = PaymentDate.Replace(" 00:00:00", ""),
                    Closed = Closed,
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

        [HttpGet]
        public ActionResult get_preview(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string accType = Session["ACCOUNTTYPE"].ToString();
                string username = Session["USERNAME"].ToString();
                string PurchaseNo = "", TradeQty = "", UnitPrice = "", ShipmentFrom = "", ShipmentTo = "", Origin = "", TradeQty_C = "", UnitPrice_C = "",
                    Shipper = "", Seller = "", Remark = "", DocRequired = "", PurchaseIDE = "", ContractNo = "", ContractDate = "", StatusDesDem = "", Closed = "",
                    Currency = "", Guid = "", FinalUnitPrice = "", PaymentDueDate = "", PaymentDate = "", FinalAmount = "", FinalAmount_C = "", Seller_Name = "",
                    Shipper_Name = "", Shipment_Period = "", Wheat = "", Origin_Name = "";

                DataTable dt = Helper.LoadQuery("GET_HEADER_TRX_PREVIEW '" + id + "', '" + accType + "', '" + username + "'");
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
                    TradeQty_C = dt.Rows[0]["TradeQty_C"].ToString();
                    UnitPrice_C = dt.Rows[0]["UnitPrice_C"].ToString();
                    Currency = dt.Rows[0]["Currency"].ToString();
                    Guid = id;
                    FinalUnitPrice = dt.Rows[0]["FinalUnitPrice"].ToString();
                    PaymentDueDate = dt.Rows[0]["PaymentDueDate"].ToString();
                    PaymentDate = dt.Rows[0]["PaymentDate"].ToString();
                    Closed = Convert.ToBoolean(dt.Rows[0]["Closed"]) ? "1" : "0";
                    FinalAmount = dt.Rows[0]["FinalAmount"].ToString();
                    FinalAmount_C = dt.Rows[0]["FinalAmount_C"].ToString();
                    Seller_Name = dt.Rows[0]["Seller_Name"].ToString();
                    Shipper_Name = dt.Rows[0]["Shipper_Name"].ToString();
                    Shipment_Period = dt.Rows[0]["SHIPMENT_PERIODE"].ToString();
                    Wheat = dt.Rows[0]["WHEAT"].ToString();
                    Origin_Name = dt.Rows[0]["ORIGIN_NAME"].ToString();
                }

                DataTable dt2 = Helper.LoadQuery("GET_CONTRACT_INFO '" + PurchaseIDE + "', '" + accType + "'");
                if (dt2 != null && dt2.Rows.Count > 0)
                {
                    ContractNo = dt2.Rows[0]["CONTRACTNO"].ToString();
                    ContractDate = dt2.Rows[0]["CONTRACTDATE"].ToString();
                }

                string CommDiscDate = "", LTCDueDate = "", LTCPaymentDueDate = "", CompDiscDate = "", Confirm = "", ConfirmBy = "", ConfirmDate = "";
                DataTable dt3 = Helper.LoadQuery("GET_LTCP '" + PurchaseIDE + "'");
                if (dt3 != null && dt3.Rows.Count > 0)
                {
                    CommDiscDate = dt3.Rows[0]["LTCCommDiscDate"].ToString();
                    CompDiscDate = dt3.Rows[0]["LTCCompDiscDate"].ToString();
                    //PaymentDueDate = dt3.Rows[0]["PaymentDueDate"].ToString();
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
                    TradeQty_C = TradeQty_C,
                    UnitPrice_C = UnitPrice_C,
                    ContractNo = ContractNo,
                    ContractDate = ContractDate,
                    CommDiscDate = CommDiscDate.Replace(" 00:00:00", ""),
                    CompDiscDate = CompDiscDate.Replace(" 00:00:00", ""),
                    StatusDesDemConfirm = Confirm,
                    ConfirmBy = ConfirmBy,
                    ConfirmDate = ConfirmDate,
                    StatusDesDem = StatusDesDem,
                    FinalAmount = FinalAmount,
                    Currency = Currency,
                    Guid = Guid,
                    FinalUnitPrice = FinalUnitPrice,
                    PaymentDueDate = PaymentDueDate.Replace(" 00:00:00", ""),
                    PaymentDate = PaymentDate.Replace(" 00:00:00", ""),
                    Closed = Closed,
                    FinalAmount_C = FinalAmount_C,
                    Seller_Name = Seller_Name,
                    Shipper_Name = Shipper_Name,
                    Shipment_Period = Shipment_Period,
                    Wheat = Wheat,
                    Origin_Name = Origin_Name,
                    LTCDueDate = LTCDueDate,
                    LTCPaymentDueDate= LTCPaymentDueDate,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
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
                string UID = "", WHEATCODE = "", DISCPORT = "", QTY = "", ETADISC = "", ETBDISC = "", ETCDISC = "";

                DataTable dt = Helper.LoadQuery("GET_ITEM '" + id + "', '" + accType + "', '" + username + "'");
                if (dt != null && dt.Rows.Count > 0)
                {
                    UID = dt.Rows[0]["UID"].ToString();
                    WHEATCODE = dt.Rows[0]["WHEATCODE"].ToString();
                    DISCPORT = dt.Rows[0]["DISCPORT"].ToString();
                    QTY = dt.Rows[0]["QTY"].ToString();
                    ETADISC = dt.Rows[0]["ETADISC"].ToString();
                    ETBDISC = dt.Rows[0]["ETBDISC"].ToString();
                    ETCDISC = dt.Rows[0]["ETCDISC"].ToString();
                }


                return Json(new
                {
                    success = true,
                    UID = UID,
                    WHEATCODE = WHEATCODE,
                    DISCPORT = DISCPORT,
                    QTY = QTY,
                    ETADISC = ETADISC.Replace(" 00:00:00", ""),
                    ETBDISC = ETBDISC.Replace(" 00:00:00", ""),
                    ETCDISC = ETCDISC.Replace(" 00:00:00", ""),
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
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
                //TempData["Message"] = ex.Message;
                //return null;
                return Json(new { data = "failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult load_modal_data_doc_contract(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                string USERNAME = Session["USERNAME"].ToString();
                string accType = Session["ACCOUNTTYPE"].ToString();
                DataTable dt = Helper.LoadQuery("EXEC GET_HEADER_TRX_DOC_CONTRACT '" + id + "','" + accType + "','" + USERNAME + "'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //TempData["Message"] = ex.Message;
                //return null;
                return Json(new { data = "failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult load_modal_data_doc_instruction(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                string USERNAME = Session["USERNAME"].ToString();
                string accType = Session["ACCOUNTTYPE"].ToString();
                DataTable dt = Helper.LoadQuery("EXEC GET_HEADER_TRX_DOC_INSTRUCTION '" + id + "','" + accType + "','" + USERNAME + "'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //TempData["Message"] = ex.Message;
                //return null;
                return Json(new { data = "failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult load_modal_data_doc_ltc(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                string USERNAME = Session["USERNAME"].ToString();
                string accType = Session["ACCOUNTTYPE"].ToString();
                DataTable dt = Helper.LoadQuery("EXEC GET_HEADER_TRX_DOC_LTC '" + id + "','" + accType + "','" + USERNAME + "'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //TempData["Message"] = ex.Message;
                //return null;
                return Json(new { data = "failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult load_modal_data_doc_payment(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                string USERNAME = Session["USERNAME"].ToString();
                string accType = Session["ACCOUNTTYPE"].ToString();
                DataTable dt = Helper.LoadQuery("EXEC GET_HEADER_TRX_DOC_PAYMENT '" + id + "','" + accType + "','" + USERNAME + "'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //TempData["Message"] = ex.Message;
                //return null;
                return Json(new { data = "failed" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Get_ContractInfo(string id)
        {
            try
            {
                string ContractNo = "", ContractDate = "";
                string accType = Session["ACCOUNTTYPE"].ToString();

                DataTable dt2 = Helper.LoadQuery("GET_CONTRACT_INFO '" + id + "', '" + accType + "'");
                if (dt2 != null && dt2.Rows.Count > 0)
                {
                    ContractNo = dt2.Rows[0]["CONTRACTNO"].ToString();
                    ContractDate = dt2.Rows[0]["CONTRACTDATE"].ToString();
                }

                return Json(new
                {
                    success = true,
                    ContractNo = ContractNo,
                    ContractDate = ContractDate,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult load_modal_data_doc_cancel(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                string USERNAME = Session["USERNAME"].ToString();
                string accType = Session["ACCOUNTTYPE"].ToString();
                DataTable dt = Helper.LoadQuery("GET_HEADER_TRX_DOC_CANCEL '" + id + "','" + accType + "','" + USERNAME + "'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //TempData["Message"] = ex.Message;
                //return null;
                return Json(new { data = "failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult load_modal_data_doc_received(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                string USERNAME = Session["USERNAME"].ToString();
                string accType = Session["ACCOUNTTYPE"].ToString();
                DataTable dt = Helper.LoadQuery("GET_HEADER_TRX_DOC_RECEIVED '" + id + "','" + accType + "','" + USERNAME + "'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //TempData["Message"] = ex.Message;
                //return null;
                return Json(new { data = "failed" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult get_pwd_doc(string id, string pwd)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                B2BSCMDb db = new B2BSCMDb();
                string username = Session["USERNAME"].ToString();
                string decrypt_pwd = WMS.Library.Security.EncryptData("Q!W@E#R$", pwd);
                var check_1 = db.MS_USER.Count(x => x.Usr == username && x.Pwd == decrypt_pwd);
                if (check_1 == 0)
                {
                    return Json(new { Success = false, Message = "Password Incorrect" }, JsonRequestBehavior.AllowGet);
                }


                string USERNAME = Session["USERNAME"].ToString();
                string accType = Session["ACCOUNTTYPE"].ToString();
                DataTable dt = Helper.LoadQuery("GET_PWD_DOC '" + id + "'");

                string pwd_result = "";
                try
                {
                    pwd_result = WMS.Library.Security.DecryptData(dt.Rows[0][0].ToString(), "Q!W@E#R$");
                }
                catch
                {
                    pwd_result = dt.Rows[0][0].ToString();
                }

                return Json(new { Success = true, Message = "Password : " + pwd_result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Error : " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult delete_doc(string Id, string purchaseno)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                if (Helper.IsClosed(purchaseno))
                    return Json(new { Success = false, Message = Helper.Msg }, JsonRequestBehavior.AllowGet);

                string username = Session["USERNAME"].ToString();
                Helper.ExecuteQuery("DELETE_DOC '" + Id + "','','" + username + "'");
                return Json(new { Success = true, Message = "Document successfully deleted!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Message = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
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
        public ActionResult load_modal_awb(string pno)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                string USERNAME = Session["USERNAME"].ToString();
                DataTable dt = Helper.LoadQuery("GET_LIST_AWB '" + pno + "','GENERAL'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return null;
            }
        }

        [HttpGet]
        public ActionResult load_modal_awb_contract(string pno)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                string USERNAME = Session["USERNAME"].ToString();
                DataTable dt = Helper.LoadQuery("GET_LIST_AWB '" + pno + "','CONTRACT'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return null;
            }
        }

        [HttpGet]
        public ActionResult load_modal_awb2(string pno)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                string USERNAME = Session["USERNAME"].ToString();
                DataTable dt = Helper.LoadQuery("GET_LIST_AWB_SELLER '" + pno + "','" + USERNAME + "', 'GENERAL'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return null;
            }
        }
        [HttpGet]
        public ActionResult get_detail_doc(string uid)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                string UID = "", NAME = "", ATTACHMENTNAME = "", VENDORNAME = "",
                    STATUS = "", ACCOUNTTYPE = "";

                DataTable dt = Helper.LoadQuery("GET_DETAIL_DOC '" + uid + "'");
                if (dt != null && dt.Rows.Count > 0)
                {
                    UID = dt.Rows[0]["UID"].ToString();
                    NAME = dt.Rows[0]["NAME"].ToString();
                    ATTACHMENTNAME = dt.Rows[0]["ATTACHMENTNAME"].ToString();
                    VENDORNAME = dt.Rows[0]["VENDORNAME"].ToString();
                    STATUS = dt.Rows[0]["STATUS"].ToString();
                    ACCOUNTTYPE = dt.Rows[0]["ACCOUNTTYPE"].ToString();
                }

                return Json(new
                {
                    success = true,
                    UID = UID,
                    NAME = NAME,
                    ATTACHMENTNAME = ATTACHMENTNAME,
                    VENDORNAME = VENDORNAME,
                    STATUS = STATUS,
                    ACCOUNTTYPE = ACCOUNTTYPE
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, Message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult load_modal_data_item(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
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
        public ActionResult load_table_doc_controlling(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                DataTable dt = Helper.LoadQuery("TRACKER_DOCUMENT '" + id + "'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return null;
            }
        }

        [HttpPost]
        public ActionResult update_status_doc(string uid, string type, string remark)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                string STATUS = "";
                STATUS = type == "APPROVED" ? "3" : "2";


                if (Helper.IsClosedbyUid(uid))
                    return Json(new { Success = false, Message = Helper.Msg }, JsonRequestBehavior.AllowGet);

                long _uid = Convert.ToInt64(uid);
                B2BSCMDb db = new B2BSCMDb();
                var result = db.TRX_DOC.FirstOrDefault(x => x.Uid == _uid);

                if (result.DocType == 1 || result.DocType == 2 || result.DocType == 3 || result.DocType == 4)
                {
                    STATUS = type == "APPROVED" ? "6" : "2";
                }

                if (!Helper.ExecuteQuery("UPDATE_TRX_DOC '" + uid + "','" + STATUS + "','" + remark + "','" + username + "'"))
                {
                    return Json(new { Success = false, Message = "Error: " + Helper.Msg }, JsonRequestBehavior.AllowGet);
                }


                #region SEND EMAIL
                DataTable dt_seller_shipper = Helper.LoadQuery("select seller,shipper from trx where PurchaseNo2 = '" + result.PurchaseNo2.ToString() + "'");
                DataTable dt_document_name = Helper.LoadQuery("select Name from MS_DOCTYPE where Id = '" + result.DocType + "'");
                string seller_ = dt_seller_shipper.Rows[0][0].ToString();
                string shipper_ = dt_seller_shipper.Rows[0][1].ToString();

                string purchaseno_format = Helper.Get_Format_PurchaseNo(result.PurchaseNo2.ToString());

                string new_line = "<br/>";
                string message = "Dear Mr/Mrs," + new_line + new_line;
                message += "Draft \"" + dt_document_name.Rows[0][0].ToString().ToUpper() + "\" (" + result.AttachmentName + ") - " + purchaseno_format + (type == "APPROVED" ? " is confirmed." : " is need to be revised.") + new_line + new_line;
                message += "Thanks," + new_line;
                message += "This is an automated message, please do not reply" + new_line;

                Helper.SendEmail(Helper.LoadQuery("GET_EMAIL_SELLER_SHIPPER '" + seller_ + "','" + shipper_ + "'").Rows[0][0].ToString(), "DRAFT \"" + dt_document_name.Rows[0][0].ToString() + "\" " + (type == "APPROVED" ? "CONFIRMATION" : "NEED REV") + " - " + purchaseno_format, message);
                #endregion

                return Json(new { Success = true, Message = "Document successfully " + type, PurchaseNo = result.PurchaseNo2.ToString() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult submit_new_trx_item(long id, string wheat, string port, decimal qty)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                if (Helper.IsClosed(id.ToString()))
                    return Json(new { Success = false, Message = Helper.Msg }, JsonRequestBehavior.AllowGet);
                string username = Session["USERNAME"].ToString();

                string wheatCode = wheat.Split('-')[0];

                B2BSCMDb db = new B2BSCMDb();

                DataTable dt_check = Helper.LoadQuery("CHECK_QTY_WHEN_ADD_ITEM '" + id + "','" + qty + "'");
                if (!string.IsNullOrWhiteSpace(dt_check.Rows[0][0].ToString()))
                {
                    return Json(new { Success = false, Message = "Error: " + dt_check.Rows[0][0].ToString() }, JsonRequestBehavior.AllowGet);
                }

                var check = db.TRX_DTL.FirstOrDefault(x => x.PurchaseNo2 == id && x.WheatCode == wheatCode && x.DiscPort == port && x.Status == true);
                if (check == null)
                {
                    TRX_DTL data = new TRX_DTL();
                    data.PurchaseNo2 = id;
                    data.WheatCode = wheatCode;
                    data.WheatName = wheat.Split('-')[1];
                    data.DiscPort = port;
                    data.Qty = qty;
                    data.CreatedBy = username;
                    data.CreatedDate = DateTime.Now;
                    data.Status = true;

                    db.TRX_DTL.Add(data);
                    db.SaveChanges();
                    return Json(new { Success = true, Message = "Item successfully saved" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    check.DiscPort = port;
                    check.Qty = qty;
                    check.UpdBy = username;
                    check.UpdDate = DateTime.Now;
                    db.Entry(check).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { Success = true, Message = "Item successfully updated" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Message = "Error: " + e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult active_deactive_trx(string id, string status)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();

                B2BSCMDb db = new B2BSCMDb();

                var check = db.TRX.FirstOrDefault(x => x.Guid.ToString() == id);
                if (check != null)
                {
                    check.UpdBy = username;
                    check.UpdDate = DateTime.Now;
                    check.Status = status == "0" ? false : true;
                    db.Entry(check).State = EntityState.Modified;
                    db.SaveChanges();
                }

                TempData["Message"] = "Data successfully restored!";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error: " + e.Message;
            }
            return RedirectToAction("index");
        }

        [HttpPost]
        public ActionResult submit_delete_trx_item(long id, string purchaseno)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                if (Helper.IsClosed(purchaseno))
                    return Json(new { Success = false, Message = Helper.Msg }, JsonRequestBehavior.AllowGet);

                string username = Session["USERNAME"].ToString();

                B2BSCMDb db = new B2BSCMDb();

                var check = db.TRX_DTL.FirstOrDefault(x => x.Uid == id);
                if (check != null)
                {
                    check.UpdBy = username;
                    check.UpdDate = DateTime.Now;
                    check.Status = false;
                    db.Entry(check).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return Json(new { Success = true, Message = "Data successfully delete!" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                return Json(new { Success = false, Message = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult _partial_doc_required(string mode = "N", string seller = "", string shipper = "", string pid = "")
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");

            string accType = Session["ACCOUNTTYPE"].ToString();
            Custom model = new Custom();
            model.ModeTransactionDoc = mode;
            model.Custom_List_Doc_Type = Helper.SelectLists.DocType2(seller, shipper, mode, pid);

            return PartialView("_partial_doc_required", model);
        }

        [HttpGet]
        public ActionResult _partial_doc_chat(string uid = "")
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");

            string username = Session["USERNAME"].ToString();
            Custom model = new Custom();
            model.UID_Chat = uid;
            model.Custom_List_Chat = Helper.SelectLists.ChatByGuid(uid, username);
            //model.Custom_List_Doc_Type = Helper.SelectLists.DocType2(seller, shipper, mode, pid);

            return PartialView("_partial_doc_chat", model);
        }


        [HttpGet]
        public ActionResult _partial_notif()
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");

            return PartialView("_partial_notif");
        }

        [HttpGet]
        public ActionResult _partial_notif_header()
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");

            string username = Session["USERNAME"].ToString();
            string acctype = Session["ACCOUNTTYPE"].ToString();

            Custom2 model = new Custom2();
            model.Custom_List_Notif = Helper.SelectLists.Notification(acctype, username);
            model.Total_Notif = Helper.SelectLists.Notification(acctype, username).Count();
            ViewBag.Total_Notif = model.Custom_List_Notif.Count;
            return PartialView("_partial_notif_header", model);
        }

        [HttpGet]
        public ActionResult _partial_notif_content()
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");

            string username = Session["USERNAME"].ToString();
            string acctype = Session["ACCOUNTTYPE"].ToString();

            Custom2 model = new Custom2();
            model.Custom_List_Notif = Helper.SelectLists.Notification(acctype, username);
            model.Total_Notif = model.Custom_List_Notif.Count();
            return PartialView("_partial_notif_content", model);
        }

        [HttpGet]
        public ActionResult _partial_upload_doc(string id = "")
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");

            string accType = Session["ACCOUNTTYPE"].ToString();
            Custom model = new Custom();
            model.ContractDate = DateTime.Now;
            model.Custom_List_Doc_Upload = Helper.SelectLists.DocTypeUpload(id: id, accountType: accType);

            return PartialView("_partial_upload_doc", model);
        }

        public ActionResult Check_Document(string purchaseno, string doctype)
        {
            string accType = Session["ACCOUNTTYPE"].ToString();
            DataTable dt_check_exists = Helper.LoadQuery("CHECK_DOC_EXISTS '" + doctype + "', '" + purchaseno + "', '" + accType + "'");
            if (Convert.ToInt32(dt_check_exists.Rows[0][0]) > 0)
            {
                return Json(new { Success = true, Status = Convert.ToInt32(dt_check_exists.Rows[0][0]), Message = "Document is already exists, Do you want still upload?" }, JsonRequestBehavior.AllowGet);
            }

            //switch (Convert.ToInt32(doctype))
            //{
            //    case 6:
            //    case 9:
            //    case 10:
            //    case 11:
            //    case 12:
            //    case 13:
            //    case 14:
            //    case 15:
            //    case 16:
            //    case 17:
            //    case 18:
            //    case 19:
            //    case 20:
            //    case 21:
            //    case 22:
            //    case 23:
            //    case 24:
            //    case 25:
            //        break;
            //}


            return Json(new { Success = true, Message = "Upload success!" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Upload(FormCollection frm)
        {
            try
            {
                string accType = Session["ACCOUNTTYPE"].ToString();
                int DocType = Convert.ToInt32(frm["DocType"]);
                long PurchaseNo = Convert.ToInt32(frm["PNO"]);
                string contractNo = frm["ContractNo"];
                string contractDate = frm["ContractDate"];


                if (Helper.IsClosed(PurchaseNo.ToString()))
                    return Json(new { Success = false, Message = Helper.Msg }, JsonRequestBehavior.AllowGet);

                B2BSCMDb db = new B2BSCMDb();
                HttpPostedFileBase postedFile = Request.Files["Image"];
                //HttpPostedFileBase[] postedFiles = (HttpPostedFileBase[])Request.F["Image"];
                if (postedFile == null)
                {
                    return Json(new { Success = false, Message = "Document is nothing, please Upload document first, and click submit" });
                }

                //DataTable dt_check_ = Helper.LoadQuery("VALIDASI_DOC_2 '" + DocType + "','" + PurchaseNo + "','" + accType + "'");
                //if (dt_check_ != null && dt_check_.Rows.Count > 0)
                //{
                //    if (dt_check_.Rows[0][0].ToString() == "1")
                //    {
                //        return Json(new { Success = false, Message = "Sales contract/Addendum contract must be completed first!" });
                //    }
                //}

                string uname = Session["USERNAME"].ToString();
                TRX_DOC doc = new TRX_DOC();
                doc.PurchaseNo2 = PurchaseNo;

                string fileInputPath = "";
                string fileOuputPath = "";

                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    string FileName = System.IO.Path.GetFileNameWithoutExtension(postedFile.FileName);
                    string FileExtn = System.IO.Path.GetExtension(postedFile.FileName);
                    if (FileExtn != ".pdf")
                    {
                        return Json(new { Success = false, Message = "Only pdf can upload!" });
                    }

                    if (DocType == 1)
                    {
                        if (string.IsNullOrWhiteSpace(contractNo))
                        {
                            return Json(new { Success = false, Message = "Contract No is mandatory!" });
                        }
                        if (string.IsNullOrWhiteSpace(contractDate))
                        {
                            return Json(new { Success = false, Message = "Contract Date is mandatory!" });
                        }
                        doc.ContractNo = contractNo;
                        DateTime dt_contract = new DateTime(Convert.ToInt32(contractDate.Split('-')[2]), Convert.ToInt32(contractDate.Split('-')[1]), Convert.ToInt32(contractDate.Split('-')[0]));
                        doc.ContractDate = dt_contract;

                        Helper.ExecuteQuery("EXEC UPDATE_CONTRACT '" + contractNo + "', '" + dt_contract.ToString("yyyy-MM-dd") + "', '" + PurchaseNo + "'");
                    }

                    //string password = WMS.Library.Security.EncryptData(Helper.RandomGenerator.RandomPassword().ToUpper(), "Q!W@E#R$");

                    DataTable dt_check = Helper.LoadQuery("VALIDASI_DOC '" + DocType + "'");
                    if (dt_check != null && dt_check.Rows.Count > 0)
                    {
                        if (Convert.ToBoolean(dt_check.Rows[0]["SECURE"]))
                        {
                            string fileName = doc.PurchaseNo2.ToString() + "_" + DateTime.Now.ToString("yyMMddHHmmss") + "_" + Path.GetFileName(postedFile.FileName);
                            fileInputPath = Path.Combine(Server.MapPath("~/upload"), fileName);
                            postedFile.SaveAs(fileInputPath);

                            string password = Helper.RandomGenerator.RandomPassword().ToUpper();

                            PdfDocument document = PdfReader.Open(fileInputPath, PdfDocumentOpenMode.Modify);
                            PdfSecuritySettings securitySettings = document.SecuritySettings;
                            securitySettings.UserPassword = password;
                            securitySettings.PermitAccessibilityExtractContent = false;
                            securitySettings.PermitAnnotations = false;
                            securitySettings.PermitAssembleDocument = false;
                            securitySettings.PermitExtractContent = false;
                            securitySettings.PermitFormsFill = true;
                            securitySettings.PermitFullQualityPrint = false;
                            securitySettings.PermitModifyDocument = true;
                            securitySettings.PermitPrint = false;

                            string fileNameSecure = doc.PurchaseNo2.ToString() + "_" + DateTime.Now.ToString("yyMMddHHmmss") + "_" + FileName + "_Secure" + FileExtn;
                            fileOuputPath = Path.Combine(Server.MapPath("~/upload"), fileNameSecure);
                            document.Save(fileOuputPath);
                            document.Close();

                            //FileStream stream = System.IO.File.OpenRead("@" + fileOuputPath);

                            byte[] bytes = System.IO.File.ReadAllBytes(fileOuputPath);

                            doc.AttachmentFile = bytes;
                            doc.DocPassword = WMS.Library.Security.EncryptData(password, "Q!W@E#R$");
                        }
                        else
                        {
                            BinaryReader Binary_Reader = new BinaryReader(postedFile.InputStream);
                            byte[] File_Buffer = Binary_Reader.ReadBytes(postedFile.ContentLength);

                            doc.AttachmentFile = File_Buffer;
                        }
                    }

                    doc.AttachmentName = Guid.NewGuid().ToString().Substring(0, 6).ToUpper() + "_" + postedFile.FileName;
                    doc.AttachmentType = postedFile.ContentType;

                    if (accType == "SELLER" || accType == "SHIPPER")
                    {
                        if (DocType == 28 || DocType == 29)
                            doc.AttachmentStatus = 4;
                        else if (DocType == 1 || DocType == 2 || DocType == 3 || DocType == 4)
                        {
                            DataTable dt_check_last_status = Helper.LoadQuery("select AttachmentStatus from TRX_DOC_HEADER where PurchaseNo2 = " + PurchaseNo + " and DocType IN (1,2,3,4)");
                            if (dt_check_last_status != null && dt_check_last_status.Rows.Count > 0)
                            {
                                if (dt_check_last_status.Rows[0][0].ToString() == "6")
                                    doc.AttachmentStatus = 7;
                            }
                            else
                            {
                                doc.AttachmentStatus = 1;
                            }
                        }
                        else
                            doc.AttachmentStatus = 1;
                    }
                    else if (accType == "SCM" || accType == "FINANCE")
                    {
                        if (DocType == 28 || DocType == 29 || DocType == 30)
                        {
                            doc.AttachmentStatus = 4;
                        }
                        else
                        {
                            doc.AttachmentStatus = DocType == 31 ? 1 : 4;
                        }
                    }

                }
                doc.Remark = frm["Remark"];
                doc.DocType = DocType;
                doc.CreatedUser = uname;
                doc.CreatedDate = DateTime.Now;
                doc.Status = true;
                doc.Guid = Guid.NewGuid().ToString();

                db.TRX_DOC.Add(doc);
                db.SaveChanges();

                Helper.ExecuteQuery("EXEC INSERT_TRX_DOC_HEADER " + PurchaseNo + ", '" + DocType + "','','" + doc.Uid + "','" + accType + "'");

                string PaymentDate = frm["PaymentDate"];
                if (DocType == 30)
                    Helper.ExecuteQuery("EXEC UPDATE_PAYMENT_DATE " + PurchaseNo + ", '" + PaymentDate + "','" + uname + "'");

                //Helper.ExecuteQuery("EXEC INSERT_TRX_DOC_CHAT " + doc.Uid + ", '" + doc.Remark + "', '" + uname + "'");

                string purchaseno_format = Helper.Get_Format_PurchaseNo(PurchaseNo.ToString());
                #region SEND EMAIL
                //document instruction
                if (accType == "SCM" || accType == "FINANCE")
                {
                    DataTable dt_document_name = Helper.LoadQuery("select Name from MS_DOCTYPE where Id = '" + DocType + "'");
                    DataTable dt_seller_shipper = Helper.LoadQuery("select seller,shipper from trx where PurchaseNo2 = '" + PurchaseNo.ToString() + "'");
                    string seller_ = dt_seller_shipper.Rows[0][0].ToString();
                    string shipper_ = dt_seller_shipper.Rows[0][1].ToString();


                    string new_line = "<br/>";
                    string message = "Dear Mr/Mrs," + new_line + new_line;
                    message += "Document \"" + dt_document_name.Rows[0][0].ToString().ToUpper() + "\" (" + doc.AttachmentName + ") " + purchaseno_format + " is available" + new_line + new_line;
                    message += "Thanks," + new_line;
                    message += "This is an automated message, please do not reply" + new_line;

                    Helper.SendEmail(Helper.LoadQuery("GET_EMAIL_SELLER_SHIPPER '" + seller_ + "','" + shipper_ + "'").Rows[0][0].ToString(), "DOCUMENT \"" + dt_document_name.Rows[0][0].ToString().ToUpper() + "\" - " + purchaseno_format, message);
                }
                else
                {
                    DataTable dt_document_name = Helper.LoadQuery("select Name from MS_DOCTYPE where Id = '" + DocType + "'");
                    //var check_ = db.TRX_DOC_HEADER.Where(x => x.DocType == DocType);

                    string new_line = "<br/>";
                    string message = "Dear Mr/Mrs," + new_line + new_line;
                    message += "Draft \"" + dt_document_name.Rows[0][0].ToString() + "\" " + purchaseno_format + " is available to be checked." + new_line + new_line;
                    message += "Thanks," + new_line;
                    message += "This is an automated message, please do not reply" + new_line;

                    Helper.SendEmail(Helper.LoadQuery("GET_EMAIL_SCM").Rows[0][0].ToString(), "DRAFT \"" + dt_document_name.Rows[0][0].ToString().ToUpper() + "\" - " + purchaseno_format, message);
                }
                #endregion

                try
                {
                    if (!string.IsNullOrWhiteSpace(fileInputPath))
                    {
                        if (System.IO.File.Exists(fileInputPath))
                        {
                            System.IO.File.Delete(Path.Combine(fileInputPath));
                            Console.WriteLine("File deleted.");
                        }
                    }
                }
                catch { }
                try
                {
                    if (!string.IsNullOrWhiteSpace(fileOuputPath))
                    {
                        if (System.IO.File.Exists(fileOuputPath))
                        {
                            System.IO.File.Delete(Path.Combine(fileOuputPath));
                            Console.WriteLine("File deleted.");
                        }
                    }

                }
                catch { }

                return Json(new { Success = true, Message = "Upload success!" });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Error: " + ex.Message });
            }

        }

        [HttpPost]
        public ActionResult UploadRev(FormCollection frm)
        {
            try
            {
                string accType = Session["ACCOUNTTYPE"].ToString();
                int DocType = Convert.ToInt32(frm["DocType"]);
                long PurchaseNo = Convert.ToInt32(frm["PurchaseNo"]);
                long UID = Convert.ToInt32(frm["UID"]);
                string GUID = frm["GUID"];
                string contractNo = frm["ContractInfo"];
                string contractDate = frm["ContractDate"];
                string Remark = frm["Remark"];

                if (Helper.IsClosed(PurchaseNo.ToString()))
                    return Json(new { Success = false, Message = Helper.Msg }, JsonRequestBehavior.AllowGet);

                B2BSCMDb db = new B2BSCMDb();
                HttpPostedFileBase postedFile = Request.Files["Image"];
                //HttpPostedFileBase[] postedFiles = (HttpPostedFileBase[])Request.F["Image"];
                if (postedFile == null)
                {
                    return Json(new { Success = false, Message = "Document is nothing, please Upload document first, and click submit" });
                }


                string uname = Session["USERNAME"].ToString();
                TRX_DOC doc = new TRX_DOC();
                doc.PurchaseNo2 = PurchaseNo;

                string fileInputPath = "";
                string fileOuputPath = "";

                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    string FileName = System.IO.Path.GetFileNameWithoutExtension(postedFile.FileName);
                    string FileExtn = System.IO.Path.GetExtension(postedFile.FileName);
                    if (FileExtn != ".pdf")
                    {
                        return Json(new { Success = false, Message = "Only pdf can upload!" });
                    }

                    if (DocType == 1)
                    {
                        if (string.IsNullOrWhiteSpace(contractNo))
                        {
                            return Json(new { Success = false, Message = "Contract No is mandatory!" });
                        }
                        if (string.IsNullOrWhiteSpace(contractDate))
                        {
                            return Json(new { Success = false, Message = "Contract Date is mandatory!" });
                        }
                        doc.ContractNo = contractNo;
                        DateTime dt_contract = new DateTime(Convert.ToInt32(contractDate.Split('-')[2]), Convert.ToInt32(contractDate.Split('-')[1]), Convert.ToInt32(contractDate.Split('-')[0]));
                        doc.ContractDate = dt_contract;

                        Helper.ExecuteQuery("EXEC UPDATE_CONTRACT '" + contractNo + "', '" + dt_contract.ToString("yyyy-MM-dd") + "', '" + PurchaseNo + "'");
                    }

                    //string password = WMS.Library.Security.EncryptData(Helper.RandomGenerator.RandomPassword().ToUpper(), "Q!W@E#R$");

                    DataTable dt_check = Helper.LoadQuery("VALIDASI_DOC '" + DocType + "'");
                    if (dt_check != null && dt_check.Rows.Count > 0)
                    {
                        if (Convert.ToBoolean(dt_check.Rows[0]["SECURE"]))
                        {
                            string fileName = doc.PurchaseNo2.ToString() + "_" + DateTime.Now.ToString("yyMMddHHmmss") + "_" + Path.GetFileName(postedFile.FileName);
                            fileInputPath = Path.Combine(Server.MapPath("~/upload"), fileName);
                            postedFile.SaveAs(fileInputPath);

                            string password = Helper.RandomGenerator.RandomPassword().ToUpper();

                            PdfDocument document = PdfReader.Open(fileInputPath, PdfDocumentOpenMode.Modify);
                            PdfSecuritySettings securitySettings = document.SecuritySettings;
                            securitySettings.UserPassword = password;
                            securitySettings.PermitAccessibilityExtractContent = false;
                            securitySettings.PermitAnnotations = false;
                            securitySettings.PermitAssembleDocument = false;
                            securitySettings.PermitExtractContent = false;
                            securitySettings.PermitFormsFill = true;
                            securitySettings.PermitFullQualityPrint = false;
                            securitySettings.PermitModifyDocument = true;
                            securitySettings.PermitPrint = false;

                            string fileNameSecure = doc.PurchaseNo2.ToString() + "_" + DateTime.Now.ToString("yyMMddHHmmss") + "_" + FileName + "_Secure" + FileExtn;
                            fileOuputPath = Path.Combine(Server.MapPath("~/upload"), fileNameSecure);
                            document.Save(fileOuputPath);
                            document.Close();

                            //FileStream stream = System.IO.File.OpenRead("@" + fileOuputPath);

                            byte[] bytes = System.IO.File.ReadAllBytes(fileOuputPath);

                            doc.AttachmentFile = bytes;
                            doc.DocPassword = WMS.Library.Security.EncryptData(password, "Q!W@E#R$");
                        }
                        else
                        {
                            BinaryReader Binary_Reader = new BinaryReader(postedFile.InputStream);
                            byte[] File_Buffer = Binary_Reader.ReadBytes(postedFile.ContentLength);

                            doc.AttachmentFile = File_Buffer;
                        }
                    }

                    doc.AttachmentName = Guid.NewGuid().ToString().Substring(0, 6).ToUpper() + "_" + postedFile.FileName;
                    doc.AttachmentType = postedFile.ContentType;
                    doc.Remark = Remark;

                    if (accType == "SELLER" || accType == "SHIPPER")
                    {
                        if (DocType == 28 || DocType == 29)
                            doc.AttachmentStatus = 4;
                        else
                            doc.AttachmentStatus = 1;
                    }
                    else if (accType == "SCM" || accType == "FINANCE")
                    {
                        if (DocType == 28 || DocType == 29 || DocType == 30)
                            doc.AttachmentStatus = 4;
                        else
                            doc.AttachmentStatus = DocType == 31 ? 1 : 4;
                    }

                }
                doc.Remark = frm["Remark"];
                doc.DocType = DocType;
                doc.CreatedUser = uname;
                doc.CreatedDate = DateTime.Now;
                doc.Status = true;
                doc.Guid = GUID;
                //doc.RefId = UID;

                db.TRX_DOC.Add(doc);
                db.SaveChanges();

                Helper.ExecuteQuery("EXEC UPDATE_TRX_DOC_HEADER '" + UID + "','" + doc.AttachmentName + "','" + doc.Uid + "', '" + GUID + "'");

                //string PaymentDate = frm["PaymentDate"];
                //if (DocType == 30)
                //    Helper.ExecuteQuery("EXEC UPDATE_PAYMENT_DATE " + PurchaseNo + ", '" + PaymentDate + "','" + uname + "'");

                //Helper.ExecuteQuery("EXEC INSERT_TRX_DOC_CHAT "+doc.Uid+", '"+ doc.Remark + "', '" + uname + "'");

                string purchaseno_format = Helper.Get_Format_PurchaseNo(PurchaseNo.ToString());
                //#region SEND EMAIL
                ////document instruction
                //if (accType == "SCM" || accType == "FINANCE")
                //{
                //    DataTable dt_document_name = Helper.LoadQuery("select Name from MS_DOCTYPE where Id = '" + DocType + "'");
                //    DataTable dt_seller_shipper = Helper.LoadQuery("select seller,shipper from trx where PurchaseNo2 = '" + PurchaseNo.ToString() + "'");
                //    string seller_ = dt_seller_shipper.Rows[0][0].ToString();
                //    string shipper_ = dt_seller_shipper.Rows[0][1].ToString();


                //    string new_line = "<br/>";
                //    string message = "Dear Mr/Mrs," + new_line + new_line;
                //    message += "Document \"" + dt_document_name.Rows[0][0].ToString().ToUpper() + "\" (" + doc.AttachmentName + ") " + purchaseno_format + " is available" + new_line + new_line;
                //    message += "Thanks," + new_line;
                //    message += "This is an automated message, please do not reply" + new_line;

                //    Helper.SendEmail(Helper.LoadQuery("GET_EMAIL_SELLER_SHIPPER '" + seller_ + "','" + shipper_ + "'").Rows[0][0].ToString(), "DOCUMENT \"" + dt_document_name.Rows[0][0].ToString().ToUpper() + "\" - " + purchaseno_format, message);
                //}
                //else
                //{
                //    DataTable dt_document_name = Helper.LoadQuery("select Name from MS_DOCTYPE where Id = '" + DocType + "'");
                //    //var check_ = db.TRX_DOC_HEADER.Where(x => x.DocType == DocType);

                //    string new_line = "<br/>";
                //    string message = "Dear Mr/Mrs," + new_line + new_line;
                //    message += "Draft \"" + dt_document_name.Rows[0][0].ToString() + "\" " + purchaseno_format + " is available to be checked." + new_line + new_line;
                //    message += "Thanks," + new_line;
                //    message += "This is an automated message, please do not reply" + new_line;

                //    Helper.SendEmail(Helper.LoadQuery("GET_EMAIL_SCM").Rows[0][0].ToString(), "DRAFT \"" + dt_document_name.Rows[0][0].ToString().ToUpper() + "\" - " + purchaseno_format, message);
                //}
                //#endregion

                try
                {
                    if (!string.IsNullOrWhiteSpace(fileInputPath))
                    {
                        if (System.IO.File.Exists(fileInputPath))
                        {
                            System.IO.File.Delete(Path.Combine(fileInputPath));
                            Console.WriteLine("File deleted.");
                        }
                    }
                }
                catch { }
                try
                {
                    if (!string.IsNullOrWhiteSpace(fileOuputPath))
                    {
                        if (System.IO.File.Exists(fileOuputPath))
                        {
                            System.IO.File.Delete(Path.Combine(fileOuputPath));
                            Console.WriteLine("File deleted.");
                        }
                    }

                }
                catch { }

                return Json(new { Success = true, Message = "Upload success!" });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Error: " + ex.Message });
            }

        }

        [HttpPost]
        public ActionResult insert_chat(string uid, string remark)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                if (!string.IsNullOrWhiteSpace(remark))
                {
                    if (!Helper.ExecuteQuery("INSERT_TRX_DOC_CHAT '" + uid + "','" + remark + "','" + username + "'"))
                    {
                        return Json(new { Success = false, Message = "Error: " + Helper.Msg }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { Success = true, Message = "" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Success = false, Message = "Chat is empty!" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult update_ltc(long PurchaseNo, string commDate, string compDate, string status, string fAmount)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                DateTime dt_comm = new DateTime();
                DateTime dt_comp = new DateTime();

                if (Helper.IsClosed(PurchaseNo.ToString()))
                    return Json(new { Success = false, Message = Helper.Msg }, JsonRequestBehavior.AllowGet);

                string username = Session["USERNAME"].ToString();
                if (!string.IsNullOrWhiteSpace(commDate))
                    dt_comm = new DateTime(Convert.ToInt32(commDate.Split('-')[2]), Convert.ToInt32(commDate.Split('-')[1]), Convert.ToInt32(commDate.Split('-')[0]));
                if (!string.IsNullOrWhiteSpace(compDate))
                    dt_comp = new DateTime(Convert.ToInt32(compDate.Split('-')[2]), Convert.ToInt32(compDate.Split('-')[1]), Convert.ToInt32(compDate.Split('-')[0]));
                //if (!string.IsNullOrWhiteSpace(dueDate))
                //    dt_duedate = new DateTime(Convert.ToInt32(dueDate.Split('-')[2]), Convert.ToInt32(dueDate.Split('-')[1]), Convert.ToInt32(dueDate.Split('-')[0]));

                Helper.ExecuteQuery("UPDATE_LTCP '" + PurchaseNo + "','" + (!string.IsNullOrWhiteSpace(commDate) ? dt_comm.ToString("yyyy-MM-dd") : "") + "','" + (!string.IsNullOrWhiteSpace(compDate) ? dt_comp.ToString("yyyy-MM-dd") : "") + "','" + username + "','" + status + "', '" + fAmount + "'");

                return Json(new { Success = true, Message = "Data successfully updated!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult update_item(long uid, string wheat, string port, string qty, string eta, string etb, string etc, string eta2 = "", string pno = "", string fp = "")
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string dtETA = "";
                string dtETB = "";
                string dtETC = "";
                string dtETA2 = "";
                string accType = Session["ACCOUNTTYPE"].ToString();
                string username = Session["USERNAME"].ToString();

                if (!string.IsNullOrWhiteSpace(eta) && eta != "01-01-1900")
                {
                    if (eta != "01-01-1900")
                    {
                        dtETA = eta.Split('-')[2] + "-" + eta.Split('-')[1] + "-" + eta.Split('-')[0];

                    }
                }

                if (!string.IsNullOrWhiteSpace(etb))
                {
                    if (etb != "01-01-1900")
                    {
                        dtETB = etb.Split('-')[2] + "-" + etb.Split('-')[1] + "-" + etb.Split('-')[0];

                    }
                }

                if (!string.IsNullOrWhiteSpace(etc))
                {
                    if (etc != "01-01-1900")
                    {
                        dtETC = etc.Split('-')[2] + "-" + etc.Split('-')[1] + "-" + etc.Split('-')[0];

                    }
                }

                if (!string.IsNullOrWhiteSpace(eta2))
                {
                    if (eta2 != "01-01-1900")
                    {
                        dtETA2 = eta2.Split('-')[2] + "-" + eta2.Split('-')[1] + "-" + eta2.Split('-')[0];
                    }
                }

                if (Helper.IsClosed(pno))
                    return Json(new { Success = false, Message = Helper.Msg }, JsonRequestBehavior.AllowGet);

                Helper.ExecuteQuery("UPDATE_ITEM '" + uid + "','" + accType + "','" + wheat.Split('-')[0] + "','" + wheat.Split('-')[1] + "'," +
                    "'" + port + "','" + qty + "','" + dtETA + "','" + dtETB + "','" + dtETC + "','" + username + "','" + dtETA2 + "','" + fp + "'");

                return Json(new { Success = true, Message = "Data successfully updated!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult update_dd(string Id, string date)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                if (Helper.IsClosed(Id))
                    return Json(new { Success = false, Message = Helper.Msg }, JsonRequestBehavior.AllowGet);

                DateTime dt = new DateTime();
                if (!string.IsNullOrWhiteSpace(date))
                    dt = new DateTime(Convert.ToInt32(date.Split('-')[2]), Convert.ToInt32(date.Split('-')[1]), Convert.ToInt32(date.Split('-')[0]));
                string username = Session["USERNAME"].ToString();
                Helper.ExecuteQuery("UPDATE_DUE_DATE  '" + Id + "','" + dt.ToString("yyyy-MM-dd") + "','" + username + "'");
                return Json(new { Success = true, Message = "Due date successfully saved!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Message = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult update_pd(string Id, string date)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                if (Helper.IsClosed(Id))
                    return Json(new { Success = false, Message = Helper.Msg }, JsonRequestBehavior.AllowGet);

                DateTime dt = new DateTime();
                if (!string.IsNullOrWhiteSpace(date))
                    dt = new DateTime(Convert.ToInt32(date.Split('-')[2]), Convert.ToInt32(date.Split('-')[1]), Convert.ToInt32(date.Split('-')[0]));
                string username = Session["USERNAME"].ToString();
                Helper.ExecuteQuery("UPDATE_PAYMENT_DATE  '" + Id + "','" + dt.ToString("yyyy-MM-dd") + "','" + username + "'");
                return Json(new { Success = true, Message = "Payment date successfully saved!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Message = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult open_trx(string Id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                //if (Helper.IsClosed(Id))
                //    return Json(new { Success = false, Message = Helper.Msg }, JsonRequestBehavior.AllowGet);
                //
                string username = Session["USERNAME"].ToString();
                Helper.ExecuteQuery("OPEN_TRX  '" + Id + "','" + username + "'");
                return Json(new { Success = true, Message = "Transaction successfully open!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Message = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult close_trx(string Id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                if (Helper.IsClosed(Id))
                    return Json(new { Success = false, Message = Helper.Msg }, JsonRequestBehavior.AllowGet);

                string username = Session["USERNAME"].ToString();
                Helper.ExecuteQuery("CLOSE_TRX  '" + Id + "','" + username + "'");
                return Json(new { Success = true, Message = "Transaction successfully close!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Message = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Report()
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");


            if (Session["ACCOUNTTYPE"].ToString() == "SELLER")
                return RedirectToAction("index_seller", "seller");
            else if (Session["ACCOUNTTYPE"].ToString() == "SHIPPER")
                return RedirectToAction("index_shipper", "shipper");
            else if (Session["ACCOUNTTYPE"].ToString() == "FINANCE")
                return RedirectToAction("index_finance", "home");

            Custom model = new Custom();

            model.DateFrom = DateTime.Now.AddMonths(-1);
            model.DateTo = DateTime.Now;

            return View(model);
        }

        [HttpGet]
        public ActionResult load_report(string fromDate, string toDate)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                if (fromDate == null)
                    fromDate = DateTime.Now.AddMonths(-1).ToString("dd-MM-yyyy");
                if (toDate == null)
                    toDate = DateTime.Now.ToString("dd-MM-yyyy");
                DataTable dt = Helper.LoadQuery("REPORTING '" + fromDate + "','" + toDate + "'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return null;
            }
        }

        [HttpGet]
        public ActionResult get_origin(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                string USERNAME = Session["USERNAME"].ToString();
                string ORIGIN = "";

                DataTable dt = Helper.LoadQuery("GET_ORIGIN_SPLIT '" + id + "'");
                if (dt != null && dt.Rows.Count > 0)
                {
                    ORIGIN = dt.Rows[0]["ORIGIN"].ToString();
                }

                return Json(new
                {
                    Success = true,
                    ORIGIN = ORIGIN,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, status = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Cancel_Approved(string Id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                //if (Helper.IsClosed(Id))
                //    return Json(new { Success = false, Message = Helper.Msg }, JsonRequestBehavior.AllowGet);
                //
                string username = Session["USERNAME"].ToString();
                Helper.ExecuteQuery("CANCEL_APPROVED  '" + Id + "','" + username + "'");
                return Json(new { Success = true, Message = "Document successfully cancel approve!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Message = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Received_Doc(string Id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                //if (Helper.IsClosed(Id))
                //    return Json(new { Success = false, Message = Helper.Msg }, JsonRequestBehavior.AllowGet);
                //
                string username = Session["USERNAME"].ToString();
                Helper.ExecuteQuery("RECEIVED_DOC  '" + Id + "','" + username + "'");
                return Json(new { Success = true, Message = "Document successfully received!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Message = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult submit_chg_pwd(FormCollection frm)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                //return Json(new { Success = true, Message = "Data successfully update!" }, JsonRequestBehavior.AllowGet);
                string pwd1 = frm["pwd1"];
                string pwd2 = frm["pwd2"];
                string pwd3 = frm["pwd3"];

                string username = Session["USERNAME"].ToString();
                B2BSCMDb db = new B2BSCMDb();
                var user = db.MS_USER.FirstOrDefault(x => x.Usr == username);
                string pwd1_enc = WMS.Library.Security.EncryptData("Q!W@E#R$", pwd1);

                if (pwd1_enc != user.Pwd)
                    return Json(new { Success = false, Message = "Error: The current password does not match!" }, JsonRequestBehavior.AllowGet);

                if (pwd3 != pwd2)
                    return Json(new { Success = false, Message = "Error: The password confirmation does not match!" }, JsonRequestBehavior.AllowGet);

                string password = WMS.Library.Security.EncryptData("Q!W@E#R$", pwd2);

                Helper.ExecuteQuery("UPDATE_USER_PWD '" + username + "','" + password + "','" + username + "'");

                return Json(new { Success = true, Message = "Password successfully update!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult unlock_acc(FormCollection frm)
        {
            try
            {
                //return Json(new { Success = true, Message = "Data successfully update!" }, JsonRequestBehavior.AllowGet);
                string user_ = frm["user"];

                DateTime dt_now = DateTime.Now;
                string dt_exp = dt_now.AddHours(2).ToString("yyyy-MM-dd HH:mm");

                B2BSCMDb db = new B2BSCMDb();
                var user = db.MS_USER.FirstOrDefault(x => x.Usr == user_);
                if (user != null)
                {
                    if ((bool)user.Active)
                    {
                        DataTable dt = Helper.LoadQuery("CREATE_TOKEN '" + user_ + "','UA', '" + dt_exp + "'");

                        string new_line = "<br/>";
                        string message = "Hello," + new_line + new_line;
                        message += "As a security precaution, access to your B2B portal account was recently disabled." + new_line;
                        message += "To enable your access to B2B portal again you must <a href=\"http://b2b.bungasari.com\\user?token=" + dt.Rows[0][0].ToString() + "&type=UA\">click here</a>" + new_line + new_line;
                        message += "Your link will be expired on " + dt_now.AddHours(2).ToString("dd-MMM-yy HH:mm") + "" + new_line + new_line;
                        message += "Thanks," + new_line;
                        message += "This is an automated message, please do not reply" + new_line;

                        Helper.SendEmail(user.Email, "REQUEST FOR UNLOCK ACCOUNT", message);
                    }


                }

                //if (pwd1_enc != user.Pwd)
                //    return Json(new { Success = false, Message = "Error: The current password does not match!" }, JsonRequestBehavior.AllowGet);

                //if (pwd3 != pwd2)
                //    return Json(new { Success = false, Message = "Error: The password confirmation does not match!" }, JsonRequestBehavior.AllowGet);

                //string password = WMS.Library.Security.EncryptData("Q!W@E#R$", pwd2);

                //Helper.ExecuteQuery("UPDATE_USER_PWD '" + username + "','" + password + "','" + username + "'");

                return Json(new { Success = true, Message = "Request for unlock account has been sent to email!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Route("user")]
        public ActionResult user(string token, string type = "")
        {
            DataTable dt = Helper.LoadQuery("CHECK_TOKEN '" + token + "', '" + type + "'");
            TempData["Message"] = dt.Rows[0][0].ToString();
            return RedirectToAction("login");
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


        [HttpGet]
        public ActionResult _partial_timeline(string uid = "")
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");

            //string username = Session["USERNAME"].ToString();
            //Custom model = new Custom();
            //model.UID_Chat = uid;
            //model.Custom_List_Chat = Helper.SelectLists.ChatByGuid(uid, username);
            ////model.Custom_List_Doc_Type = Helper.SelectLists.DocType2(seller, shipper, mode, pid);
            ///
            DataTable dt = Helper.LoadQuery("EXEC TRACKER '"+ uid + "'");

            return PartialView("_partial_timeline", dt);
        }

        [HttpPost]
        public ActionResult update_ltc_due_date(string id, string date)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                DateTime dt = new DateTime(Convert.ToInt32(date.Split('-')[2]), Convert.ToInt32(date.Split('-')[1]), Convert.ToInt32(date.Split('-')[0]));

                Helper.ExecuteQuery("UPDATE_LTCP3 '" + id + "','" + dt.ToString("yyyy-MM-dd") + "','','" + username + "'");
                return Json(new { Success = true, Message = "LTC Due Date successfully saved!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Message = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult update_ltc_payment_due_date(string id, string date)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                DateTime dt = new DateTime(Convert.ToInt32(date.Split('-')[2]), Convert.ToInt32(date.Split('-')[1]), Convert.ToInt32(date.Split('-')[0]));

                Helper.ExecuteQuery("UPDATE_LTCP3 '" + id + "','','" + dt.ToString("yyyy-MM-dd") + "','" + username + "'");
                return Json(new { Success = true, Message = "LTC Payment Due Date successfully saved!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Message = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}