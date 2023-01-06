using B2B_SCM.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace B2B_SCM.Controllers
{
    public class MasterController : Controller
    {
        [Route("transaction/master/bp")]
        public ActionResult business_partner()
        {
            Custom model = new Custom();
            model.CountryList = Helper.SelectLists.CountryList(withEmptyValue: true);
            return View(model);
        }

        [Route("transaction/master/bp/pic")]
        public ActionResult business_partner_pic()
        {
            Custom model = new Custom();
            model.BPList = Helper.SelectLists.BPList();
            return View(model);
        }

        [HttpGet]
        public ActionResult load_business_partner()
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                DataTable dt = Helper.LoadQuery("GET_MS_BP '" + username + "'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return null;
            }
        }

        [HttpGet]
        public ActionResult load_business_partner_cp()
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                DataTable dt = Helper.LoadQuery("GET_MS_BP_CP '" + username + "'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return null;
            }
        }

        [HttpPost]
        public ActionResult submit_new_bp(Models.Custom model, FormCollection frm)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                string vendorType = frm["AccountType"];
                if (model.ModeTransaction == "NEW")
                {
                    if (vendorType == "BOTH")
                    {
                        Helper.ExecuteQuery("INSERT_MS_BP '" + model.VendorName + "','SELLER','" + model.Address + "','" + model.CountryId + "','" + model.Email + "','" + model.Phone + "','" + username + "'");
                        Helper.ExecuteQuery("INSERT_MS_BP '" + model.VendorName + "','SHIPPER','" + model.Address + "','" + model.CountryId + "','" + model.Email + "','" + model.Phone + "','" + username + "'");
                    }
                    else
                    {
                        Helper.ExecuteQuery("INSERT_MS_BP '" + model.VendorName + "','" + vendorType + "','" + model.Address + "','" + model.CountryId + "','" + model.Email + "','" + model.Phone + "','" + username + "'");
                    }
                }
                else
                    Helper.ExecuteQuery("UPDATE_MS_BP  '" + model.VendorCode + "','" + model.VendorName + "','" + vendorType + "','" + model.Address + "','" + model.CountryId + "','" + model.Email + "','" + model.Phone + "','','" + username + "'");

                TempData["Message"] = "Data successfully submitted!";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error: " + e.Message;
            }
            return RedirectToAction("business_partner");
        }

        [HttpPost]
        public ActionResult submit_new_bp_pic(Models.Custom model, FormCollection frm)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                string vendorType = frm["AccountType"];
                if (model.ModeTransaction == "NEW")
                    Helper.ExecuteQuery("INSERT_MS_BP_CP '" + model.VendorCode + "','" + model.PICName + "','" + model.PICTitle + "','" + model.PICEmail + "','" + model.PICPhone + "','','" + username + "'");
                else
                    Helper.ExecuteQuery("UPDATE_MS_BP_CP  '" + model.UID + "','" + model.VendorCode + "','" + model.PICName + "','" + model.PICTitle + "','" + model.PICEmail + "','" + model.PICPhone + "','','" + username + "'");

                TempData["Message"] = "Data successfully submitted!";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error: " + e.Message;
            }
            return RedirectToAction("business_partner_pic");
        }


        [HttpPost]
        public ActionResult active_deactive_bp(string id, string status)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                Helper.ExecuteQuery("UPDATE_MS_BP  '" + id + "','','','','','','','" + status + "','" + username + "'");
                return Json(new { success = true, message = "Data successfully updated!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult get_detail_bp(string id)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                string VendorCode = "", VendorName = "", VendorType = "", Address = "", CountryId = "", Email = "", Phone = "", Status = "";
                DataTable dt = Helper.LoadQuery("GET_MS_BP  '" + username + "','" + id + "'");
                if (dt != null && dt.Rows.Count > 0)
                {
                    VendorCode = dt.Rows[0]["VendorCode"].ToString();
                    VendorName = dt.Rows[0]["VendorName"].ToString();
                    VendorType = dt.Rows[0]["VendorType"].ToString();
                    Address = dt.Rows[0]["Address"].ToString();
                    CountryId = dt.Rows[0]["CountryId"].ToString();
                    Email = dt.Rows[0]["Email"].ToString();
                    Phone = dt.Rows[0]["Phone"].ToString();
                    Status = dt.Rows[0]["Status"].ToString();
                }

                return Json(new
                {
                    success = true,
                    VendorCode = VendorCode,
                    VendorName = VendorName,
                    VendorType = VendorType,
                    Address = Address,
                    CountryId = CountryId,
                    Email = Email,
                    Phone = Phone,
                    Status = Status,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult get_detail_bp_cp(string id)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                string UID = "", VENDORCODE = "", VENDORNAME = "", PICNAME = "", PICTITLE = "", PICEMAIL = "", PICPHONE = "";
                DataTable dt = Helper.LoadQuery("GET_MS_BP_CP  '" + username + "','" + id + "'");
                if (dt != null && dt.Rows.Count > 0)
                {
                    UID = dt.Rows[0]["UID"].ToString();
                    VENDORCODE = dt.Rows[0]["VENDORCODE"].ToString();
                    VENDORNAME = dt.Rows[0]["VENDORNAME"].ToString();
                    PICNAME = dt.Rows[0]["PICNAME"].ToString();
                    PICTITLE = dt.Rows[0]["PICTITLE"].ToString();
                    PICEMAIL = dt.Rows[0]["PICEMAIL"].ToString();
                    PICPHONE = dt.Rows[0]["PICPHONE"].ToString();
                }

                return Json(new
                {
                    success = true,
                    UID = UID,
                    VENDORCODE = VENDORCODE,
                    VENDORNAME = VENDORNAME,
                    PICNAME = PICNAME,
                    PICTITLE = PICTITLE,
                    PICEMAIL = PICEMAIL,
                    PICPHONE = PICPHONE,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult active_deactive_bp_cp(string id, string status)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                Helper.ExecuteQuery("UPDATE_MS_BP_CP  '" + id + "','','','','','','" + status + "','" + username + "'");
                return Json(new { success = true, message = "Data successfully updated!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [Route("transaction/master/country")]
        public ActionResult country()
        {
            Custom model = new Custom();
            model.BPList = Helper.SelectLists.BPList();
            return View(model);
        }

        [HttpGet]
        public ActionResult load_country()
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                DataTable dt = Helper.LoadQuery("GET_MS_COUNTRY '" + username + "'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return null;
            }
        }

        [HttpGet]
        public ActionResult get_detail_country(string id)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                string CountryId = "", CountryName = "";
                DataTable dt = Helper.LoadQuery("GET_MS_COUNTRY  '" + username + "','" + id + "'");
                if (dt != null && dt.Rows.Count > 0)
                {
                    CountryId = dt.Rows[0]["ID"].ToString();
                    CountryName = dt.Rows[0]["NAME"].ToString();
                }

                return Json(new
                {
                    success = true,
                    CountryId = CountryId,
                    CountryName = CountryName,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult submit_new_country(Models.Custom model, FormCollection frm)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                if (model.ModeTransaction == "NEW")
                    Helper.ExecuteQuery("INSERT_MS_COUNTRY '" + model.CountryName + "','" + username + "'");
                else
                    Helper.ExecuteQuery("UPDATE_MS_COUNTRY  '" + model.CountryId + "','" + model.CountryName + "','" + username + "',''");

                TempData["Message"] = "Data successfully submitted!";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error: " + e.Message;
            }
            return RedirectToAction("country");
        }

        [HttpPost]
        public ActionResult active_deactive_country(string id, string status)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                Helper.ExecuteQuery("UPDATE_MS_COUNTRY  '" + id + "','','" + username + "','" + status + "'");
                return Json(new { success = true, message = "Data successfully updated!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [Route("transaction/master/port")]
        public ActionResult port()
        {
            Custom model = new Custom();
            model.BPList = Helper.SelectLists.BPList();
            model.CountryList = Helper.SelectLists.CountryList();
            return View(model);
        }

        [HttpGet]
        public ActionResult load_port()
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                DataTable dt = Helper.LoadQuery("GET_MS_PORT '" + username + "'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return null;
            }
        }


        [HttpGet]
        public ActionResult get_detail_port(string id)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                string CountryId = "", PortId = "", PortName = "";
                DataTable dt = Helper.LoadQuery("GET_MS_PORT  '" + username + "','" + id + "'");
                if (dt != null && dt.Rows.Count > 0)
                {
                    PortId = dt.Rows[0]["ID"].ToString();
                    PortName = dt.Rows[0]["NAME"].ToString();
                    CountryId = dt.Rows[0]["COUNTRYID"].ToString();
                }

                return Json(new
                {
                    success = true,
                    PortId = PortId,
                    PortName = PortName,
                    CountryId = CountryId,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult submit_new_port(Models.Custom model, FormCollection frm)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                if (model.ModeTransaction == "NEW")
                    Helper.ExecuteQuery("INSERT_MS_PORT '" + model.PortName + "','" + model.CountryId + "','" + username + "'");
                else
                    Helper.ExecuteQuery("UPDATE_MS_PORT '" + model.PortID + "','" + model.PortName + "','" + model.CountryId + "','" + username + "',''");

                TempData["Message"] = "Data successfully submitted!";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error: " + e.Message;
            }
            return RedirectToAction("port");
        }

        [HttpPost]
        public ActionResult active_deactive_port(string id, string status)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                Helper.ExecuteQuery("UPDATE_MS_PORT '" + id + "','','','" + username + "','" + status + "'");
                return Json(new { success = true, message = "Data successfully updated!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [Route("transaction/master/bp/doc")]
        public ActionResult bp_doc()
        {
            Custom model = new Custom();
            model.BPList = Helper.SelectLists.BPList();
            model.CountryList = Helper.SelectLists.CountryList();
            return View(model);
        }

        [HttpGet]
        public ActionResult load_bp_doc(string vendorCode)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                DataTable dt = Helper.LoadQuery("GET_MS_BP_DOC_TYPE '" + vendorCode + "'");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return null;
            }
        }


        [HttpPost]
        public ActionResult enabled_disabled_bp_doc(string id, string status, string type)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                Helper.ExecuteQuery("UPDATE_MS_BP_DOC  '" + id + "','" + (status == "true" ? "1" : "0") + "','" + type + "','" + username + "'");
                return Json(new { success = true, message = "Data successfully updated!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Route("transaction/master/user")]
        public ActionResult user()
        {
            Custom model = new Custom();
            model.BPList = Helper.SelectLists.BPList();
            model.CountryList = Helper.SelectLists.CountryList();

            model.au_sellercode_list = Helper.SelectLists.BPListByType(vendorType: "SELLER");
            model.au_shippercode_list = Helper.SelectLists.BPListByType(vendorType: "SHIPPER");
            return View(model);
        }

        [HttpGet]
        public ActionResult load_ms_user()
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                DataTable dt = Helper.LoadQuery("GET_MS_USER 'I',''");
                return Json(new { data = JsonConvert.SerializeObject(dt) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return null;
            }
        }

        [HttpGet]
        public ActionResult get_ms_user(string id)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                string username = Session["USERNAME"].ToString();
                string user = "", fullname = "", email = "", phone = "", accounttype = "", locked = "", active = "";
                DataTable dt = Helper.LoadQuery("GET_MS_USER 'I','" + id + "'");
                if (dt != null && dt.Rows.Count > 0)
                {
                    user = dt.Rows[0]["USR"].ToString();
                    fullname = dt.Rows[0]["FN"].ToString();
                    email = dt.Rows[0]["EMAIL"].ToString();
                    phone = dt.Rows[0]["PHONE"].ToString();
                    accounttype = dt.Rows[0]["ACCOUNTTYPE2"].ToString();
                    locked = dt.Rows[0]["LOCKED"].ToString();
                    active = dt.Rows[0]["ACTIVE"].ToString();
                }

                return Json(new
                {
                    success = true,
                    user = user,
                    fullname = fullname,
                    email = email,
                    phone = phone,
                    accounttype = accounttype,
                    locked = locked,
                    active = active,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult submit_new_user(Models.Custom model, FormCollection frm)
        {
            //if (Session["USERNAME"] == null)
            //    return RedirectToAction("login");
            try
            {
                B2BSCMDb db = new B2BSCMDb();
                var check = db.MS_USER.Where(x => x.Usr == model.Usr).Count();
                if (check > 0)
                {
                    TempData["Message"] = "Error: User is already exists!";
                    return RedirectToAction("user");
                }

                var check2 = db.MS_USER.Where(x => x.Email == model.Email).Count();
                if (check2 > 0)
                {
                    TempData["Message"] = "Error: Email is already exists!";
                    return RedirectToAction("user");
                }

                string password = Helper.RandomGenerator.RandomPassword().ToUpper();

                string username = Session["USERNAME"].ToString();
                string AccountType = frm["AccountType"];
                MS_USER usr = new MS_USER();
                usr.Usr = model.Usr;
                usr.Pwd = WMS.Library.Security.EncryptData("Q!W@E#R$", password);
                usr.Fn = model.Fn;
                usr.AccountType = AccountType;
                usr.Email = model.Email;
                usr.Phone = model.Phone;

                if (AccountType == "SELLER")
                    usr.VendorCode = model.au_sellercode;
                if (AccountType == "SHIPPER")
                    usr.VendorCode = model.au_shippercode;

                usr.Locked = false;
                usr.Active = true;
                usr.CreatedDate = DateTime.Now;
                usr.CreatedUser = username;
                db.MS_USER.Add(usr);
                db.SaveChanges();

                string new_line = "<br/>";
                string message = "Dear member," + new_line + new_line;
                message += "This is username and password for login into system," + new_line + new_line;
                message += "Details" + new_line;
                message += "Username : " + usr.Usr + new_line;
                message += "Password : " + password + new_line + new_line;

                //message += "Item&ensp;&ensp;&ensp;&ensp;: " + new_line;
                //message += "For detail you can see on http://halopurchasing.bungasari.com" + new_line + new_line;
                message += "Thanks," + new_line;
                message += "This is an automated message, please do not reply" + new_line;
                Helper.SendEmail(usr.Email, "Password for login B2B.bungasari.com", message);
                TempData["Message"] = "User successfully created, password for login already send to email " + usr.Email;
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error: " + e.Message;
            }
            return RedirectToAction("user");
        }

        [HttpPost]
        public ActionResult submit_edit_user(FormCollection frm)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                //return Json(new { Success = true, Message = "Data successfully update!" }, JsonRequestBehavior.AllowGet);
                string sellercode = frm["sellercode"];
                string shippercode = frm["shippercode"];
                string user = frm["user"];
                string fn = frm["fn"];
                string email = frm["email"];
                string phone = frm["phone"];

                string lock_ = frm["lock"];
                string active = frm["active"];

                string username = Session["USERNAME"].ToString();

                Helper.ExecuteQuery("UPDATE_USER '" + user + "','" + fn + "','" + email + "','" + phone + "','" + sellercode + "','" + shippercode + "','" + lock_ + "','" + active + "','" + username + "'");

                return Json(new { Success = true, Message = "Data successfully update!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult submit_reset_pwd(FormCollection frm)
        {
            if (Session["USERNAME"] == null)
                return RedirectToAction("login");
            try
            {
                //return Json(new { Success = true, Message = "Data successfully update!" }, JsonRequestBehavior.AllowGet);
                string user = frm["user"];
                string pwd1 = frm["pwd1"];
                string pwd2 = frm["pwd2"];

                if(pwd1 != pwd2)
                    return Json(new { Success = false, Message = "Error: The password confirmation does not match!" }, JsonRequestBehavior.AllowGet);

                string username = Session["USERNAME"].ToString();
                string password = WMS.Library.Security.EncryptData("Q!W@E#R$", pwd1);

                Helper.ExecuteQuery("UPDATE_USER_PWD '" + user + "','" + password + "','" + username + "'");

                return Json(new { Success = true, Message = "Data successfully update!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}