using B2B_SCM.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace B2B_SCM
{
    public static class Helper
    {
        public static Image GetImageFromPicPath(string strUrl)
        {
            using (WebResponse wrFileResponse = WebRequest.Create(strUrl).GetResponse())
            using (Stream objWebStream = wrFileResponse.GetResponseStream())
            {
                MemoryStream ms = new MemoryStream();
                objWebStream.CopyTo(ms, 8192);
                return System.Drawing.Image.FromStream(ms);
            }
        }

        public class RandomGenerator
        {
            // Generate a random number between two numbers    
            public static int RandomNumber(int min, int max)
            {
                Random random = new Random();
                return random.Next(min, max);
            }

            // Generate a random string with a given size    
            private static string RandomString(int size, bool lowerCase)
            {
                StringBuilder builder = new StringBuilder();
                Random random = new Random();
                char ch;
                for (int i = 0; i < size; i++)
                {
                    ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                    builder.Append(ch);
                }
                if (lowerCase)
                    return builder.ToString().ToLower();
                return builder.ToString();
            }

            // Generate a random password    
            public static string RandomPassword()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(RandomString(4, true));
                builder.Append(RandomNumber(1000, 9999));
                builder.Append(RandomString(2, false));
                return builder.ToString().ToUpper();
            }

        }
        public static string Msg = "";
        private static string Execute(string Url, string User, string Password)
        {
            string result = "";
            string username = User != "" ? User : "BFM.ABAP01";
            string password = Password != "" ? Password : "Bfm1234";


            System.Net.HttpWebRequest request = WebRequest.Create(Url) as HttpWebRequest;
            String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
            request.Headers.Add("Authorization", "Basic " + encoded);
            request.ContentType = "application/json";
            //get the response
            System.Net.WebResponse response = request.GetResponse();
            System.IO.Stream responseStream = response.GetResponseStream();
            System.IO.StreamReader responseReader = new System.IO.StreamReader(responseStream);

            string responsestring = responseReader.ReadToEnd();
            result = (responsestring);
            responseReader.Close();
            responseStream.Dispose();
            responseStream.Dispose();
            return result;
        }

        public static DataTable SAP_WT(string Url, string User, string Password, string Json = "")
        {
            DateTime dt_now = DateTime.Now;
            string json_ = Json != "" ? Json : Execute(Url, User, Password);
            Models.OData.OutboundWT cc = JsonConvert.DeserializeObject<Models.OData.OutboundWT>(json_);

            DataTable dt = new DataTable();
            dt.Columns.Add("Matnr");
            dt.Columns.Add("Maktx");

            foreach (Models.OData.OutboundWTResults c2 in cc.d.results)
            {
                DataRow row = dt.NewRow();
                row["Matnr"] = c2.Matnr;
                row["Maktx"] = c2.Maktx;
                dt.Rows.Add(row);
            }
            return dt;
        }

        public static bool ExecuteQuery(string query)
        {
            using (var context = new B2BSCMDb())
            {
                var dt = new DataTable();
                var conn = context.Database.Connection;
                var connectionState = conn.State;
                try
                {
                    if (connectionState != ConnectionState.Open) conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = query;
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteReader();
                        Msg = "";
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Msg = "Error: " + ex.Message + " ";
                    return false;
                    throw;
                }
                finally
                {
                    if (connectionState != ConnectionState.Closed) conn.Close();
                }
            }
        }

        public static string Get_Format_PurchaseNo(string purchaseno)
        {
            return Helper.LoadQuery("GET_FORMAT_PUCRCHASENO '" + purchaseno + "'").Rows[0][0].ToString();
        }

        public static DataTable LoadQuery(string query)
        {
            using (var context = new B2BSCMDb())
            {
                var dt = new DataTable();
                var conn = context.Database.Connection;
                var connectionState = conn.State;
                try
                {
                    if (connectionState != ConnectionState.Open) conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = query;
                        cmd.CommandType = CommandType.Text;

                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    if (connectionState != ConnectionState.Closed) conn.Close();
                }
                return dt;
            }
        }

        //public static byte[] ConvertToBytes(FormFile image)
        //{
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        image.OpenReadStream().CopyTo(memoryStream);
        //        return memoryStream.ToArray();
        //    }
        //}

        public static bool IsClosed(string purchaseNo = "")
        {
            bool ltrue = false;
            DataTable dt = Helper.LoadQuery("CHECK_CLOSED_TRANSACTION '" + purchaseNo + "'");
            if(dt!= null && dt.Rows.Count > 0)
            {
                if (dt.Rows[0][0].ToString() == "1")
                {
                    ltrue = true;
                    Msg = dt.Rows[0][1].ToString();
                }
                else
                {
                    ltrue = false;
                    Msg = "";
                }
            }
            return ltrue;
        }
        public static bool IsClosedbyUid(string uid = "")
        {
            bool ltrue = false;
            DataTable dt = Helper.LoadQuery("CHECK_CLOSED_TRANSACTION '" + uid + "'");
            if (dt != null && dt.Rows.Count > 0)
            {
                if (dt.Rows[0][0].ToString() == "1")
                {
                    ltrue = true;
                    Msg = dt.Rows[0][1].ToString();
                }
                else
                {
                    ltrue = false;
                    Msg = "";
                }
            }
            return ltrue;
        }

        public static void SendEmail(string toEmail, string subject, string body, string cc = "")
        {
            SmtpClient c = new SmtpClient("mail.bungasari.com", 587);
            MailMessage msg = new MailMessage();
            if (toEmail.Contains(";"))
            {
                string[] mail = toEmail.Split(';');
                foreach (string itm in mail)
                {
                    if (!string.IsNullOrWhiteSpace(itm))
                    {
                        msg.To.Add(new MailAddress(itm));
                    }
                }
            }
            else
            {
                msg.To.Add(new MailAddress(toEmail));
            }

            if (!string.IsNullOrWhiteSpace(cc))
            {
                if (cc.Contains(";"))
                {
                    string[] mail2 = cc.Split(';');
                    foreach (string itm2 in mail2)
                    {
                        if (!string.IsNullOrWhiteSpace(itm2))
                            msg.CC.Add(new MailAddress(itm2));
                    }
                }
                else
                {
                    msg.CC.Add(new MailAddress(cc));
                }
            }

            //msg.CC.Add(new MailAddress("system.notification@bungasari.com"));
            msg.From = new MailAddress("system.notification@bungasari.com");
            msg.IsBodyHtml = true;
            msg.Subject = subject;
            msg.Body = body;
            c.Credentials = new System.Net.NetworkCredential("system.notification@bungasari.com", "Bfm531!");
            c.EnableSsl = false;
            c.Send(msg);
        }

        public static void BulkInsert(string tableName, DataTable dt)
        {
            B2BSCMDb db = new B2BSCMDb();

            SqlConnection conn = db.Database.Connection as SqlConnection;

            SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.UseInternalTransaction, null);
            bulkCopy.DestinationTableName = tableName;
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            bulkCopy.WriteToServer(dt);
            conn.Close();
        }

        public static byte[] ConvertImageToByte(Image Value)
        {
            if (Value != null)
            {
                MemoryStream fs = new MemoryStream();
                ((Bitmap)Value).Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] retval = fs.ToArray();
                fs.Dispose();
                return retval;
            }
            return null;
        }
        public static class SelectLists
        {

            public static SelectList CountryList(bool withEmptyValue = false)
            {
                List<SelectListItem> list = new List<SelectListItem>();
                try
                {
                    DataTable dt = Helper.LoadQuery("GET_MS_COUNTRY");

                    foreach (DataRow itm in dt.Rows)
                    {
                        if (itm["STATUS"].ToString() == "ACTIVE")
                        {
                            list.Add(new SelectListItem()
                            {
                                Value = itm[0].ToString(),
                                Text = itm[1].ToString(),
                            });
                        }
                    }
                    if (withEmptyValue)
                    {
                        list.Add(new SelectListItem()
                        {
                            Value = "",
                            Text = "-Select Value-",
                        });
                    }

                }
                catch { }
                return new SelectList(list.OrderBy(x => x.Value), "Value", "Text");
            }

            public static SelectList BPList(bool withEmptyValue = false)
            {
                List<SelectListItem> list = new List<SelectListItem>();
                try
                {
                    DataTable dt = Helper.LoadQuery("GET_MS_BP");

                    foreach (DataRow itm in dt.Rows)
                    {
                        list.Add(new SelectListItem()
                        {
                            Value = itm[0].ToString(),
                            Text = itm[1].ToString(),
                        });
                    }
                    if (withEmptyValue)
                    {
                        list.Add(new SelectListItem()
                        {
                            Value = "",
                            Text = "All Data",
                        });
                    }

                }
                catch { }
                return new SelectList(list.OrderBy(x => x.Value), "Value", "Text");
            }

            public static SelectList WTList(bool withEmptyValue = false)
            {
                List<SelectListItem> list = new List<SelectListItem>();
                try
                {
                    string url = Helper.LoadQuery("select value1 from CONFIG where id = 3").Rows[0][0].ToString();
                    string usr = WMS.Library.Security.DecryptData(Helper.LoadQuery("select value1 from CONFIG where id = 4").Rows[0][0].ToString(), "111222333");
                    string pwd = WMS.Library.Security.DecryptData(Helper.LoadQuery("select value1 from CONFIG where id = 5").Rows[0][0].ToString(), "111222333");

                    DataTable dt_PR = Helper.SAP_WT(url, usr, pwd);

                    foreach (DataRow itm in dt_PR.Rows)
                    {
                        list.Add(new SelectListItem()
                        {
                            Value = itm[0].ToString() + "-" + itm[1].ToString(),
                            Text = itm[1].ToString(),
                        });
                    }
                    if (withEmptyValue)
                    {
                        list.Add(new SelectListItem()
                        {
                            Value = "",
                            Text = "-Select Value-",
                        });
                    }

                }
                catch { }
                return new SelectList(list.OrderBy(x => x.Value), "Value", "Text");
            }

            public static SelectList BPListByType(bool withEmptyValue = false, string vendorType = "")
            {
                List<SelectListItem> list = new List<SelectListItem>();
                try
                {
                    DataTable dt = Helper.LoadQuery("GET_MS_BP_SELECTLIST '" + vendorType + "'");

                    foreach (DataRow itm in dt.Rows)
                    {
                        list.Add(new SelectListItem()
                        {
                            Value = itm[0].ToString(),
                            Text = itm[1].ToString(),
                        });
                    }
                    if (withEmptyValue)
                    {
                        list.Add(new SelectListItem()
                        {
                            Value = "",
                            Text = "-Select Value-",
                        });
                    }

                }
                catch { }
                return new SelectList(list.OrderBy(x => x.Value), "Value", "Text");
            }


            public static List<Custom_List_Doc_Type> DocType(int from = 0, int to = 0)
            {
                List<Custom_List_Doc_Type> list = new List<Custom_List_Doc_Type>();
                try
                {
                    if (from == 0)
                    {
                        DataTable dt = Helper.LoadQuery("GET_MS_DOCTYPE");
                        foreach (DataRow itm in dt.Rows)
                        {
                            Custom_List_Doc_Type data = new Custom_List_Doc_Type();
                            data.Id = Convert.ToInt32(itm[0]);
                            data.Name = Convert.ToString(itm[1]);
                            data.Required = Convert.ToBoolean(itm[2]);
                            data.Secure = Convert.ToBoolean(itm[3]);
                            list.Add((Custom_List_Doc_Type)data);
                        }
                    }
                    else
                    {
                        DataTable dt = Helper.LoadQuery("GET_MS_DOCTYPE " + from + "," + to + "");
                        foreach (DataRow itm in dt.Rows)
                        {
                            Custom_List_Doc_Type data = new Custom_List_Doc_Type();
                            data.Id = Convert.ToInt32(itm[0]);
                            data.Name = Convert.ToString(itm[1]);
                            data.Required = Convert.ToBoolean(itm[2]);
                            data.Secure = Convert.ToBoolean(itm[3]);
                            list.Add((Custom_List_Doc_Type)data);
                        }
                    }

                }
                catch { }
                return list;
            }


            public static SelectList PortList(bool withEmptyValue = false, string portCode = "")
            {
                List<SelectListItem> list = new List<SelectListItem>();
                try
                {
                    DataTable dt = Helper.LoadQuery("GET_MS_PORT '" + portCode + "'");

                    foreach (DataRow itm in dt.Rows)
                    {
                        list.Add(new SelectListItem()
                        {
                            Value = itm[0].ToString(),
                            Text = itm[1].ToString(),
                        });
                    }
                    if (withEmptyValue)
                    {
                        list.Add(new SelectListItem()
                        {
                            Value = "",
                            Text = "-Select Value-",
                        });
                    }

                }
                catch { }
                return new SelectList(list.OrderBy(x => x.Value), "Value", "Text");
            }


            public static List<Custom_List_Doc_Type> DocTypeUpload(bool withEmptyValue = false, string id = "", string accountType = "")
            {
                List<Custom_List_Doc_Type> list = new List<Custom_List_Doc_Type>();
                try
                {
                    DataTable dt = Helper.LoadQuery("GET_REQUIRED_DOC '" + accountType + "', '" + id + "'");
                    foreach (DataRow itm in dt.Rows)
                    {
                        Custom_List_Doc_Type data = new Custom_List_Doc_Type();
                        data.Id = Convert.ToInt32(itm[0]);
                        data.Name = Convert.ToString(itm[1]);
                        data.Required = Convert.ToBoolean(itm[2]);
                        data.ContractNo = Convert.ToString(itm[3]);
                        if (itm[4] != DBNull.Value)
                            data.ContractDateString = Convert.ToString(itm[4]);
                        data.IsUpload = Convert.ToString(itm[6]);
                        list.Add((Custom_List_Doc_Type)data);
                    }
                }
                catch { }
                return list;
            }

            public static List<Custom_List_Doc_Type> DocType2(string seller, string shipper, string mode, string pid)
            {
                List<Custom_List_Doc_Type> list = new List<Custom_List_Doc_Type>();
                try
                {
                    DataTable dt = Helper.LoadQuery("GET_DOC_TYPE_SELLER_SHIPPER '" + seller + "','" + shipper + "','" + mode + "','" + pid + "'");
                    foreach (DataRow itm in dt.Rows)
                    {
                        Custom_List_Doc_Type data = new Custom_List_Doc_Type();
                        data.Id = Convert.ToInt32(itm[0]);
                        data.Name = Convert.ToString(itm[1]);
                        data.Required = Convert.ToBoolean(itm[2]);
                        list.Add((Custom_List_Doc_Type)data);
                    }

                }
                catch { }
                return list;
            }

            public static List<Custom_List_Chat> Chat(string uid, string user)
            {
                List<Custom_List_Chat> list = new List<Custom_List_Chat>();
                try
                {
                    DataTable dt = Helper.LoadQuery("GET_CHAT_DOC '" + uid + "','" + user + "'");
                    foreach (DataRow itm in dt.Rows)
                    {
                        Custom_List_Chat data = new Custom_List_Chat();
                        data.Id = Convert.ToInt32(itm["Uid"]);
                        data.Date = Convert.ToDateTime(itm["CreatedDate"]);
                        data.From = Convert.ToString(itm["CreatedUser"]);
                        data.Chat = Convert.ToString(itm["Remark"]);
                        data.Odd = Convert.ToString(itm["ODD"]);
                        data.Chat_ID = Convert.ToString(itm["CHAT_ID"]);
                        list.Add(data);
                    }

                    foreach(Custom_List_Chat itm in list)
                    {
                        Helper.ExecuteQuery("READ_CHAT '" + itm.Chat_ID+ "','" + user + "'");
                    }
                }
                catch { }
                return list;
            }

            public static List<Custom_List_Chat> ChatByGuid(string uid, string user)
            {
                List<Custom_List_Chat> list = new List<Custom_List_Chat>();
                try
                {
                    DataTable dt = Helper.LoadQuery("GET_CHAT_DOC_BY_GUID '" + uid + "','" + user + "'");
                    foreach (DataRow itm in dt.Rows)
                    {
                        Custom_List_Chat data = new Custom_List_Chat();
                        data.Id = Convert.ToInt32(itm["Uid"]);
                        data.Date = Convert.ToDateTime(itm["CreatedDate"]);
                        data.From = Convert.ToString(itm["CreatedUser"]);
                        data.Chat = Convert.ToString(itm["Remark"]);
                        data.Odd = Convert.ToString(itm["ODD"]);
                        data.Chat_ID = Convert.ToString(itm["CHAT_ID"]);
                        list.Add(data);
                    }

                    foreach (Custom_List_Chat itm in list)
                    {
                        Helper.ExecuteQuery("READ_CHAT '" + itm.Chat_ID + "','" + user + "'");
                    }
                }
                catch { }
                return list;
            }


            public static List<Custom_List_Notif> Notification(string AccountType, string user)
            {
                List<Custom_List_Notif> list = new List<Custom_List_Notif>();
                try
                {
                    DataTable dt = Helper.LoadQuery("GET_NOTIFICATION '" + AccountType + "','" + user + "'");
                    foreach (DataRow itm in dt.Rows)
                    {
                        Custom_List_Notif data = new Custom_List_Notif();
                        data.Message = Convert.ToString(itm[0]);
                        data.Date = Convert.ToString(itm[1]);
                        list.Add(data);
                    }
                }
                catch { }
                return list;
            }

            public static SelectList CurrencyList(bool withEmptyValue = false)
            {
                List<SelectListItem> list = new List<SelectListItem>();
                try
                {
                    DataTable dt = Helper.LoadQuery("GET_MS_CURR");

                    foreach (DataRow itm in dt.Rows)
                    {
                        list.Add(new SelectListItem()
                        {
                            Value = itm[0].ToString(),
                            Text = itm[0].ToString(),
                        });
                    }
                    if (withEmptyValue)
                    {
                        list.Add(new SelectListItem()
                        {
                            Value = "",
                            Text = "-Select Value-",
                        });
                    }

                }
                catch { }
                return new SelectList(list.OrderBy(x => x.Value), "Value", "Text");
            }

            public static SelectList CourierList(bool withEmptyValue = false, string id = "")
            {
                List<SelectListItem> list = new List<SelectListItem>();
                try
                {
                    DataTable dt = Helper.LoadQuery("GET_MS_COURIER '" + id + "'");
                    foreach (DataRow itm in dt.Rows)
                    {
                        list.Add(new SelectListItem()
                        {
                            Value = itm[0].ToString(),
                            Text = itm[1].ToString(),
                        });
                    }
                    if (withEmptyValue)
                    {
                        list.Add(new SelectListItem()
                        {
                            Value = "",
                            Text = "-Select Value-",
                        });
                    }
                }
                catch { }
                return new SelectList(list.OrderBy(x => x.Value), "Value", "Text");
            }

        }
    }
}