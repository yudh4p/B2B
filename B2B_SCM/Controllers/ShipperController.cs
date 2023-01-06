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
    public class ShipperController : Controller
    {
        [Route("shipper/index")]
        public ActionResult Index_Shipper()
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");

            if (Session["ACCOUNTTYPE"].ToString() == "SCM")
                return RedirectToAction("index", "home");
            if (Session["ACCOUNTTYPE"].ToString() == "SELLER")
                return RedirectToAction("index_seller", "seller");
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
            model.CourierList = Helper.SelectLists.CourierList();
            model.DateFrom = DateTime.Now.AddMonths(-1);
            model.DateTo = DateTime.Now;
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

                dt.Columns.Remove("UNITPRICE");
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
                    Shipper = "", Seller = "", Remark = "", DocRequired = "", PurchaseIDE = "", Origin_Name = "", Shipper_Name = "", Seller_Name = "", StatusDesDem = "", FinalAmount = "", FinalAmount_C = "";

                string VendorCode = Session["VENDORCODE"].ToString();
                string accType = Session["ACCOUNTTYPE"].ToString();
                DataTable dt = Helper.LoadQuery("GET_HEADER_TRX '" + id + "', '" + accType + "', '" + VendorCode + "'");
                if (dt != null && dt.Rows.Count > 0)
                {
                    PurchaseIDE = dt.Rows[0]["PurchaseNo2"].ToString();
                    PurchaseNo = dt.Rows[0]["PurchaseNo1"].ToString() + "." + dt.Rows[0]["PurchaseNo2_C"].ToString();
                    TradeQty = dt.Rows[0]["TradeQty"].ToString();
                    UnitPrice = "";
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
                    FinalAmount = dt.Rows[0]["FinalAmount"].ToString();
                    FinalAmount_C = dt.Rows[0]["FinalAmount_C"].ToString();
                }

                string CommDiscDate = "", CompDiscDate = "", PaymentDueDate = "", Confirm = "", ConfirmBy = "", ConfirmDate = "";
                DataTable dt3 = Helper.LoadQuery("GET_LTCP '" + id + "'");
                if (dt3 != null && dt3.Rows.Count > 0)
                {
                    CommDiscDate = dt3.Rows[0]["LTCCommDiscDate"].ToString();
                    CompDiscDate = dt3.Rows[0]["LTCCompDiscDate"].ToString();
                    //PaymentDueDate = dt3.Rows[0]["PaymentDueDate"].ToString();
                    Confirm = dt3.Rows[0]["LTCConfirm"].ToString();
                    ConfirmBy = dt3.Rows[0]["LTCConfirmBy"].ToString();
                    ConfirmDate = dt3.Rows[0]["LTCConfirmDate"].ToString();
                    StatusDesDem = dt3.Rows[0]["STATUSDESDEM"].ToString();
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
                    PaymentDueDate = PaymentDueDate.Replace(" 00:00:00", ""),
                    StatusDesDemConfirm = Confirm,
                    ConfirmBy = ConfirmBy,
                    ConfirmDate = ConfirmDate,
                    StatusDesDem = StatusDesDem,
                    FinalAmount = FinalAmount,
                    FinalAmount_C = FinalAmount_C,
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
                string UID = "", QTY = "",  ETALOAD = "", ETBLOAD = "", ETDLOAD = "", WHEATNAME = "", FINALQTY = "", LOADPORT = "", ETADISC = "";

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
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult get_header_vessel(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                string USERNAME = Session["USERNAME"].ToString();
                string PURCHASENO2 = "", VESSELNAME = "", VESSELDETAIL = "", RATES = "", APPOINTAGENT = "";

                DataTable dt = Helper.LoadQuery("GET_VESSEL_INFO '" + id + "'");
                if (dt != null && dt.Rows.Count > 0)
                {
                    PURCHASENO2 = dt.Rows[0]["PURCHASENO2"].ToString();
                    VESSELNAME = dt.Rows[0]["VESSELNAME"].ToString();
                    VESSELDETAIL = dt.Rows[0]["VESSELDETAIL"].ToString();
                    RATES = dt.Rows[0]["RATES"].ToString();
                    APPOINTAGENT = dt.Rows[0]["APPOINTAGENT"].ToString();
                }

                return Json(new
                {
                    success = true,
                    PURCHASENO2 = PURCHASENO2,
                    VESSELNAME = VESSELNAME,
                    VESSELDETAIL = VESSELDETAIL,
                    APPOINTAGENT = APPOINTAGENT,
                    RATES = RATES,
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
                string accountType = Session["ACCOUNTTYPE"].ToString();
                DataTable dt = Helper.LoadQuery("GET_HEADER_TRX_DOC '" + id + "','" + accountType + "','" + USERNAME + "'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return null;
            }
        }

        public ActionResult Check_Document(string purchaseno, string doctype)
        {
            DataTable dt_check_exists = Helper.LoadQuery("CHECK_DOC_EXISTS '" + doctype + "', '" + purchaseno + "'");
            if (Convert.ToInt32(dt_check_exists.Rows[0][0]) > 0)
            {
                return Json(new { Success = true, Status = Convert.ToInt32(dt_check_exists.Rows[0][0]), Message = "Document is already exists, Do you want still upload?" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Success = false, Message = "Upload success!" }, JsonRequestBehavior.AllowGet);
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


        //[HttpPost]
        //public ActionResult update_shipper(Models.Custom model, FormCollection frm)
        //{
        //    if (Session["USERNAME"] == null)
        //        return RedirectToAction("login");
        //    try
        //    {

        //        //DataTable dt_check_ = Helper.LoadQuery("VALIDASI_DOC_SHIPPER '" + model.PurchaseIDE + "'");
        //        //if (dt_check_ != null && dt_check_.Rows.Count > 0)
        //        //{
        //        //    if (dt_check_.Rows[0][0].ToString() == "1")
        //        //    {
        //        //        TempData["Message"] = "Error: (" + model.PurchaseNoE + ") - " + dt_check_.Rows[0][1].ToString();
        //        //        return RedirectToAction("Index_Shipper");
        //        //    }
        //        //}
        //        if (Helper.IsClosed(model.PurchaseIDE.ToString()))
        //        {
        //            TempData["Message"] = Helper.Msg;
        //            return RedirectToAction("Index_Shipper");
        //        }

        //        string username = Session["USERNAME"].ToString();

        //        string port = frm["LoadPort"];

        //        B2BSCMDb db = new B2BSCMDb();
        //        var result = db.TRX_SHP.FirstOrDefault(x => x.PurchaseNo2 == model.PurchaseIDE);
        //        if (result != null)
        //        {
        //            result.VesselName = model.VesselName;
        //            result.VesselDetail = model.VesselDetail;
        //            result.AppointAgent = model.AppointAgent;
        //            result.UpdBy = username;
        //            result.UpdDate = DateTime.Now;

        //            db.Entry(result).State = EntityState.Modified;
        //            db.SaveChanges();
        //        }
        //        else
        //        {
        //            TRX_SHP trx = new TRX_SHP();
        //            trx.PurchaseNo2 = model.PurchaseIDE;
        //            trx.VesselName = model.VesselName;
        //            trx.VesselDetail = model.VesselDetail;
        //            trx.AppointAgent = model.AppointAgent;
        //            trx.CreatedUser = username;
        //            trx.CreatedDate = DateTime.Now;
        //            db.TRX_SHP.Add(trx);
        //            db.SaveChanges();
        //        }

        //        Helper.ExecuteQuery("UPDATE_RATES  '" + model.PurchaseIDE + "','" + model.DemDes + "','" + username + "'");

        //        TempData["Message"] = "Transaction successfully updated!";
        //    }
        //    catch (Exception e)
        //    {
        //        TempData["Message"] = "Error: " + e.Message;
        //    }
        //    return RedirectToAction("Index_Shipper");
        //}


        [HttpPost]
        public ActionResult update_shipper(FormCollection frm)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string PurchaseNo = frm["PurchaseNo"];
                string VesselName = frm["VesselName"];
                string VesselDetail = frm["VesselDetail"];
                string DemDes = frm["DemDes"];
                string AppointAgent = frm["AppointAgent"];


                if (Helper.IsClosed(PurchaseNo))
                {
                    return Json(new { Success = false, Message = "Error: " + Helper.Msg }, JsonRequestBehavior.AllowGet);
                }

                string username = Session["USERNAME"].ToString();

                long PurchaseNoInt = Convert.ToInt32(PurchaseNo);

                Helper.ExecuteQuery("UPDATE_SHIPPER  '" + VesselName.Replace("'", "-").Replace("\"", "-") + "','" + VesselDetail.Replace("'", "-").Replace("\"", "-") + "','" + AppointAgent.Replace("'", "-").Replace("\"", "-") + "','" + username + "','" + PurchaseNoInt + "'");


                //B2BSCMDb db = new B2BSCMDb();
                //var result = db.TRX_SHP.FirstOrDefault(x => x.PurchaseNo2 == PurchaseNoInt);
                //if (result != null)
                //{
                //    result.VesselName = VesselName;
                //    result.VesselDetail = VesselDetail;
                //    result.AppointAgent = AppointAgent;
                //    result.UpdBy = username;
                //    result.UpdDate = DateTime.Now;

                //    db.Entry(result).State = EntityState.Modified;
                //    db.SaveChanges();
                //}
                //else
                //{
                //    TRX_SHP trx = new TRX_SHP();
                //    trx.PurchaseNo2 = PurchaseNoInt;
                //    trx.VesselName = VesselName;
                //    trx.VesselDetail = VesselDetail;
                //    trx.AppointAgent = AppointAgent;
                //    trx.CreatedUser = username;
                //    trx.CreatedDate = DateTime.Now;
                //    db.TRX_SHP.Add(trx);
                //    db.SaveChanges();
                //}

                Helper.ExecuteQuery("UPDATE_RATES  '" + PurchaseNo + "','" + DemDes + "','" + username + "'");

                return Json(new { Success = true, Message = "Data successfully update" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Message = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult update_ltc(FormCollection frm)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string PurchaseNo = frm["PurchaseNo"];
                string Type = frm["Type"];
                string Remark = frm["Remark"];
                string username = Session["USERNAME"].ToString();

                if (Helper.IsClosed(PurchaseNo))
                    return Json(new { Success = false, Message = Helper.Msg }, JsonRequestBehavior.AllowGet);

                B2BSCMDb db = new B2BSCMDb();
                TRX_DOC doc = new TRX_DOC();

                HttpPostedFileBase postedFile = Request.Files["Image"];
                //HttpPostedFileBase[] postedFiles = (HttpPostedFileBase[])Request.F["Image"];
                if (postedFile == null)
                {
                    return Json(new { Success = false, Message = "Document is nothing, please Upload document first, and click approved/postpone" });
                }


                doc.PurchaseNo2 = Convert.ToInt32(PurchaseNo);
                doc.AttachmentStatus = Type == "CONFIRM" ? 4 : 2;

                doc.Remark = Remark;
                doc.DocType = 31;
                doc.CreatedUser = username;
                doc.CreatedDate = DateTime.Now;
                doc.Status = true;

                if (postedFile != null)
                {
                    string FileName = System.IO.Path.GetFileNameWithoutExtension(postedFile.FileName);
                    string FileExtn = System.IO.Path.GetExtension(postedFile.FileName);
                    if (FileExtn != ".pdf")
                    {
                        return Json(new { Success = false, Message = "Only pdf can upload!" });
                    }

                    BinaryReader Binary_Reader = new BinaryReader(postedFile.InputStream);
                    byte[] File_Buffer = Binary_Reader.ReadBytes(postedFile.ContentLength);

                    doc.AttachmentFile = File_Buffer;
                    doc.AttachmentName = postedFile.FileName;
                    doc.AttachmentType = postedFile.ContentType;
                }
                else
                {
                    doc.AttachmentFile = null;
                    doc.AttachmentName = null;
                    doc.AttachmentType = null;
                }

                db.TRX_DOC.Add(doc);
                db.SaveChanges();

                Helper.ExecuteQuery("UPDATE_LTCP2 '" + Type + "','" + PurchaseNo + "','" + username + "'");

                return Json(new { Success = true, Message = "Data successfully updated!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
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

        [HttpGet]
        public ActionResult get_detail_awb(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login", "home");
            try
            {
                string USERNAME = Session["USERNAME"].ToString();
                string AWB = "", COURIERCODE = "", REMARK = "", CTGRY = "";

                DataTable dt = Helper.LoadQuery("GET_DETAIL_AWB '" + id + "'");
                if (dt != null && dt.Rows.Count > 0)
                {
                    AWB = dt.Rows[0]["AWB"].ToString();
                    COURIERCODE = dt.Rows[0]["COURIERCODE"].ToString();
                    REMARK = dt.Rows[0]["REMARK"].ToString();
                    CTGRY = dt.Rows[0]["CTGRY"].ToString();
                }
                return Json(new
                {
                    success = true,
                    AWB = AWB,
                    COURIERCODE = COURIERCODE,
                    REMARK = REMARK,
                    CTGRY = CTGRY,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult delete_awb(string id)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();

                Helper.ExecuteQuery("DELETE_AWB '" + username + "','" + id + "'");
                return Json(new { Success = true, Message = "Data successfully delete!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error: " + e.Message;
            }
            return RedirectToAction("index");
        }
    }
}