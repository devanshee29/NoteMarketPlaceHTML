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
using System.IO;

namespace NotesMarketPlace.Controllers
{
    public class RegisteredUserController : Controller
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

                u.RoleID = 1;
                u.IsActive = true;
                db.Users.Add(u);
                db.SaveChanges();


                //send Email to User
                SendVerificationLinkEmail(u.EmailID, u.ActivationCode.ToString(), u.FirstName, u.LastName,u.Password, "EmailVerification");
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
        public void SendVerificationLinkEmail(string emailID, string activationCode, string firstName, string lastName, string password,string emailFor = "EmailVerification")
        {
            var verifyUrl = "/RegisteredUser/" + emailFor + "/ " + activationCode;
             var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
            //var link = "https://localhost:44379/" + verifyUrl;

            MailMessage mm = new MailMessage("notesmarketplacesrs@gmail.com", emailID);

            if (emailFor == "EmailVerification")
            {

                mm.Subject = "Note Marketplace -Email Verification";
                mm.Body = "Hello " + firstName + " " + lastName +
                                 ",<br><br>Thank you for signing up with us. Please click on below link to verify your email address and to do login."+
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
                SendVerificationLinkEmail(account.EmailID, null, null, null,Password, "GeneratePassword") ;
               
                ViewBag.Status = true;
            }
            else
            {
                message = "Account not found.";
            }
            return View();
        }


        [HttpGet]
        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Contact(ContactUs c)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    MailMessage msz = new MailMessage();
                    msz.From = new MailAddress(c.EmailID);     //Email which you are getting 
                                                               //from contact us page 
                    msz.To.Add("dwiti1602@gmail.com");     //Where mail will be sent 
                    msz.Subject = c.FullName + " - " + c.Subject;
                    msz.Body = "Hello,<br><br>" + c.Comment + "<br><br>Regards,<br>" + c.FullName;
                    msz.IsBodyHtml = true;


                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;


                    NetworkCredential nc = new NetworkCredential("notesmarketplacesrs@gmail.com", "NotesMP@123");
                    smtp.EnableSsl = true;
                    smtp.Credentials = nc;
                    smtp.Send(msz);

                    ModelState.Clear();
                    ViewBag.Message = "Thank you for Contacting us ";
                }
                catch (Exception ex)
                {
                    ModelState.Clear();
                    ViewBag.Message = $" Sorry we are facing Problem here {ex.Message}";
                }
            }

            return View();
        }
        public ActionResult Error()
        {
            return View();
        }


        [HttpGet]
        public ActionResult AddNote()
        {
            AddNote a = new AddNote
            {
                types=GetNoteTypes(),
                categories=GetNoteCategories(),
                countries=GetCountryList()
            };
            return View(a);
        }
        public List<NoteType> GetNoteTypes()
        {
            List<NoteType> noteTypes = db.NoteTypes.ToList();
            return noteTypes;
        }
        public List<NoteCategory> GetNoteCategories()
        {
            List<NoteCategory> noteCategories = db.NoteCategories.ToList();
            return noteCategories;
        }
        public List<Country> GetCountryList()
        {
            List<Country> countries = db.Countries.ToList();
            return countries;
        }


        [HttpPost]
        public ActionResult AddNote(AddNote a, HttpPostedFileBase display_Picture, HttpPostedFileBase fileName, HttpPostedFileBase notesPreview)
        {
            
            
            ViewBag.show = false;

            //Store Display picture in database
           var filename = Path.GetFileName(display_Picture.FileName);
            string path = Path.Combine(Server.MapPath("~/Uploadimg"), filename);
            display_Picture.SaveAs(path);

            //Store preview picture in database
            var preview_img = Path.GetFileName(notesPreview.FileName);
            string preview_path = Path.Combine(Server.MapPath("~/Uploadimg"), preview_img);
            notesPreview.SaveAs(preview_path);

            //Store Notes(pdf_form) in database
            var notes_pdf = Path.GetFileName(fileName.FileName);
            string notes_pdf_path = Path.Combine(Server.MapPath("~/Uploadimg"), notes_pdf);
            fileName.SaveAs(notes_pdf_path);

            string name = Path.GetFileName(filename);
            string preview_img_name = Path.GetFileName(preview_img);
            string upload_note_name = Path.GetFileName(notes_pdf);

           /* if (a.SellFor == "Free")
            {
                model.IsPaid = false;
            }
            if (model.sellfor == "Paid")
            {
                model.IsPaid = true;
            }*/

            bool flag = false;
            
            if (ModelState.IsValid)
            {
                var sellernote = new SellerNote
                {
                    Title = a.Title,
                    Category = a.Category,
                    NoteType = a.NoteType,
                    Status = 0,
                   Display_Picture = name,
                    Number_Of_Pages = a.Number_Of_Pages,
                    Description = a.Description,
                    University_Name = a.University_Name,
                    Country = a.Country,
                    Course = a.Course,
                    CourseCode = a.CourseCode,
                    Professor = a.Professor,
                    IsPaid = a.IsPaid,
                    SellingPrice = a.SellingPrice,
                    CreatedDate = DateTime.Now,
                   NotesPreview = preview_img_name,
                    SellerID = 4
                };
                var sellernotesattachments = new SellerNotesAttachment
                {
                   FileName = upload_note_name,
                  FilePath = notes_pdf_path,
                    CreatedDate = DateTime.Now
                };

                db.SellerNotes.Add(sellernote);
                db.SellerNotesAttachments.Add(sellernotesattachments);
                db.SaveChanges();
                //ViewBag.show = true;
                flag = true;
            }
            ModelState.Clear();
            return RedirectToAction("AddNote");
        }

        public ActionResult FAQ()
        {
            return View();
        }

        public ActionResult DashBoard()
        {
            return View();
        }

        public ActionResult buyerRequests()
        {
            return View();
        }

    }
}