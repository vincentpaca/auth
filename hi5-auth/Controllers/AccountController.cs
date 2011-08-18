﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using hi5_auth.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System.Net;

using OAuth2.Mvc;

namespace hi5_auth.Controllers
{
    public class AccountController : Controller
    {
       
        //
        // GET: /Account/LogOn

        public ActionResult LogOn()
        {
            return View();
        }

        //
        // POST: /Account/LogOn

        [HttpPost]
        public ActionResult LogOn(string requestToken, LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var accessResponse = OAuthServiceBase.Instance.AccessToken(requestToken, "User", model.UserName, model.Password, true);
                MySqlConnection _con = new MySqlConnection("Server=localhost; Port=3307; Database=hi5; Uid=root; Pwd=imba;");
                string error = "";
                string username = "";
                try
                {
                    _con.Open();
                    MySqlCommand _command = new MySqlCommand("select username from users where username = '" + model.UserName + "' and " + "`password` = '" + model.Password + "';", _con);

                    try
                    {
                        username = _command.ExecuteScalar().ToString();
                    }
                    catch (Exception ex) { error = ex.Message; }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    _con.Close();
                }

                if (username != string.Empty)
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        var requestResponce = OAuthServiceBase.Instance.RequestToken();
                        return View("Success", accessResponse);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/LogOff

        public ActionResult LogOff()
        {
            Response.Cookies.Clear();
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                Membership.CreateUser(model.UserName, model.Password, model.Email, null, null, true, null, out createStatus);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, false /* createPersistentCookie */);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePassword

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        #region facebook authentication

        [HttpGet]
        [ActionName("Handshake")]
        public ActionResult Handshake(string token)
        {
            WebClient client = new WebClient();
            string JsonResult = client.DownloadString(string.Concat(
                   "https://graph.facebook.com/me?access_token=", token));

            JObject jsonUserInfo = JObject.Parse(JsonResult);

            string username = jsonUserInfo.Value<string>("email");
            string email = jsonUserInfo.Value<string>("email");
            string locale = jsonUserInfo.Value<string>("locale");
            int facebook_userID = jsonUserInfo.Value<int>("id");

            /**
             * we just made a "temporary" login by using facebook oauth
             * what I mean by temporary is that we haven't really saved any information about the user on our database.
             * so the following code just adds the user to the database using the data we got from Facebook
             **/

            //but we don't have a password yet, so we'll generate the user's password based on his username
            string password = username;

            // step 1, calculate MD5 hash from input
            //System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            //byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(password);
            //byte[] hash = md5.ComputeHash(inputBytes);

            //// step 2, convert byte array to hex string
            //System.Text.StringBuilder sb = new System.Text.StringBuilder();
            //for (int i = 0; i < hash.Length; i++)
            //{
            //    sb.Append(hash[i].ToString("X2"));
            //}

            //string hashedPassword = sb.ToString();

            //save the user now we have all the information we need
            //MembershipCreateStatus createStatus;
            //Membership.CreateUser(username, hashedPassword, email, null, null, true, null, out createStatus);
            MySqlConnection _con = new MySqlConnection("Server=localhost; Port=3307; Database=hi5; Uid=root; Pwd=imba;");
            string error = "";
            try
            {
                _con.Open();
                MySqlCommand _command = new MySqlCommand("select count(*) into @gwapo from users where username = '" + username + "' and " + "email = '" + email + "';" +
                "if @gwapo = 0 then " + 
                "insert into users set " +
                "username = '" + username + "'," +
                "`password` = '" + password + "'," +
                "email = '" + email + "';" +
                "end if;", _con);

                try
                {
                    _command.ExecuteScalar().ToString();
                }
                catch (Exception ex) { error = ex.Message; }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _con.Close();
            }



            /*if (createStatus == MembershipCreateStatus.Success)
            {
                //use the facebook email for our auth cookie
                FormsAuthentication.SetAuthCookie(email, false);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", ErrorCodeToString(createStatus));
                return RedirectToAction("LogOn", "Account");
            }*/

           
            if (!String.IsNullOrEmpty(email) && error!="")
            {
                FormsAuthentication.SetAuthCookie(email, false);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "gwapo ko");
                return RedirectToAction("LogOn", "Account");
            }
        }

        #endregion

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
