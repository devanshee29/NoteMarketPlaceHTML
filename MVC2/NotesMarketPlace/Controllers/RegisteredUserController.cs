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
using System.Runtime.Remoting.Contexts;

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
                if (string.Compare(Crypto.Hash(l.Password), v.Password) == 0)
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
            TempData.Remove("EmailID");
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
        public ActionResult Home()
        {
            return View();
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
                types = GetNoteTypes(),
                categories = GetNoteCategories(),
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
                    SellerID = (int)Session["ID"],
                    IsActive=true
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
                flag = true;
            }
            ModelState.Clear();
            return RedirectToAction("AddNote");
        }

        public ActionResult FAQ()
        {
            return View();
        }


        [HttpGet]
        public ActionResult SearchNotes(int? i, string search,string country,string type,string category,string university,string course)
        {
           List<SellerNote> notes= db.SellerNotes.Where(e => e.IsActive == true).ToList();
           ViewData["Country"] = db.Countries.ToList<Country>();
           ViewData["Type"] = db.NoteTypes.ToList<NoteType>();
           ViewData["Category"] = db.NoteCategories.ToList<NoteCategory>();
           ViewData["University"] = db.SellerNotes.Where(e=>e.University_Name!=null).Select(e=>e.University_Name).Distinct().ToList();
           ViewData["Course"] = db.SellerNotes.Where(e => e.Course != null).Select(e => e.Course).Distinct().ToList();



            if (!String.IsNullOrEmpty(search))
            {
                notes = notes.Where(e => e.Title.ToLower().Contains(search.ToLower())).ToList();
            }
            if(country!="Select country" && !String.IsNullOrEmpty(country))
            {
                notes = notes.Where(e => e.Country==Convert.ToInt32(country)).ToList();
            }
            if (type != "Select type" && !String.IsNullOrEmpty(type))
            {
                notes = notes.Where(e => e.NoteType == Convert.ToInt32(type)).ToList();
            }
            if (country != "Select category" && !String.IsNullOrEmpty(category))
            {
                notes = notes.Where(e => e.Category == Convert.ToInt32(category)).ToList();
            }
            if (university != "Select university" && !String.IsNullOrEmpty(university))
            {
                notes = notes.Where(e => e.University_Name!=null && e.University_Name.ToLower().Contains(university.ToLower())).ToList();
            }
            if (course != "Select course" && !String.IsNullOrEmpty(course))
            {
                notes = notes.Where(e => e.Course!= null && e.Course.ToLower().Contains(course.ToLower())).ToList();
            }

            ViewBag.Count = notes.Count;
            return View(notes.ToPagedList(i ?? 1,9));

            
        }

        [HttpGet]
        public ActionResult UserProfile()
        {
            ViewData["Country"] = db.Countries.ToList<Country>();
            ViewData["ReferenceData"] = db.ReferenceDatas.ToList<ReferenceData>();
            return View();
        }

        
       
        [HttpPost] 
        public ActionResult UserProfile( Userprofile u, HttpPostedFileBase profile_Picture)
        {
            ViewBag.show = false;

            //Store Display picture in database
            var filename = Path.GetFileName(profile_Picture.FileName);
            string path = Path.Combine(Server.MapPath("~/Uploadimg"), filename);
            profile_Picture.SaveAs(path);

            string name = Path.GetFileName(filename);

            if (ModelState.IsValid)
            {
                var v = db.Users.Where(e => e.FirstName == u.FirstName && e.LastName==u.LastName).FirstOrDefault();
                if (v!=null)
                {
                  
                    var userprofile = new UserProfile
                    {
                   
                     UserID= (int)Session["ID"],
                    DOB=u.DOB,
                    Gender=u.Gender,
                    SecondaryEmailAddress=u.SecondaryEmailAddress,
                    Phone_number_Country_Code=u.Phone_number_Country_Code,
                    Phone_Number=u.Phone_Number,
                    Profile_Picture=name,
                    Address_Line_1=u.Address_Line_1,
                    Address_Line_2=u.Address_Line_2,
                    City=u.City,
                    State=u.State,
                    ZipCode=u.ZipCode,
                    Country=u.Country,
                    University=u.University,
                    College=u.College,
                    CreatedDate=DateTime.Now
                };
                
                db.UserProfiles.Add(userprofile);
                db.SaveChanges();
                }

            }
            ModelState.Clear();
            return RedirectToAction("DashBoard");
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
            String email = TempData["EmailID"].ToString();
            int id = int.Parse(TempData["ID"].ToString());
            var v = db.Users.Where(e => e.EmailID == (email) && e.ID == (id)).FirstOrDefault();

            cp.OldPassword = Crypto.Hash(cp.OldPassword);
            if (v.Password==cp.OldPassword)
            {
                if(cp.NewPassword==cp.ConfirmPassword)
                {
                    v.Password = Crypto.Hash(cp.NewPassword);
                    db.Configuration.ValidateOnSaveEnabled = false;
                    
                    //db.Entry(v).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["msg"]="<script>alert('Password has been changed Successfully');</script>";
                }
                else
                {
                    TempData["msg"] = "<script>alert('NewPassword and Confirm Password do not match');</script>";
                }
            }
            else { 
            TempData["msg"] = "<script>alert('OldPassword and  NewPassword do not match');</script>";
            }
            return RedirectToAction("DashBoard");
        }


        
       [HttpGet]
        public ActionResult NoteDetails(string noteid)
        {
            
            List<SellerNote> notes = db.SellerNotes.Where(e => e.IsActive == true).ToList();
            SellerNote v = db.SellerNotes.FirstOrDefault(e => e.ID.ToString().Equals(noteid));
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
            /*int counts = 0;
            if(reviews.Count!=0)
            {
                decimal rating = 0;
                ViewBag.counts = counts;
                foreach(var review in reviews)
                {
                    User u = db.Users.Where(e => e.ID == review.ReviewedByID).FirstOrDefault();
                    UserProfile up = db.UserProfiles.Where(e => e.UserID == u.ID).FirstOrDefault();
                    ViewBag.Profile_Picture[counts] = up.Profile_Picture;
                    ViewBag.FullName[counts] = u.FirstName + " " + u.LastName;
                    ViewBag.Ratings[counts] = review.Ratings * 2;
                    ViewBag.Comments[counts] = review.Comments;

                    rating = rating + review.Ratings;
                    counts += 1;
                }
                decimal rate = Math.Round(rating * 2 / counts);
                ViewBag.rate = rate;
                
            }
            ViewBag.count = counts;*/

            var country = db.Countries.Where(e => e.ID == v.Country).FirstOrDefault();
            var category = db.NoteCategories.Where(e => e.ID == v.Category).FirstOrDefault();
            ViewBag.Category = category.Name;
            ViewBag.Country = country.Name;
            List<SellerNotesReview> ratings = db.SellerNotesReviews.Where(e => e.NoteID == v.ID).ToList();
            return View(notes);
        }

        [HttpPost]
        public ActionResult FreeDownload(int id)
        {
            SellerNote s = db.SellerNotes.Where(e => e.ID == id).FirstOrDefault();
            SellerNotesAttachment a = db.SellerNotesAttachments.Where(e => e.NoteID == id).FirstOrDefault();
            var category = db.NoteCategories.Where(e => e.ID == s.ID).FirstOrDefault();
            Download d = new Download()
            {
                NoteID = id,
                Seller = s.SellerID,
                Downloader = (int)Session["ID"],
                IsSellerHasAllowedDownload = true,
                AttachmentPath = a.FilePath + "/" + a.FileName,
                IsAttachmentDownloaded = true,
                AttachmentDownloadedDate = DateTime.Now,
                IsPaid = false,
                PurchasedPrice = 0,
                NoteTitle = s.Title,
                NoteCategory = category.Name

             };
            db.Downloads.Add(d);
            db.SaveChanges();
            return RedirectToAction("DashBoard");
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