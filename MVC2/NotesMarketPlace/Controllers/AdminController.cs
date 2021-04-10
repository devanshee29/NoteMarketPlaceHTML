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
using PagedList;

namespace NotesMarketPlace.Controllers
{
    public class AdminController : Controller
    {
        private NotesMarketPlaceEntities2 db = new NotesMarketPlaceEntities2();

        // GET: FrontUser


        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            var v = db.Users.Where(a => a.EmailID == emailID).FirstOrDefault();
            return v != null;
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
                if (string.Compare(l.Password, Crypto.Decrypt(v.Password)) == 0)
                {
                    int timeout = l.RememberMe ? 525600 : 1000;
                    var ticket = new FormsAuthenticationTicket(l.EmailID, l.RememberMe, timeout);
                    string encrypted = FormsAuthentication.Encrypt(ticket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                    cookie.Expires = DateTime.Now.AddMinutes(timeout);
                    //   cookie.HttpOnly = true;
                    Response.Cookies.Add(cookie);
                    Session["EmailID"] = l.EmailID;
                    Session["ID"] = v.ID;


                    if (Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        int id = (int)Session["ID"];
                        return RedirectToAction("AddAdministrator", "Admin");
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
            return RedirectToAction("Login", "Admin");
        }


        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string EmailID)
        {
            string message = "";
            bool status = false;

            var account = db.Users.Where(a => a.EmailID == EmailID).FirstOrDefault();
            if (account != null)
            {
                String Password = Membership.GeneratePassword(12, 0);
                account.Password = Crypto.Hash(Password);
                db.Configuration.ValidateOnSaveEnabled = false;

                MailMessage mm = new MailMessage("notesmarketplacesrs@gmail.com", EmailID);

                mm.Subject = "Note Marketplace - Password";
                 mm.Body = "Hello " + account.FirstName + " " + account.LastName +
                                   ",<br><br>This is your Password for Notes MArketPlace account.<br><br>Password:"+Password+" <br/><br/>Regards,<br>Notes Marketplace";

                  mm.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;

                NetworkCredential nc = new NetworkCredential("notesmarketplacesrs@gmail.com", "NotesMP@123");
                smtp.EnableSsl = true;
                smtp.Credentials = nc;
                smtp.Send(mm);

                db.SaveChanges();
                ViewBag.Status = true;
            }
            else
            {
                message = "Account not found.";
            }
            return View();
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(Changepassword cp)
        {
            User u = new User();
            var email = Session["EmailID"];
            int id = (int)Session["ID"];
            var v = db.Users.Where(e => e.EmailID == (email) && e.ID == (id)).FirstOrDefault();

            cp.OldPassword = Crypto.Hash(cp.OldPassword);
            if (v.Password == cp.OldPassword)
            {
                if (cp.NewPassword == cp.ConfirmPassword)
                {
                    v.Password = Crypto.Hash(cp.NewPassword);
                    db.Configuration.ValidateOnSaveEnabled = false;

                    //db.Entry(v).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["msg"] = "<script>alert('Password has been changed Successfully');</script>";
                }
                else
                {
                    TempData["msg"] = "<script>alert('NewPassword and Confirm Password do not match');</script>";
                }
            }
            else
            {
                TempData["msg"] = "<script>alert('OldPassword and  NewPassword do not match');</script>";
            }
            return RedirectToAction("DashBoard");
        }

        [HttpGet]
        public ActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCategory(AddCategory ac)
        {
            if (ModelState.IsValid)
            {
                var v = db.NoteCategories.Where(a => a.Name == ac.CategoryName).FirstOrDefault();
                if (v == null)
                {
                    var addcategory = new NoteCategory
                    {
                        Name = ac.CategoryName,
                        Description = ac.Description,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    };
                    db.NoteCategories.Add(addcategory);
                    db.SaveChanges();
                }
            }
            return View();
        }


        [HttpGet]
        public ActionResult AddType()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddType(AddType at)
        {
            if (ModelState.IsValid)
            {
                var v = db.NoteTypes.Where(a => a.Name == at.TypeName).FirstOrDefault();
                if (v == null)
                {
                    var addtype = new NoteType
                    {
                        Name = at.TypeName,
                        Description = at.Description,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    };
                    db.NoteTypes.Add(addtype);
                    db.SaveChanges();
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult AddCountry()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCountry(AddCountry aco)
        {
            string msg = "";
            if (ModelState.IsValid)
            {
                var v = db.Countries.Where(a => a.Name == aco.CountryName).FirstOrDefault();
                if (v == null)
                {

                    var addcountry = new Country
                    {
                        Name = aco.CountryName,
                        CountryCode = aco.CountryCode,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    };
                    db.Countries.Add(addcountry);
                    db.SaveChanges();
                }
                else
                {
                    msg = "You already entered this Country!";
                }

            }
            ViewBag.Message = msg;
            return View();
        }


        [HttpGet]
        public ActionResult AddAdministrator()
        {
            ViewData["Country"] = db.Countries.ToList<Country>();
            return View();
        }

        [HttpPost]
        public ActionResult AddAdministrator(Addadministrator administrator)
        {
            if (ModelState.IsValid)
            {

                var isExist = IsEmailExist(administrator.EmailID);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(administrator);
                }
                String Password = Membership.GeneratePassword(12, 0);

                User admindata = new User
                {
                    FirstName = administrator.FirstName,
                    LastName = administrator.LastName,
                    EmailID = administrator.EmailID,
                    Password = Crypto.Hash(Password),
                    IsEmailVerified = true,
                    IsActive = true,
                    RoleID = 2,
                    CreatedDate = DateTime.Now,
                    ConfirmPassword = Crypto.Hash(Password)
                };
                db.Users.Add(admindata);

                string decryptPassword = Crypto.Decrypt(admindata.Password);

                MailMessage mm = new MailMessage("notesmarketplacesrs@gmail.com", administrator.EmailID);

                mm.Subject = "Note Marketplace - Password";
                mm.Body = "Hello " + administrator.FirstName + " " + administrator.LastName +
                                 ",<br><br>This is your Password for Notes MArketPlace account.<br><br>Password:" + Password + " <br/><br/>Regards,<br>Notes Marketplace";

                mm.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;

                NetworkCredential nc = new NetworkCredential("notesmarketplacesrs@gmail.com", "NotesMP@123");
                smtp.EnableSsl = true;
                smtp.Credentials = nc;
                smtp.Send(mm);

                var userprofile = new UserProfile
                {
                    Phone_Number = administrator.Phone_Number,
                    Phone_number_Country_Code = administrator.Phone_number_Country_Code,
                    Address_Line_1 = "XYZ",
                    Address_Line_2 = "ABC",
                    City = "ABC",
                    State = "ABC",
                    ZipCode = "123456",
                    Country = "ABC",
                    University = "ABC",
                    College = "ABC",
                    CreatedDate = DateTime.Now
                };
                db.UserProfiles.Add(userprofile);
                db.SaveChanges();
            }
            ModelState.Clear();
            return RedirectToAction("Login", "Admin");
        }

        [HttpGet]
        public ActionResult MyProfile()
        {
            ViewData["Country"] = db.Countries.ToList<Country>();
            return View();
        }

        [HttpPost]
        public ActionResult MyProfile(Myprofile myprofile, HttpPostedFileBase profile_Picture)
        {
            var userid = (int)Session["ID"];
            var filename = Path.GetFileName(profile_Picture.FileName);
            string path = Path.Combine(Server.MapPath("~/Uploadimg"), filename);
            profile_Picture.SaveAs(path);

            string name = Path.GetFileName(filename);

            if (ModelState.IsValid)
            {
                if (myprofile.EmailID != null)
                {
                    var user_profile = db.UserProfiles.Where(e => e.UserID == userid).FirstOrDefault();

                    user_profile.SecondaryEmailAddress = myprofile.SecondaryEmailAddress;
                    user_profile.Profile_Picture = name;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Login", "Admin");
        }


        [HttpGet]
        public ActionResult NoteDetails(string id)
        {

            List<SellerNote> notes = db.SellerNotes.Where(e => e.IsActive == true).ToList();
            var v = db.SellerNotes.Where(e => e.ID.ToString().Equals(id)).FirstOrDefault();
            if (v != null)
            {
                var sellerid = v.SellerID;
                bool isvalid = db.Downloads.Any(m => m.NoteID.ToString().Equals(id) && m.Seller == sellerid && m.IsSellerHasAllowedDownload);
                if (isvalid)
                {
                    ViewBag.valid = "true";
                }

                ViewBag.Display_Picture = v.Display_Picture;
                ViewBag.Title = v.Title;
                ViewBag.Description = v.Description;
                ViewBag.IsPaid = v.IsPaid;
                ViewBag.University_Name = v.University_Name;
                ViewBag.Course = v.Course;
                ViewBag.CourseCode = v.CourseCode;
                ViewBag.Professor = v.Professor;
                ViewBag.Number_Of_Pages = v.Number_Of_Pages;
                ViewBag.SellingPrice = v.SellingPrice;
                ViewBag.NotesPreview = v.NotesPreview;
                ViewBag.ID = v.ID;
                String s = v.CreatedDate.ToString();

                DateTime d = new DateTime();
                d = DateTime.ParseExact(s, "dd-MM-yyyy HH:mm:ss", null);
                String s1 = d.ToString("MMMM dd yyyy");
                ViewBag.s1 = s1;

                List<SellerNotesReview> reviews = db.SellerNotesReviews.Where(e => e.NoteID == v.ID).ToList();

                var country = db.Countries.Where(e => e.ID == v.Country).FirstOrDefault();
                var category = db.NoteCategories.Where(e => e.ID == v.Category).FirstOrDefault();
                var attachment = db.SellerNotesAttachments.Where(e => e.NoteID.ToString().Equals(id)).FirstOrDefault();
                ViewBag.path = attachment.FilePath;
                ViewBag.Category = category.Name;
                ViewBag.Country = country.Name;
                List<SellerNotesReview> ratings = db.SellerNotesReviews.Where(e => e.NoteID == v.ID).ToList();
                return View(notes);
            }
            else
            {
                return HttpNotFound();
            }
        }

        [HttpGet]
        public ActionResult MemberDetails(string id )
        {
            User user = db.Users.Where(e => e.ID.ToString().Equals(id) && e.IsActive).FirstOrDefault();
            UserProfile userprofile = db.UserProfiles.Where(e => e.UserID.ToString().Equals(id)).FirstOrDefault();
            List<SellerNote> seller = db.SellerNotes.Where(e => e.SellerID.ToString().Equals(id) && e.Status!=0).ToList();
            List<ReferenceData> data = db.ReferenceDatas.ToList();
            List<NoteCategory> categories = db.NoteCategories.ToList();
            List<SellerNotesAttachment> attachments = db.SellerNotesAttachments.ToList();
            var member_detail = from s in seller
                                join c in categories on s.Category equals c.ID into T1
                                from c in T1
                                join d in data on s.Status equals d.ID into T2
                                from d in T2
                                join a in attachments on s.ID equals a.NoteID into T3
                                from a in T3
                                select new Memberdetails
                                {
                                    note = s,
                                    category = c,
                                    status = d,
                                    attachment = a
                                };

            foreach (SellerNote s in seller)
            {
                List<Download> d = db.Downloads.Where(e => e.NoteID == s.ID && e.IsSellerHasAllowedDownload == true && e.IsAttachmentDownloaded == true).ToList();

                
                string ids = Convert.ToString(s.ID);
                TempData[ids] = d.Count;
                decimal money = 0;
                foreach (var i in d)
                {
                    if (i.IsPaid)
                    {
                        money += Convert.ToDecimal(i.PurchasedPrice);
                    }
                }
                TempData[ids + " earned"] = money;
            }
            var model = new Memberbasicdetail
            {
                user = user,
                userprofile = userprofile,
                memberdetails = member_detail
            };
            return View(model);
        }

        [HttpGet]
        public ActionResult Member(int? i, string search)
        {
             List<User> user = db.Users.Where(e => e.RoleID==1 && e.IsActive).ToList();
           
            foreach (User u in user)
            {
                List<SellerNote> inReview = db.SellerNotes.Where(e => e.SellerID == u.ID && e.Status == 10).ToList();
                List<SellerNote> Publish = db.SellerNotes.Where(e => e.SellerID == u.ID && e.Status == 11).ToList();
                List<Download> Download = db.Downloads.Where(e => e.Seller == u.ID && e.IsSellerHasAllowedDownload == true && e.IsAttachmentDownloaded == true).ToList();
                List<Download> expense = db.Downloads.Where(e => e.Downloader == u.ID && e.IsSellerHasAllowedDownload == true && e.IsAttachmentDownloaded == true).ToList();
                string id1 = Convert.ToString(u.ID);
                TempData[id1] = inReview.Count;
                TempData[id1+" publish"] = Publish.Count;
                TempData[id1+" download"] = Download.Count;
                decimal money = 0;
                decimal expenses = 0;
                foreach (var j in Download)
                {
                    if (j.IsPaid)
                    {
                        money += Convert.ToDecimal(j.PurchasedPrice);
                    }
                }
                foreach (var k in expense)
                {
                    if (k.IsPaid)
                    {
                        expenses += Convert.ToDecimal(k.PurchasedPrice);
                    }
                }
                TempData[id1 + " earned"] = money;
                TempData[id1 + " expenses"] = expenses;
            }

            return View(user.ToPagedList(i ?? 1, 5));
        }
            
        [HttpGet]
        public ActionResult ManageAdministrator(int? i, string search)
        {
            List<User> user = db.Users.Where(e => e.RoleID == 2).ToList();
            
            if (!String.IsNullOrEmpty(search))
            {
                user = user.Where(e => e.FirstName.ToLower().Contains(search.ToLower())).ToList();
                if (user == null)
                {
                    user = user.Where(e => e.LastName.ToLower().Contains(search.ToLower())).ToList();
                    if(user==null)
                    {
                        user = user.Where(e => e.EmailID.ToLower().Contains(search.ToLower())).ToList();
                    }
                }

            }
            return View(user.ToPagedList(i ?? 1, 5));
        }

        public ActionResult DeleteAdmin(int id)
        {
            User u = db.Users.Where(e => e.ID==id).FirstOrDefault();
            UserProfile up = db.UserProfiles.Where(e => e.UserID == id).FirstOrDefault();
            db.UserProfiles.Remove(up);
            db.Users.Remove(u);
            db.SaveChanges();
            return RedirectToAction("ManageAdministrator","Admin");
        }

        [HttpGet]
        public ActionResult NotesUnderReview(int? i,string search,string seller)
        {
            List<SellerNote> notes = db.SellerNotes.Where(e => e.IsActive == true && (e.Status==7 || e.Status==10)).ToList();
            ViewData["Category"] = db.NoteCategories.ToList<NoteCategory>();
            ViewData["Status"] = db.ReferenceDatas.ToList<ReferenceData>();
            ViewData["User"] = db.Users.ToList<User>();
            
            if (!String.IsNullOrEmpty(search))
            {
                notes = notes.Where(e => e.Title.ToLower().Contains(search.ToLower())).ToList();
            }
            if (seller != "Seller" && !String.IsNullOrEmpty(seller))
            {
                notes = notes.Where(e => e.SellerID == Convert.ToInt32(seller)).ToList();
            }
            return View(notes.ToPagedList(i ?? 1, 5));
        }

      
        public ActionResult Approve(string id)
        {
            SellerNote v = db.SellerNotes.Where(e => e.ID.ToString().Equals(id)).FirstOrDefault();
            v.Status = 11;
            v.ActionedBy = (int)Session["ID"];
            db.SaveChanges();
            return RedirectToAction("NotesUnderReview", "Admin");
        }
        public ActionResult Reject(string id)
        {
            SellerNote v = db.SellerNotes.Where(e => e.ID.ToString().Equals(id)).FirstOrDefault();
            v.Status = 12;
            v.ActionedBy = (int)Session["ID"];
            db.SaveChanges();
            return RedirectToAction("NotesUnderReview", "Admin");
        }
        public ActionResult InReview(string id)
        {
            SellerNote v = db.SellerNotes.Where(e => e.ID.ToString().Equals(id)).FirstOrDefault();
            v.Status = 10;
            v.ActionedBy = (int)Session["ID"];
            db.SaveChanges();
            return RedirectToAction("NotesUnderReview", "Admin");
        }

        [HttpGet]
        public ActionResult PublishNote()
        {
            List<SellerNote> notes = db.SellerNotes.Where(e => e.IsActive == true && e.Status == 11).ToList();
            ViewData["Category"] = db.NoteCategories.ToList<NoteCategory>();
            ViewData["User"] = db.Users.ToList<User>();
            return View(notes);
        }

    }
}