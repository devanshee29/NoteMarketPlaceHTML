using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NotesMarketPlace.Models;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.Web.Security;

namespace NotesMarketPlace.Controllers
{
    public class AdminController : Controller
    {
        private NotesMarketPlaceEntities2 db = new NotesMarketPlaceEntities2();

        // GET: FrontUser

        [HttpGet]
        public ActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Signup([Bind(Exclude = "IsEmailVerified,ActivationCode")] User u)
        {
            bool Status = false;
            string message = "";
            if (ModelState.IsValid)
            {

                var isExist = IsEmailExist(u.EmailID);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(u);
                }

                u.ActivationCode = Guid.NewGuid();

                u.Password = Crypto.Hash(u.Password);
                u.ConfirmPassword = Crypto.Hash(u.ConfirmPassword);

                u.IsEmailVerified = false;

                u.RoleID = 2;
                u.IsActive = true;
                db.Users.Add(u);
                db.SaveChanges();


                //send Email to User
                SendVerificationLinkEmail(u.EmailID, u.ActivationCode.ToString(), u.FirstName, u.LastName, u.Password, "EmailVerification");
                message = "Account activation link" +
                    " has been sent to your email id";
                Status = true;

            }

            else
            {
                message = "Invalid Request";
            }
            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(u);
        }



        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            var v = db.Users.Where(a => a.EmailID == emailID).FirstOrDefault();
            return v != null;
        }

        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode, string firstName, string lastName, string password, string emailFor = "EmailVerification")
        {
            var verifyUrl = "/RegisteredUser/" + emailFor + "/ " + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
            //var link = "https://localhost:44379/" + verifyUrl;

            MailMessage mm = new MailMessage("notesmarketplacesrs@gmail.com", emailID);

            if (emailFor == "EmailVerification")
            {

                mm.Subject = "Note Marketplace -Email Verification";
                mm.Body = "Hello " + firstName + " " + lastName +
                                 ",<br><br>Thank you for signing up with us. Please click on below link to verify your email address and to do login." +
                                 "<br/><br/><a href='" + link + "'> " + link + "</a><br/><br/>Regards,<br>Notes Marketplace";
            }
            else if (emailFor == "GeneratePassword")
            {

                mm.Subject = " New Temporary Password has been created for you";
                mm.Body = "Hello" +
                                  ",<br><br>We have generated a new password for you" +
                                  "<br><br>Password:" + password +
                                "<br><br>Regards,<br>Notes Marketplace";

            }
            mm.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.UseDefaultCredentials = false;

            NetworkCredential nc = new NetworkCredential("notesmarketplacesrs@gmail.com", "NotesMP@123");
            smtp.EnableSsl = true;
            smtp.Credentials = nc;
            smtp.Send(mm);
        }



        [HttpGet]
        public ActionResult Login()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin l, string ReturnUrl)
        {
            string message = "";
            var v = db.Users.Where(a => a.EmailID == l.EmailID).FirstOrDefault();
            if (v != null)
            {
                if (string.Compare(l.Password, v.Password) == 0)
                {
                    int timeout = l.RememberMe ? 525600 : 1000;
                    var ticket = new FormsAuthenticationTicket(l.EmailID, l.RememberMe, timeout);
                    string encrypted = FormsAuthentication.Encrypt(ticket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                    cookie.Expires = DateTime.Now.AddMinutes(timeout);
                    //   cookie.HttpOnly = true;
                    Response.Cookies.Add(cookie);

                    if (Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("DashBoard");
                    }

                }
                else
                {
                    message = "This password that you've enterd is incorrect.";
                }
            }
            else
            {
                message = "Invalid credential provided.";
            }

            ViewBag.Message = message;
            return View();
        }


        [Authorize]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "RegisteredUser");
        }


        [HttpGet]
        public ActionResult EmailVerification(string id)
        {
            bool Status = false;
            db.Configuration.ValidateOnSaveEnabled = false;
            User v = db.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
            if (v != null)
            {
                v.IsEmailVerified = true;
                db.SaveChanges();
                Status = true;
            }
            else
            {
                ViewBag.Message = "Invalid Request";
            }
            ViewBag.Status = true;
            return View(v);
        }


        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string EmailID, User u)
        {
            string message = "";
            bool status = false;

            var account = db.Users.Where(a => a.EmailID == EmailID).FirstOrDefault();
            if (account != null)
            {
                String Password = Membership.GeneratePassword(12, 0);
                account.Password = Crypto.Hash(Password);
                db.Configuration.ValidateOnSaveEnabled = false;
                db.SaveChanges();
                SendVerificationLinkEmail(account.EmailID, null, null, null, Password, "GeneratePassword");

                ViewBag.Status = true;
            }
            else
            {
                message = "Account not found.";
            }
            return View();
        }


    }
}