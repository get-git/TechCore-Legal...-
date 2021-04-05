using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proxemics2
{
    public partial class InternLogIn : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //-------------------------------------------------------------------------------------------------------
            //CHANGE this value to target TIKAL, LocalHost, or PROD, but DON"T FORGET TO CHANGE IT BACK before going to live
            //-------------------------------------------------------------------------------------------------------
            //string direct = "tikal";
            //string direct = "localhost";
            //string direct = "prod";
            string direct = "izapa";

            Boolean DebugMode = false;
            // 
            if (DebugMode != true)
            {

                //---------------------------------------------------------------------------------
                //AUTHENTICATION
                //---------------------------------------------------------------------------------
                Boolean WebAuthSuccess = false;

                // Let's check to see if the user has a cookie.
                HttpCookie TGCookie = new HttpCookie("IH");
                TGCookie = Request.Cookies["IH"];

                // Check to see if the cookie is empty
                if (TGCookie != null) // Yes, let them in!
                {
                    WebAuthSuccess = true;
                }
                else // No, but have they just successfully WebAuth'd? Let's find out...
                {
                    // Parse the URL
                    string ThisURL = HttpContext.Current.Request.RawUrl;
                    string QueryString = null;
                    string TicketToken = "null";
                    string TicketTokenResponse = "";

                    // Does the URL have tokenized parameters?
                    int num_TokenizedParameters = ThisURL.IndexOf('?');

                    if (num_TokenizedParameters >= 0) // Yes!, Let's get those parameters. 
                    {
                        QueryString = (num_TokenizedParameters < ThisURL.Length - 1) ? ThisURL.Substring(num_TokenizedParameters + 1) : String.Empty;
                        NameValueCollection qscoll = HttpUtility.ParseQueryString(QueryString);

                        //is one of them "ticket"? (that's the token being returned from WebAuth)
                        if (qscoll["ticket"] != null)
                        {
                            //OK, but is it a valid ticket value?
                            if (direct == "tikal")
                            {
                                TicketToken = "service=https://tikal.arizona.edu/InternLogIn.aspx&ticket=" + qscoll["ticket"].ToString();
                            }
                            else if (direct == "prod")
                            {
                                TicketToken = "service=https://techcore.arizona.edu/InternLogIn.aspx&ticket=" + qscoll["ticket"].ToString();
                            }
                            else if (direct == "localhost")
                            {
                                TicketToken = "service=http://localhost:56509/InternLogIn.aspx&ticket=" + qscoll["ticket"].ToString();
                            }
                            else if (direct == "izapa")
                            {
                                TicketToken = "service=https://proxemics.izapa.arizona.edu/InternLogIn.aspx&ticket=" + qscoll["ticket"].ToString();
                            }

                            //Validate the ticket with WebAuth
                            System.Net.WebClient wc = new System.Net.WebClient();
                            TicketTokenResponse = wc.DownloadString("https://webauth.arizona.edu/webauth/validate?" + TicketToken);

                            //Let's parse the TicketTokenResponse to see if it contains "yes" and a username
                            string[] SplitTicket = TicketTokenResponse.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                            if (SplitTicket[0].ToString() == "yes") //YES! This is an authenticated user, let's find out who they are and give them a MyOGI cookie
                            {
                                //Forge the Token
                                string Token = ForgeToken(8);
                                string ServerToken = ForgeToken(8);

                                //Get the NetID
                                string NetID = SplitTicket[1].ToString();

                                //Set the Cookie
                                HttpCookie SetTGCookie = new HttpCookie("IH");
                                SetTGCookie["Token"] = Token;
                                //SetMyOGICookie.Expires = DateTime.Now.AddHours(2); --default is "expire with end of sesssion"
                                Response.Cookies.Add(SetTGCookie);

                                //Write to Database
                                using (SqlConnection Conn = new SqlConnection(ConfigurationManager.AppSettings["DBConn"]))
                                {
                                    Conn.Open();
                                    SqlCommand Com = new SqlCommand("INSERT INTO Proxemics_UserSessions (UserId, Token, StartTime, ServerToken ) SELECT @UserId, @Token, GETDATE(), @ServerToken", Conn);
                                    Com.Parameters.Add(new SqlParameter("UserId", NetID));
                                    Com.Parameters.Add(new SqlParameter("Token", Token));
                                    Com.Parameters.Add(new SqlParameter("ServerToken", ServerToken));
                                    Com.ExecuteNonQuery();
                                }

                                //Session Maintenance - blow away any UserSessions older than 72 hours
                                using (SqlConnection Conn = new SqlConnection(ConfigurationManager.AppSettings["DBConn"]))
                                {
                                    Conn.Open();
                                    SqlCommand Com = new SqlCommand("DELETE Proxemics_UserSessions WHERE StartTime < GETDATE() - 72", Conn);
                                    Com.ExecuteNonQuery();
                                }

                                WebAuthSuccess = true;

                            }
                        }
                    }
                }

                if (WebAuthSuccess == false)
                {
                    if (direct == "tikal")
                    {
                        Response.Redirect("https://webauth.arizona.edu/webauth/login?service=https://tikal.arizona.edu/InternLogIn.aspx");
                    }
                    else if (direct == "prod")
                    {
                        Response.Redirect("https://webauth.arizona.edu/webauth/login?service=https://techcore.arizona.edu/InternLogIn.aspx");
                    }
                    else if (direct == "localhost")
                    {
                        Response.Redirect("https://webauth.arizona.edu/webauth/login?service=http://localhost:56509/InternLogIn.aspx");
                    }
                    else if (direct == "izapa")
                    {
                        Response.Redirect("https://webauth.arizona.edu/webauth/login?service=https://proxemics.izapa.arizona.edu/InternLogIn.aspx");
                    }

                }
                else // The user is completely authenticated
                {
                    HttpCookie TGCookie2 = new HttpCookie("IH");
                    TGCookie2 = Request.Cookies["IH"];
                    string Token2 = TGCookie2["Token"].ToString();

                    if (direct == "tikal")
                    {
                        using (SqlConnection Conn = new SqlConnection(ConfigurationManager.AppSettings["DBConn"]))
                        {
                            Conn.Open();
                            SqlCommand Com = new SqlCommand("SELECT UserId FROM Proxemics_UserSessions WHERE Token = @Token AND UserID IN (SELECT UserID FROM Proxemics_Interns)", Conn);
                            Com.Parameters.Add(new SqlParameter("Token", Token2));
                            SqlDataReader r = Com.ExecuteReader();
                            while (r.Read())
                            {
                                Response.Redirect("https://tikal.arizona.edu/Interns/AirFlowByUser.aspx");
                            }
                        }
                        Response.Redirect("https://tikal.arizona.edu/index.aspx");
                    }
                    else if (direct == "prod")
                    {
                        using (SqlConnection Conn = new SqlConnection(ConfigurationManager.AppSettings["DBConn"]))
                        {
                            Conn.Open();
                            SqlCommand Com = new SqlCommand("SELECT UserId FROM Proxemics_UserSessions WHERE Token = @Token AND UserID IN (SELECT UserID FROM Proxemics_Interns)", Conn);
                            Com.Parameters.Add(new SqlParameter("Token", Token2));
                            SqlDataReader r = Com.ExecuteReader();
                            while (r.Read())
                            {
                                Response.Redirect("https://techcore.arizona.edu/Interns/AirFlowByUser.aspx");
                            }
                        }
                        Response.Redirect("https://techcore.arizona.edu/index.aspx");
                    }
                    else if (direct == "localhost")
                    {
                        using (SqlConnection Conn = new SqlConnection(ConfigurationManager.AppSettings["DBConn"]))
                        {
                            Conn.Open();
                            SqlCommand Com = new SqlCommand("SELECT UserId FROM Proxemics_UserSessions WHERE Token = @Token AND UserID IN (SELECT UserID FROM Proxemics_Interns)", Conn);
                            Com.Parameters.Add(new SqlParameter("Token", Token2));
                            SqlDataReader r = Com.ExecuteReader();
                            while (r.Read())
                            {
                                Response.Redirect("http://localhost:56509/Interns/AirFlowByUser.aspx");
                            }
                        }
                        Response.Redirect("http://localhost:56509/index.aspx");
                    }
                    else if (direct == "izapa")
                    {
                        using (SqlConnection Conn = new SqlConnection(ConfigurationManager.AppSettings["DBConn"]))
                        {
                            Conn.Open();
                            SqlCommand Com = new SqlCommand("SELECT UserId FROM Proxemics_UserSessions WHERE Token = @Token AND UserID IN (SELECT UserID FROM Proxemics_Interns)", Conn);
                            Com.Parameters.Add(new SqlParameter("Token", Token2));
                            SqlDataReader r = Com.ExecuteReader();
                            while (r.Read())
                            {
                                Response.Redirect("https://proxemics.izapa.arizona.edu/Interns/AirFlowByUser.aspx");
                            }
                        }
                        Response.Redirect("https://proxemics.izapa.arizona.edu/index.aspx");
                    }
                }
            }



            else
            { // We are in debug mode.
            }

        }

        //----------------------------------------------------
        // GLOBAL FUNCTIONS
        //----------------------------------------------------
        public static string ForgeToken(int maxSize)
        {
            char[] chars = new char[62];
            chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            data = new byte[maxSize];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        public static void WriteLog(string Event, string Token)
        {
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.AppSettings["DBConn"]))
            {
                Conn.Open();
                SqlCommand Com = new SqlCommand("sp_WriteLog", Conn);
                Com.CommandType = CommandType.StoredProcedure;
                Com.Parameters.AddWithValue("Event", Event);
                Com.Parameters.AddWithValue("Token", Token);
                Com.ExecuteNonQuery();
            }
        }
    }
}