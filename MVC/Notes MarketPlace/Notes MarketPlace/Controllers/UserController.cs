using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Notes_MarketPlace.Models;

namespace Notes_MarketPlace.Controllers
{
    public class UserController : Controller
    {
        Notes_MarketPlaceEntities db = new Notes_MarketPlaceEntities();
        static int userid;

        public ActionResult home()
        {
            if(userid != 0)
            {
                ViewBag.valid = true;
            }
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
                string Body = "Dear " + user.FirstName + " " + user.LastName + ",\n\n Thank you for signing up with Notes MarketPlace. Please click on below link to verify your email http://localhost:52734/User/emailVerification?emailid="+user.EmailID;
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
                    mail.Subject = "Notes MarketPlace - Forgot Password";
                    string Body = "Dear " + valid.FirstName + " " + valid.LastName + ",\n\n Your temporary password is : " + valid.Password;
                    mail.Body = Body;
                    

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
            return View("changePassword");
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

            var valid = db.Users.Where(v => v.EmailID.Equals(user.EmailID) && v.Password.Equals(user.Password)).FirstOrDefault();

            if( valid != null)
            {
                if(valid.RoleID == 1)
                {
                    if (valid.IsActive == true)
                    {
                        if (valid.IsEmailVerified == true)
                        {
                            userid = valid.ID;
                            return RedirectToAction("dashboard");
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
                    ViewBag.ErrorMsg = "You are not a member.";
                    return View("login");
                }
            }
            else
            {
                ViewBag.ErrorMsg = "Email address or Password is incorrect.";
                return View("login");
            }
        }

        [HttpGet]
        public ActionResult dashboard()
        {
            
            if(userid != 0)
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
        public ActionResult addnotes()
        {
            if (userid != 0)
            {
                SellerNote notes = new SellerNote();
                notes.Categories = new SelectList(db.NoteCategories.ToList(), dataValueField:"ID", dataTextField:"Name");
                notes.NoteTypes = new SelectList(db.NoteTypes.ToList(), "ID", "Name");
                notes.CountryList = new SelectList(db.Countries.ToList(), "ID", "Name");

                return View(notes);
            }
            else
            {
                return RedirectToAction("login");
            }
        }

        [HttpPost]
        public ActionResult addnotes(SellerNote note, string save, string publish)
        {
            note.SellerID = userid;
            note.Category = 1;
            note.NoteType = 1;
            note.Country = 1;
            note.CreatedDate = DateTime.Now;
            note.ModifiedDate = DateTime.Now;
            note.IsActive = true;

            if(!String.IsNullOrEmpty(publish))
            {
                note.Status = "Submitted";
            }
            else
            {
                note.Status = "Draft";
            }

            var NoteImageName = userid + note.Title;
            string NoteImageExtension = Path.GetExtension(note.DisplayImageFile.FileName);

            var NoteFileName = userid + note.Title;
            string NoteFileExtension = Path.GetExtension(note.NotesAttachmentFile.FileName);

            var NotePreviewName = note.Title + " Preview";
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

                return RedirectToAction("dashboard");
            }
            else
            {
                ViewBag.ErrorMsg = "Please select proper file.";
                return View(note);
            }
        }

        public ActionResult buyerRequests()
        {
            return View();
        }

        public ActionResult UserProfile()
        {
            return View();
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

                MailMessage mail = new MailMessage("poojapatel102938@gmail.com", c.EmailID.ToString());
                mail.Subject = "Notes MarketPlace - Query";
                string Body = "Dear " + c.FullName + ", Subject : " + c.Subject + ", Comments : " + c.Comment;
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

        public ActionResult NotesDetails()
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
            return RedirectToAction("home");
        }
    }
}