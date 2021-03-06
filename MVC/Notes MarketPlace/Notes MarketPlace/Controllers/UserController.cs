﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Notes_MarketPlace.Models;
using PagedList;

namespace Notes_MarketPlace.Controllers
{
    public class UserController : Controller
    {
        Notes_MarketPlaceEntities db = new Notes_MarketPlaceEntities();
        static int userid = 0;
        string defaultProfileImg;
        string defaultBookImg;

        public ActionResult home()
        {
            if(userid != 0)
            {
                ViewBag.valid = true;
            }
            List<SystemConfiguration> list = db.SystemConfigurations.ToList();
            defaultProfileImg = list.Where(l => l.Key == "DefaultProfileImg").FirstOrDefault().Value;
            defaultBookImg = list.Where(l => l.Key == "DefaultNoteImg").FirstOrDefault().Value;
            return View("Home");
        }


        [HttpGet]
        public ActionResult signup()
        {
            return View();
        }

        [HttpPost]
        public ActionResult signup(User user)
        {
            if(db.Users.Any( x => x.EmailID == user.EmailID))
            {
                ViewBag.DuplicateMsg = "Email ID already exist.";
                return View("signup", user);
            }

            if (user.Password != user.ConfirmPassword)
            {
                return View("signup", user);
            }

            user.RoleID = 1;
            user.IsActive = true;
            user.IsEmailVerified = false;
            user.CreatedDate = DateTime.Now;
            user.ModifiedDate = DateTime.Now;
            db.Users.Add(user);

            try
            {
                db.SaveChanges();
                ViewBag.SuccessMsg = "Your account has been successfully created. Please verify your email to complete your registration.";

                MailMessage mail = new MailMessage("poojapatel102938@gmail.com", user.EmailID.ToString());
                mail.Subject = "Notes MarketPlace - Email Verification";
                string Body = "Dear " + user.FirstName + " " + user.LastName + ",<br><br> Thank you for signing up with Notes MarketPlace. Please click on below link to verify your email http://localhost:52734/User/emailVerification?emailid="+user.EmailID;
                mail.Body = Body;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;

                NetworkCredential nc = new NetworkCredential("poojapatel102938@gmail.com", "Pooja102938");
                smtp.EnableSsl = true;
                smtp.Credentials = nc;
                smtp.Send(mail);

                return View("signup");
            }
            catch(Exception e)
            {
                return View("signup", user);
            }
        }

        [HttpGet]
        public ActionResult emailVerification(string emailid)
        {
            User user = db.Users.FirstOrDefault(u => u.EmailID.Equals(emailid));
            try
            {
                if(user != null)
                {
                    if (user.IsEmailVerified == true)
                    {
                        ViewBag.Msg = "Your email is already verified";
                    }
                    return View(user);
                }
                return HttpNotFound();
            }
            catch(Exception e)
            {
                return HttpNotFound();
            }
        }

        [HttpPost]
        public ActionResult emailVerification(int ID)
        {
           // try
            {
                User user = db.Users.FirstOrDefault(u => u.ID == ID);
                if(user != null)
                {
                    user.IsEmailVerified = true;
                    user.ConfirmPassword = user.Password;
                    db.SaveChanges();
                    return RedirectToAction("login");
                }
                return HttpNotFound();
            }
          //  catch(Exception e)
            {
                return HttpNotFound();
            }
        }

        [HttpGet]
        public ActionResult forgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult forgotPassword(User user)
        {
            var valid = db.Users.Where(v => v.EmailID.Equals(user.EmailID)).FirstOrDefault();
            if (valid != null)
            {
                ViewBag.EmailSent = "Email has been sent to you with your temporary password.";
                try
                {
                    db.SaveChanges();
                    ViewBag.SuccessMsg = "Your account has been successfully created. Please verify your email to complete your registration.";

                    MailMessage mail = new MailMessage("poojapatel102938@gmail.com", user.EmailID.ToString());
                    mail.Subject = "New Temporary Password has been created for you";
                    string Body = "Hello,<br><br>We have generated a new password for you <br>Password : " + valid.Password + "<br><br>Regards,<br>Notes Marketplace";
                    mail.Body = Body;
                    mail.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;

                    NetworkCredential nc = new NetworkCredential("poojapatel102938@gmail.com", "Pooja102938");
                    smtp.EnableSsl = true;
                    smtp.Credentials = nc;
                    smtp.Send(mail);

                    return View();
                }
                catch (Exception e)
                {
                    return View(user);
                }
            }
            else
            {
                ViewBag.ErrorMsg = "Email doesn't exist.";
                return View();
            }
        }

        [HttpGet]
        public ActionResult changePassword()
        {
            if (userid != 0)
            {
                return View();
            }
            else
            {
                return RedirectToAction("login");
            }
        }

        [HttpPost]
        public ActionResult changePassword(changePassword cp)
        {
            User user = db.Users.FirstOrDefault(u => u.ID == userid);
            if(cp.OldPassword != user.Password)
            {
                ViewBag.ErrorMsg = "Old Password is incorrect.";
                return View(cp);
            }
            if (cp.OldPassword == cp.NewPassword)
            {
                ViewBag.ErrorMsg = "Old Password and New Password shouldn't be the same.";
                return View(cp);
            }
            if (cp.NewPassword != cp.ConfirmNewPassword)
            {
                return View(cp);
            }
            user.Password = cp.NewPassword;
            user.ConfirmPassword = user.Password;
            db.SaveChanges();
            return RedirectToAction("login");
        }

        [HttpGet]
        public ActionResult login()
        {
            User user = new User();
            HttpCookie cookie = new HttpCookie("RememberMe");
            cookie = Request.Cookies.Get("RememberMe");
            user.EmailID = cookie["emailid"];
            user.Password = cookie["password"];
            return View(user);
        }

        [HttpPost]
        public ActionResult login(User user)
        {
            HttpCookie cookie = new HttpCookie("RememberMe");
            if(user.RememberMe == true)
            {
                cookie["emailid"] = user.EmailID;
                cookie["password"] = user.Password;
                cookie.Expires = DateTime.Now.AddMonths(1);
                Response.Cookies.Add(cookie);
            }
            else
            {
                cookie["emailid"] = null;
                cookie["password"] = null;
                Response.Cookies.Add(cookie);
            }

            User valid = db.Users.Where(v => v.EmailID.Equals(user.EmailID) && v.Password.Equals(user.Password)).FirstOrDefault();

            if( valid != null)
            {
                if (valid.IsActive == true)
                {
                    if (valid.IsEmailVerified == true)
                    {
                        if(valid.RoleID == 1)
                        {
                            userid = valid.ID;
                            Session["ProfileImg"] = "/Uploads/ProfilePicture/" + @Path.GetFileName(db.UserProfiles.Where(l => l.UserID == userid).FirstOrDefault().Profile_Picture);
                            return RedirectToAction("dashboard");
                        }
                        else
                        {
                            return RedirectToAction("login", "Admin", valid);
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "Email not verified, Please verify your email first.";
                        return View("login");
                    }
                }
                else
                {
                    ViewBag.ErrorMsg = "You are Blocked or not an Active member.";
                    return View("login");
                }
            }
            else
            {
                ViewBag.ErrorMsg = "Email address or Password is incorrect.";
                return View("login");
            }
            return View();
        }

        [HttpGet]
        public ActionResult dashboard()
        {
            if(userid != 0)
            {
                List<Download> dwn = db.Downloads.ToList();
                int NoOfDownloads = 0, NoOfBuyerRequests = 0, NoOfSoldNotes = 0;
                decimal? MoneyEarned = 0;
                foreach (var item in dwn)
                {
                    if (item.Seller == userid && item.IsSellerHasAllowedDownload == true)
                    {
                        NoOfSoldNotes++;
                        MoneyEarned += item.PurchasedPrice;
                    }
                    if (item.Downloader==userid && item.IsSellerHasAllowedDownload == true)
                    {
                        NoOfDownloads++;
                    }
                    if (item.Seller == userid && item.IsSellerHasAllowedDownload == false)
                    {
                        NoOfBuyerRequests++;
                    }
                }
                ViewBag.NoOfSoldNotes = NoOfSoldNotes;
                ViewBag.MoneyEarned = MoneyEarned;
                ViewBag.NoOfDownloads = NoOfDownloads;
                ViewBag.NoOfBuyerRequests = NoOfBuyerRequests;
                List<SellerNote> notes = db.SellerNotes.ToList();
                ViewBag.userid = userid;
                return View(notes);
            }
            else
            {
                return RedirectToAction("login");
            }
            
        }

        [HttpGet]
        public ActionResult SearchNotes(int? i, string search, string NoteType, string NoteCategory, string university, string course, string country, string rating)
        {
            if (userid != 0)
            {
                ViewBag.valid = true;
            }
            ViewBag.NoteTypes = db.NoteTypes.ToList();
            ViewBag.Categories = db.NoteCategories.ToList();
            ViewBag.Countries = db.Countries.ToList();

            List<SellerNote> AllNotes = db.SellerNotes.ToList();
            List<string> UniversityList = new List<string>();
            List<string> CourseList = new List<string>();

            foreach(var note in AllNotes)
            {
                if(!UniversityList.Contains(note.UniversityName))
                {
                    UniversityList.Add(note.UniversityName);
                }
                if(!CourseList.Contains(note.Course))
                {
                    CourseList.Add(note.Course);
                }
            }
            ViewBag.UniversityList = UniversityList;
            ViewBag.CourseList = CourseList;

            ViewBag.Reviews = db.SellerNotesReviews.ToList();
            List<SellerNote> notes = new List<SellerNote>();
            foreach (var list in AllNotes.Where(n => n.Status == "Published" & n.IsActive == true ))
            {
                if(!notes.Contains(list))
                {
                    notes.Add(list);
                }
            }

            //Filter Notes
            if (!String.IsNullOrEmpty(search))
            {
                notes = notes.Where(m => m.Title.ToLower().StartsWith(search.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(NoteType))
            {
                notes = notes.Where(m => m.NoteType != null).ToList();
                notes = notes.Where(m => m.NoteType.ToLower().Equals(NoteType.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(NoteCategory))
            {
                notes = notes.Where(m => m.Category != null).ToList();
                notes = notes.Where(m => m.Category.ToLower().Equals(NoteCategory.ToLower())).ToList();
            }
            if(!String.IsNullOrEmpty(university))
            {
                notes = notes.Where(m => m.UniversityName != null).ToList();
                notes = notes.Where(m => m.UniversityName.ToLower().Equals(university.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(course))
            {
                notes = notes.Where(m => m.Course != null).ToList();
                notes = notes.Where(m => m.Course.ToLower().Equals(course.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(country))
            {
                notes = notes.Where(m => m.Country != null).ToList();
                notes = notes.Where(m => m.Country.ToLower().Equals(country.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(rating))
            {
                List<SellerNote> NoteList = new List<SellerNote>();
                foreach (var book in notes)
                {
                    float rate = 0, sum = 0;
                    foreach (var review in ViewBag.Reviews)
                    {
                        if (review.NoteID == book.ID)
                        {
                            sum += int.Parse(review.Ratings.ToString());
                        }
                    }
                    if (book.SellerNotesReviews.Count() != 0)
                    {
                        rate = (int)Math.Ceiling(sum / book.SellerNotesReviews.Count());
                    }
                    else
                    {
                        rate = 0;
                    }
                    if (rate >= int.Parse(rating))
                    {
                        var item = notes.FirstOrDefault(m => m.ID == book.ID);
                        NoteList.Add( item );
                    }
                    notes = NoteList.ToList();
                }
            }
            
            ViewBag.ResultNotes = notes.Count();
            return View(notes.ToList().ToPagedList(i ?? 1, 9));
            
        }

        public ActionResult NotesDetails(int noteid)
        {
            if (userid != 0)
            {
                ViewBag.valid = true;
            }
            SellerNote note = db.SellerNotes.FirstOrDefault(n => n.ID == noteid);
            ViewBag.Reviews = db.SellerNotesReviews.ToList();
            ViewBag.UserProfile = db.UserProfiles.ToList();
            if (note.DisplayPicture == null)
            {
                note.DisplayPicture = defaultBookImg;
            }
            return View(note);
        }

        public ActionResult DownloadRequest(int noteid)
        {
            if(userid != 0)
            {
                SellerNote note = db.SellerNotes.FirstOrDefault(n => n.ID == noteid);
                if (db.Downloads.Any(d => d.NoteID == noteid & d.Downloader == userid))
                {
                    ViewBag.ErrorMsg = "This note is already downloaded by you. You can download it from My Downloads.";
                    return RedirectToAction("NotesDetails", new { noteid });
                }
                else if(note.SellerID == userid)
                {
                    ViewBag.ErrorMsg = "You are the owner of this book.";
                    return RedirectToAction("NotesDetails", new { noteid });
                }
                else
                {
                    Download dwn = new Download();
                    dwn.NoteID = noteid;
                    dwn.Seller = note.SellerID;
                    dwn.Downloader = userid;
                    dwn.IsSellerHasAllowedDownload = false;
                    dwn.IsAttachmentDownloaded = false;
                    dwn.AttachmentPath = note.NoteAttachment;
                    dwn.IsPaid = note.IsPaid;
                    dwn.PurchasedPrice = note.SellingPrice;
                    dwn.NoteTitle = note.Title;
                    dwn.NoteCategory = note.Category;
                    dwn.CreatedDate = DateTime.Now;
                    dwn.ModifiedDate = DateTime.Now;

                    db.Downloads.Add(dwn);
                    db.SaveChanges();
                    ViewBag.RequestMsg = "Download Request has been successfully sent to seller. You can download it from My Downloads after Seller allowed to download. Stay Tuned...!!! ";

                    User user = db.Users.Where(u => u.ID == note.SellerID).FirstOrDefault();
                    User user1 = db.Users.Where(u => u.ID == userid).FirstOrDefault();

                    MailMessage mail = new MailMessage("poojapatel102938@gmail.com", user.EmailID.ToString());
                    mail.Subject = user1.FirstName + " " + user1.LastName + " wants to purchase your notes";
                    string Body = "Hello " + user.FirstName + " "  + user.LastName + ",<br><br>We would like to inform you that, " + user1.FirstName + " " + user1.LastName + " wants to purchase your notes. Please see Buyer Requests tab and allow download access to Buyer if you have received the payment from him.<br><br>Regards,<br>Notes Marketplace";
                    mail.Body = Body;
                    mail.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;

                    NetworkCredential nc = new NetworkCredential("poojapatel102938@gmail.com", "Pooja102938");
                    smtp.EnableSsl = true;
                    smtp.Credentials = nc;
                    smtp.Send(mail);

                    return RedirectToAction("NotesDetails", new { noteid });
                }
            }
            else
            {
                return RedirectToAction("login");
            }
            
            
        }

        [HttpGet]
        public ActionResult addnotes()
        {
            if (userid != 0)
            {
                SellerNote notes = new SellerNote();
                ViewBag.Categories = db.NoteCategories.ToList();
                ViewBag.NoteTypes = db.NoteTypes.ToList();
                ViewBag.Countries = db.Countries.ToList();

                return View(notes);
            }
            else
            {
                return RedirectToAction("login");
            }
        }

        [HttpPost]
        public ActionResult addnotes(SellerNote note, FormCollection fc)
        {
            note.SellerID = userid;
            note.CreatedDate = DateTime.Now;
            note.ModifiedDate = DateTime.Now;
            note.IsActive = true;

            if (fc["submit"].ToString() == "publish")
            {
                note.Status = "Submitted";
            }
            else
            {
                note.Status = "Draft";
            }

            var NoteImageName = userid + note.Title + ".jpg";
            string NoteImageExtension = Path.GetExtension(note.DisplayImageFile.FileName);

            var NoteFileName = userid + note.Title + ".pdf";
            string NoteFileExtension = Path.GetExtension(note.NotesAttachmentFile.FileName);

            var NotePreviewName = note.Title + " Preview" + ".pdf";
            string NotePreviewExtension = Path.GetExtension(note.NotesPreviewFile.FileName);

            var noteSupportedTypes = new[] { ".pdf", ".PDF" };
            var imageSupportedTypes = new[] { ".png", ".jpg", ".jpeg", ".PNG", ".JPG", ".JPEG" };

            if (imageSupportedTypes.Contains(NoteImageExtension) && noteSupportedTypes.Contains(NoteFileExtension) && noteSupportedTypes.Contains(NotePreviewExtension))
            {
                var NoteImagePath = Path.Combine(Server.MapPath("~/Uploads/NoteImage/"), NoteImageName);
                var NoteFilePath = Path.Combine(Server.MapPath("~/Uploads/NoteFile/"), NoteFileName);
                var NotePreviewPath = Path.Combine(Server.MapPath("~/Uploads/NotePreview/"), NotePreviewName);

                note.DisplayPicture = NoteImagePath;
                note.NotesPreview = NotePreviewPath;
                note.NoteAttachment = NoteFilePath;

                note.DisplayImageFile.SaveAs(NoteImagePath);
                note.NotesPreviewFile.SaveAs(NotePreviewPath);
                note.NotesAttachmentFile.SaveAs(NoteFilePath);
                db.SellerNotes.Add(note);
                db.SaveChanges();

                User user = db.Users.FirstOrDefault(u => u.ID == userid);

                if (fc["submit"].ToString() == "publish")
                {
                    MailMessage mail = new MailMessage("poojapatel102938@gmail.com", "poojapatel102938@gmail.com");
                    mail.Subject = user.FirstName + " " + user.LastName + " sent his note for review";
                    string Body = "Hello Admins,<br><br>We want to inform you that, " + user.FirstName + " " + user.LastName + " sent his note " + note.Title + " for review. Please look at the notes and take required actions. <br><br>Regards,<br>Notes Marketplace";
                    mail.Body = Body;
                    mail.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;

                    NetworkCredential nc = new NetworkCredential("poojapatel102938@gmail.com", "Pooja102938");
                    smtp.EnableSsl = true;
                    smtp.Credentials = nc;
                    smtp.Send(mail);
                }

                return RedirectToAction("dashboard");
            }
            else
            {
                ViewBag.ErrorMsg = "Please select proper file.";
                return View(note);
            }
        }

        [HttpGet]
        public ActionResult EditNote(int noteid)
        {
            if(userid != 0)
            {
                ViewBag.Categories = db.NoteCategories.ToList();
                ViewBag.NoteTypes = db.NoteTypes.ToList();
                ViewBag.Countries = db.Countries.ToList();
                SellerNote note = db.SellerNotes.FirstOrDefault(n => n.ID == noteid);
                return View("addnotes", note);
            }
            else
            {
                return RedirectToAction("login");
            }
        }

        [HttpPost]
        public ActionResult EditNote(SellerNote note, FormCollection fc)
        {
            note.SellerID = userid;
            note.CreatedDate = DateTime.Now;
            note.ModifiedDate = DateTime.Now;
            note.IsActive = true;

            if (fc["submit"].ToString() == "publish")
            {
                note.Status = "Submitted";
            }
            else
            {
                note.Status = "Draft";
            }
            if(note.DisplayImageFile != null)
            {
                var NoteImageName = userid + note.Title + ".jpg";
                string NoteImageExtension = Path.GetExtension(note.DisplayImageFile.FileName);
                var imageSupportedTypes = new[] { ".png", ".jpg", ".jpeg", ".PNG", ".JPG", ".JPEG" };
                if (imageSupportedTypes.Contains(NoteImageExtension))
                {
                    var NoteImagePath = Path.Combine(Server.MapPath("~/Uploads/NoteImage/"), NoteImageName);
                    note.DisplayPicture = NoteImagePath;
                    note.DisplayImageFile.SaveAs(NoteImagePath);
                }
                else
                {
                    ViewBag.ErrorImgFile = "Upload proper image file.";
                    return View("addnotes", note);
                }
            }
            if (note.NotesAttachmentFile != null)
            {
                var NoteFileName = userid + note.Title + ".pdf";
                string NoteFileExtension = Path.GetExtension(note.NotesAttachmentFile.FileName);
                var noteSupportedTypes = new[] { ".pdf", ".PDF" };
                if (noteSupportedTypes.Contains(NoteFileExtension))
                {
                    var NoteFilePath = Path.Combine(Server.MapPath("~/Uploads/NoteFile/"), NoteFileName);
                    note.NoteAttachment = NoteFilePath;
                    note.NotesAttachmentFile.SaveAs(NoteFilePath);
                }
                else
                {
                    ViewBag.ErrorPdfFile = "Upload proper pdf file.";
                    return View("addnotes", note);
                }
            }
            if (note.NotesPreviewFile != null)
            {
                var NotePreviewName = note.Title + " Preview" + ".pdf";
                string NotePreviewExtension = Path.GetExtension(note.NotesPreviewFile.FileName);
                var noteSupportedTypes = new[] { ".pdf", ".PDF" };
                if (noteSupportedTypes.Contains(NotePreviewExtension))
                {
                    var NotePreviewPath = Path.Combine(Server.MapPath("~/Uploads/NotePreview/"), NotePreviewName);
                    note.NotesPreview = NotePreviewPath;
                    note.NotesPreviewFile.SaveAs(NotePreviewPath);
                }
                else
                {
                    ViewBag.ErrorPdfFile = "Upload proper pdf file.";
                    return View("addnotes", note);
                }
            }
            if(note.ID == 0)
            {
                db.SellerNotes.Add(note);
            }
            else
            {
                db.Entry(note).State = System.Data.Entity.EntityState.Modified;
            }
            db.SaveChanges();

            User user = db.Users.FirstOrDefault(u => u.ID == userid);
            if (fc["submit"].ToString() == "publish")
            {
                MailMessage mail = new MailMessage("poojapatel102938@gmail.com", "poojapatel102938@gmail.com");
                mail.Subject = user.FirstName + " " + user.LastName + " sent his note for review";
                string Body = "Hello Admins,<br><br>We want to inform you that, " + user.FirstName + " " + user.LastName + " sent his note " + note.Title + " for review. Please look at the notes and take required actions. <br><br>Regards,<br>Notes Marketplace";
                mail.Body = Body;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;

                NetworkCredential nc = new NetworkCredential("poojapatel102938@gmail.com", "Pooja102938");
                smtp.EnableSsl = true;
                smtp.Credentials = nc;
                smtp.Send(mail);
            }

            return RedirectToAction("dashboard");
        }

        public ActionResult DeleteNote(int noteid)
        {
            SellerNote note = db.SellerNotes.FirstOrDefault(n => n.ID == noteid);
            note.IsActive = false;
            db.Entry(note).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("dashboard");
        }

        public ActionResult DownloadNote(int noteid)
        {
            SellerNote note = db.SellerNotes.FirstOrDefault(n => n.ID == noteid);
            string NoteTitle = note.NoteAttachment;

            byte[] fileBytes = GetFile(NoteTitle);
            return File(
                fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, note.NoteAttachment);
        }

        byte[] GetFile(string s)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(s);
            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
                throw new System.IO.IOException(s);
            return data;
        }

        public ActionResult buyerRequests()
        {
            if (userid != 0)
            {
                List<Download> notes = db.Downloads.ToList();
                ViewBag.user = db.Users.ToList();
                ViewBag.UserProfile = db.UserProfiles.ToList();
                ViewBag.userid = userid;
                return View(notes);
            }
            else
            {
                return RedirectToAction("login");
            }
        }

        public ActionResult AllowToDownloadNote(int id)
        {
            Download dwn = db.Downloads.FirstOrDefault(n => n.ID == id);
            dwn.IsSellerHasAllowedDownload = true;
            dwn.AttachmentDownloadedDate = DateTime.Now;
            db.Entry(dwn).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            MailMessage mail = new MailMessage("poojapatel102938@gmail.com", dwn.User1.EmailID.ToString());
            mail.Subject = dwn.User.FirstName + " " + dwn.User.LastName + " Allows you to download a note";
            string Body = "Hello " + dwn.User1.FirstName + " " + dwn.User1.LastName + ",<br><br>We would like to inform you that, " + dwn.User.FirstName + " " + dwn.User.LastName + "  Allows you to download a note. Please login and see My Download tabs to download particular note.<br><br>Regards,<br>Notes Marketplace";
            mail.Body = Body;
            mail.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.UseDefaultCredentials = false;

            NetworkCredential nc = new NetworkCredential("poojapatel102938@gmail.com", "Pooja102938");
            smtp.EnableSsl = true;
            smtp.Credentials = nc;
            smtp.Send(mail);

            return RedirectToAction("buyerRequests");
        }

        public ActionResult MyDownloads()
        {
            if (userid != 0)
            {
                List<Download> notes = db.Downloads.ToList();
                ViewBag.user = db.Users.ToList();
                ViewBag.userid = userid;
                return View(notes);
            }
            else
            {
                return RedirectToAction("login");
            }
        }

        [HttpPost]
        public ActionResult ReportNote(FormCollection fc)
        {
            SellerNotesReportedIssue report = new SellerNotesReportedIssue();
            report.NoteID = int.Parse(fc["noteid"].ToString());
            report.ReportedBYID = userid;

            bool Valid = false;
            List<SellerNotesReportedIssue> reports = db.SellerNotesReportedIssues.ToList();
            foreach(SellerNotesReportedIssue item in reports)
            {
                if(item.NoteID==report.NoteID && item.ReportedBYID == report.ReportedBYID)
                {
                    ViewBag.ErrorMsg = "you can't report same book more than 1 time.";
                    Valid = true;
                }
            }
            if(Valid == false)
            {
                report.Remarks = fc["remark"].ToString();

                Download dwn = db.Downloads.FirstOrDefault(n => n.NoteID == report.NoteID & n.Downloader == userid);
                report.AgainstDownloadID = dwn.ID;
                report.CreatedDate = DateTime.Now;
                report.ModifiedDate = DateTime.Now;

                db.SellerNotesReportedIssues.Add(report);
                db.SaveChanges();

                MailMessage mail = new MailMessage("poojapatel102938@gmail.com", "poojapatel102938@gmail.com");
                mail.Subject = dwn.User1.FirstName + " " + dwn.User1.LastName + " Reported an issue for " + dwn.NoteTitle;
                string Body = "Hello Admins,<br><br>We want to inform you that, " + dwn.User1.FirstName + " " + dwn.User1.LastName + " Reported an issue for " + dwn.User.FirstName + " " + dwn.User.LastName + "’s Note with title " + dwn.NoteTitle + ". Please look at the notes and take required actions.<br><br>Regards,<br>Notes Marketplace";
                mail.Body = Body;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;

                NetworkCredential nc = new NetworkCredential("poojapatel102938@gmail.com", "Pooja102938");
                smtp.EnableSsl = true;
                smtp.Credentials = nc;
                smtp.Send(mail);
            }

            return RedirectToAction("MyDownloads");
        }

        [HttpPost]
        public ActionResult ReviewNote(FormCollection fc)
        {
            SellerNotesReview review = new SellerNotesReview();
            review.NoteID = int.Parse(fc["noteid"].ToString());
            review.ReviewedByID = userid;

            bool Valid = false;
            List<SellerNotesReview> reviews = db.SellerNotesReviews.ToList();
            foreach(SellerNotesReview item in reviews)
            {
                if (item.NoteID == review.NoteID && item.ReviewedByID == review.ReviewedByID)
                {
                    ViewBag.ErrorMsg = "you can't review same book more than 1 time.";
                    Valid = true;
                }
            }

            if(Valid == false)
            {
                review.Ratings = int.Parse(fc["rate"].ToString());
                review.Comments = fc["comment"].ToString();

                Download dwn = db.Downloads.FirstOrDefault(n => n.NoteID == review.NoteID & n.Downloader == userid);
                review.AgainstDownloadsID = dwn.ID;
                review.CreatedDate = DateTime.Now;
                review.ModifiedDate = DateTime.Now;
                review.IsActive = true;

                db.SellerNotesReviews.Add(review);
                db.SaveChanges();
            }
            return RedirectToAction("MyDownloads");
        }

        public ActionResult MyRejectedNotes()
        {
            if (userid != 0)
            {
                List<SellerNote> notes = db.SellerNotes.ToList();
                ViewBag.userid = userid;
                return View(notes);
            }
            else
            {
                return RedirectToAction("login");
            }
        }

        [HttpGet]
        public ActionResult CloneNote(int noteid)
        {
            if (userid != 0)
            {
                SellerNote note = db.SellerNotes.FirstOrDefault(n => n.ID == noteid);
                SellerNote clonenote = note;
                clonenote.ID = 0;
                ViewBag.Categories = db.NoteCategories.ToList();
                ViewBag.NoteTypes = db.NoteTypes.ToList();
                ViewBag.Countries = db.Countries.ToList();

                return View("addnotes", clonenote);
            }
            else
            {
                return RedirectToAction("login");
            }
        }

        [HttpPost]
        public ActionResult CloneNote(SellerNote note, FormCollection fc)
        {
            EditNote(note, fc);
            return RedirectToAction("dashboard");
        }

        public ActionResult MySoldNotes()
        {
            if (userid != 0)
            {
                List<Download> notes = db.Downloads.ToList();
                ViewBag.user = db.Users.ToList();
                ViewBag.userid = userid;
                return View(notes);
            }
            else
            {
                return RedirectToAction("login");
            }
        }

        [HttpGet]
        public ActionResult UserProfile()
        {
            if (userid != 0)
            {
                User user = db.Users.FirstOrDefault(u => u.ID == userid);
                
                UserProfile profile = db.UserProfiles.FirstOrDefault(p => p.UserID.Equals(userid));

                ViewBag.Country = db.Countries.ToList();
                
                if (profile != null)
                {
                    profile.FirstName = user.FirstName;
                    profile.LastName = user.LastName;
                    profile.EmailID = user.EmailID;
                    return View(profile);
                }
                else
                {
                    UserProfile profile1 = new UserProfile();
                    profile1.FirstName = user.FirstName;
                    profile1.LastName = user.LastName;
                    profile1.EmailID = user.EmailID;
                    return View(profile1);
                }
                
            }
            else
            {
                return RedirectToAction("login");
            }
        }

        [HttpPost]
        public ActionResult UserProfile(UserProfile profile)
        {
            User user = db.Users.FirstOrDefault(u => u.ID == userid);
            if (db.UserProfiles.Any(x => x.UserID == userid))
            {
                profile.UserID = userid;
                profile.CreatedDate = user.CreatedDate;
                profile.ModifiedDate = DateTime.Now;
                if (profile.ProfilePictureFile != null)
                {
                    var ProfileImageName = userid + user.FirstName + ".jpg";
                    string ProfileImageExtension = Path.GetExtension(profile.ProfilePictureFile.FileName);

                    var imageSupportedTypes = new[] { ".png", ".jpg", ".jpeg", ".PNG", ".JPG", ".JPEG" };

                    if (imageSupportedTypes.Contains(ProfileImageExtension))
                    {
                        var ProfileImagePath = Path.Combine(Server.MapPath("~/Uploads/ProfilePicture/"), ProfileImageName);
                        profile.Profile_Picture = ProfileImagePath;
                        profile.ProfilePictureFile.SaveAs(ProfileImagePath);
                        db.Entry(profile).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        user.EmailID = profile.EmailID;
                        user.FirstName = profile.FirstName;
                        user.LastName = profile.LastName;
                        user.ConfirmPassword = user.Password;
                        db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        return RedirectToAction("dashboard");
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "Please select proper formate image.";
                        ViewBag.Country = db.Countries.ToList();
                        return View("UserProfile");
                    }
                }
                else
                {
                    user.EmailID = profile.EmailID;
                    user.FirstName = profile.FirstName;
                    user.LastName = profile.LastName;
                    user.ConfirmPassword = user.Password;
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    db.Entry(profile).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("dashboard");
                }
            }
            else
            {
                profile.UserID = userid;
                profile.CreatedDate = user.CreatedDate;
                profile.ModifiedDate = DateTime.Now;

                var ProfileImageName = userid + user.FirstName + ".jpg";
                string ProfileImageExtension = Path.GetExtension(profile.ProfilePictureFile.FileName);

                var imageSupportedTypes = new[] { ".png", ".jpg", ".jpeg", ".PNG", ".JPG", ".JPEG" };

                if (imageSupportedTypes.Contains(ProfileImageExtension))
                {
                    var ProfileImagePath = Path.Combine(Server.MapPath("~/Uploads/ProfilePicture/"), ProfileImageName);

                    profile.Profile_Picture = ProfileImagePath;

                    profile.ProfilePictureFile.SaveAs(ProfileImagePath);
                    db.UserProfiles.Add(profile);
                    db.SaveChanges();

                    return RedirectToAction("dashboard");
                }
                else
                {
                    ViewBag.ErrorMsg = "Please select proper file.";
                    ViewBag.Country = db.Countries.ToList();
                    return View(profile);
                }
            }
        }

        [HttpGet]
        public ActionResult contactUs ()
        {
            if (userid != 0)
            {
                ViewBag.valid = true;
            }
            return View();
        }

        [HttpPost]
        public ActionResult contactUs(ContactUs c)
        {
            try
            {
                ViewBag.SuccessMsg = "Your query has been successfully sent to system. Our executive will get back to you within 24hr.";

                MailMessage mail = new MailMessage(c.EmailID.ToString(), "poojapatel102938@gmail.com");
                mail.Subject = c.FullName + " - Query";
                string Body = "Hello,<br><br>Subject/Question : " + c.Subject + ",<br>Comments : " + c.Comment + "<br><br>Regards,<br>" + c.FullName;
                mail.Body = Body;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;

                NetworkCredential nc = new NetworkCredential("poojapatel102938@gmail.com", "Pooja102938");
                smtp.EnableSsl = true;
                smtp.Credentials = nc;
                smtp.Send(mail);

                return View("contactUs");
            }
            catch (Exception e)
            {
                return View("contactUs", c);
            }
        }

        public ActionResult faq()
        {
            if (userid != 0)
            {
                ViewBag.valid = true;
            }
            return View();
        }


        public ActionResult logout()
        {
            userid = 0;
            return RedirectToAction("login");
        }
    }
}