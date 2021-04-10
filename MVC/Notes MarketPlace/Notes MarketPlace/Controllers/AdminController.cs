using Notes_MarketPlace.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Notes_MarketPlace.Controllers
{
    public class AdminController : Controller
    {
        Notes_MarketPlaceEntities db = new Notes_MarketPlaceEntities();
        static int adminid = 0, superadmin = 0;
        string defaultBookImg = "../../Content/images/Front/Notes-details/1.jpg";
        string defaultBookPreview = "../../Content/images/Front/Notes-details/sample-pdf.png";

        // GET: Admin
        public ActionResult login(User user)
        {
            superadmin = 0;
            adminid = user.ID;
            if(user.RoleID == 3)
            {
                superadmin = user.ID;
            }
            return RedirectToAction("dashboard");
        }


        [HttpGet]
        public ActionResult changePassword()
        {
            if (adminid != 0)
            {
                return View();
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        [HttpPost]
        public ActionResult changePassword(changePassword cp)
        {
            User user = db.Users.FirstOrDefault(u => u.ID == adminid);
            if (cp.OldPassword != user.Password)
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
            return RedirectToAction("login","User");
        }

        [HttpGet]
        public ActionResult UpdateProfile()
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid !=0)
            {
                User user = db.Users.FirstOrDefault(u => u.ID == adminid);

                AdminProfile profile = db.AdminProfiles.FirstOrDefault(p => p.AdminID.Equals(adminid));

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
                    AdminProfile profile1 = new AdminProfile();
                    profile1.FirstName = user.FirstName;
                    profile1.LastName = user.LastName;
                    profile1.EmailID = user.EmailID;
                    return View(profile1);
                }
            }
            else
            {
                return RedirectToAction("login","User");
            }
        }

        [HttpPost]
        public ActionResult UpdateProfile(AdminProfile profile)
        {
            User user = db.Users.FirstOrDefault(u => u.ID == adminid);
            user.ConfirmPassword = user.Password;
            profile.User = user;
            if (db.AdminProfiles.Any(x => x.AdminID == adminid))
            {
                profile.AdminID = adminid;
                profile.CreatedDate = user.CreatedDate;
                profile.ModifiedDate = DateTime.Now;
                if (profile.ProfilePictureFile != null)
                {
                    var ProfileImageName = adminid + user.FirstName + ".jpg";
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

                        db.Entry(profile).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        return RedirectToAction("dashboard");
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "Please select proper formate image.";
                        ViewBag.Country = db.Countries.ToList();
                        return View("UserProfile",profile);
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
                profile.AdminID = adminid;
                profile.CreatedDate = user.CreatedDate;
                profile.ModifiedDate = DateTime.Now;

                var ProfileImageName = adminid + user.FirstName + ".jpg";
                string ProfileImageExtension = Path.GetExtension(profile.ProfilePictureFile.FileName);

                var imageSupportedTypes = new[] { ".png", ".jpg", ".jpeg", ".PNG", ".JPG", ".JPEG" };

                if (imageSupportedTypes.Contains(ProfileImageExtension))
                {
                    var ProfileImagePath = Path.Combine(Server.MapPath("~/Uploads/ProfilePicture/"), ProfileImageName);

                    profile.Profile_Picture = ProfileImagePath;

                    profile.ProfilePictureFile.SaveAs(ProfileImagePath);

                    db.AdminProfiles.Add(profile);
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

        public ActionResult dashboard(string month)
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid != 0)
            {
                List<SellerNote> notes = db.SellerNotes.ToList();
                List<Download> dwn = db.Downloads.ToList();
                List<User> user = db.Users.ToList();

                int NoOfinReviewNotes = 0, NoOfNewDownloads = 0, NoOfNewUsers = 0;
                foreach (var item in notes)
                {
                    if (item.Status == "In Review" || item.Status == "Submitted" && item.IsActive == true)
                    {
                        NoOfinReviewNotes++;
                    }
                }
                ViewBag.NoOfinReviewNotes = NoOfinReviewNotes;

                var dt = DateTime.Now.AddDays(-7);
                foreach(var item1 in dwn)
                {
                    if(item1.AttachmentDownloadedDate > dt)
                    {
                        NoOfNewDownloads++;
                    }
                }
                ViewBag.NoOfNewDownloads = NoOfNewDownloads;

                foreach (var item2 in user)
                {
                    if (item2.CreatedDate > dt && item2.RoleID == 1)
                    {
                        NoOfNewUsers++;
                    }
                }
                ViewBag.NoOfNewUsers = NoOfNewUsers;

                if (!String.IsNullOrEmpty(month))
                {
                    notes = notes.Where(n => n.PublishedDate != null).ToList();
                    notes = notes.Where(n => n.PublishedDate.ToString().Substring(3, 2).Equals(month)).ToList();
                }

                return View(notes);
            }
            else
            {
                return RedirectToAction("login","User");
            }

        }

        [HttpGet]
        public ActionResult NotesDetails(int noteid)
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid != 0)
            {
                ViewBag.admin = true;
            }
            SellerNote note = db.SellerNotes.FirstOrDefault(n => n.ID == noteid);
            ViewBag.Reviews = db.SellerNotesReviews.ToList();
            ViewBag.UserProfile = db.UserProfiles.ToList();
            if (note.DisplayPicture == null)
            {
                note.DisplayPicture = defaultBookImg;
            }
            if (note.NotesPreview == null)
            {
                note.NotesPreview = defaultBookPreview;

            }
            return View("NotesDetails",note);
        }

        public ActionResult Unpublish(int noteid, string AdminRemark)
        {
            if (adminid != 0)
            {
                SellerNote note = db.SellerNotes.FirstOrDefault(n => n.ID == noteid);
                note.Status = "Removed";
                note.AdminRemarks = AdminRemark;
                note.ActionedBy = adminid;
                db.Entry(note).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                MailMessage mail = new MailMessage("poojapatel102938@gmail.com", note.User.EmailID.ToString());
                mail.Subject = "Sorry! We need to remove your notes from our portal.";
                string Body = "Hello " + note.User.FirstName + " " + note.User.LastName + ",<br><br> We want to inform you that, your note " + note.Title + " has been removed from the portal." + "<br>Please find our remarks as below -<br><b>" + note.AdminRemarks + "</b><br><br>Regards," + "<br>Notes Marketplace";
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

                return RedirectToAction("PublishedNotes");
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        public ActionResult Members()
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid != 0)
            {
                List<User> users = db.Users.ToList();
                ViewBag.notes = db.SellerNotes.ToList();
                ViewBag.downloads = db.Downloads.ToList();
                return View(users);
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        public ActionResult MemberDetails(int memberid)
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid != 0)
            {
                ViewBag.notes = db.SellerNotes.ToList();
                ViewBag.downloads = db.Downloads.ToList();
                UserProfile profile = db.UserProfiles.FirstOrDefault(p => p.UserID == memberid);
                    
                return View(profile);
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        public ActionResult DeactivateMember(int memberid)
        {
            if(adminid != 0)
            {
                User user = db.Users.FirstOrDefault(n => n.ID == memberid);
                user.ConfirmPassword = user.Password;
                user.ModifiedDate = DateTime.Now;
                user.IsActive = false;
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Members");
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        public ActionResult DownloadedNotes(string notetitle, string buyer, string seller)
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid != 0)
            {
                List<Download> dwn = db.Downloads.ToList();

                List<string> Notes = new List<string>();
                List<string> Buyers = new List<string>();
                List<string> Sellers = new List<string>();

                foreach(Download item in dwn)
                {
                    if (!Notes.Contains(item.NoteTitle))
                    {
                        Notes.Add(item.NoteTitle);
                    }
                    string name = item.User1.FirstName + " " + item.User1.LastName;
                    if (!Buyers.Contains(name))
                    {
                        Buyers.Add(name);
                    }
                    string name1 = item.User.FirstName + " " + item.User.LastName;
                    if (!Sellers.Contains(name1))
                    {
                        Sellers.Add(name1);
                    }
                }

                ViewBag.NoteTitles = Notes;
                ViewBag.Buyers = Buyers;
                ViewBag.Sellers = Sellers;

                if(!String.IsNullOrEmpty(notetitle))
                {
                    dwn = dwn.Where(m => m.NoteTitle.ToLower().Equals(notetitle.ToLower())).ToList();
                }
                if(!String.IsNullOrEmpty(buyer))
                {
                    List<Download> notes = new List<Download>();
                    foreach(var item in dwn)
                    {
                        string name = item.User1.FirstName + " " + item.User1.LastName;
                        if(name.Equals(buyer))
                        {
                            notes.Add(item);
                        }
                    }
                    dwn = notes.ToList();
                }
                if(!String.IsNullOrEmpty(seller))
                {
                    List<Download> notes = new List<Download>();
                    foreach (var item1 in dwn)
                    {
                        string name = item1.User.FirstName + " " + item1.User.LastName;
                        if (name.Equals(seller))
                        {
                            notes.Add(item1);
                        }
                    }
                    dwn = notes.ToList();
                }

                return View(dwn);
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        public ActionResult PublishedNotes(string seller)
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid != 0)
            {
                List<SellerNote> notes = db.SellerNotes.ToList();
                ViewBag.Users = db.Users.ToList();
                List<string> Sellers = new List<string>();
                List<SellerNote> books = new List<SellerNote>();

                foreach (SellerNote item in notes)
                {
                    string name = item.User.FirstName + " " + item.User.LastName;
                    if (!Sellers.Contains(name))
                    {
                        Sellers.Add(name);
                    }

                    if (!String.IsNullOrEmpty(seller))
                    {
                        if (name.Equals(seller))
                        {
                            books.Add(item);
                        }
                    }
                }
                if (!String.IsNullOrEmpty(seller))
                {
                    notes = books.ToList();
                }
                    
                ViewBag.Sellers = Sellers;

                return View(notes);
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        public ActionResult RejectedNotes(string seller)
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid != 0)
            {
                List<SellerNote> notes = db.SellerNotes.ToList();
                ViewBag.Users = db.Users.ToList();
                List<string> Sellers = new List<string>();
                List<SellerNote> books = new List<SellerNote>();

                foreach (SellerNote item in notes)
                {
                    string name = item.User.FirstName + " " + item.User.LastName;
                    if (!Sellers.Contains(name))
                    {
                        Sellers.Add(name);
                    }

                    if (!String.IsNullOrEmpty(seller))
                    {
                        if (name.Equals(seller))
                        {
                            books.Add(item);
                        }
                    }
                }
                if (!String.IsNullOrEmpty(seller))
                {
                    notes = books.ToList();
                }

                ViewBag.Sellers = Sellers;

                return View(notes);
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        public ActionResult ApproveNote(int noteid)
        {
            if(adminid != 0)
            {
                SellerNote note = db.SellerNotes.FirstOrDefault(n => n.ID == noteid);
                note.Status = "Published";
                note.ActionedBy = adminid;
                note.PublishedDate = DateTime.Now;
                note.AdminRemarks = null;
                db.Entry(note).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("PublishedNotes");
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        public ActionResult NotesUnderReview(string seller)
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid != 0)
            {
                List<SellerNote> notes = db.SellerNotes.ToList();
                ViewBag.Users = db.Users.ToList();
                List<string> Sellers = new List<string>();
                List<SellerNote> books = new List<SellerNote>();

                foreach (SellerNote item in notes)
                {
                    string name = item.User.FirstName + " " + item.User.LastName;
                    if (!Sellers.Contains(name))
                    {
                        Sellers.Add(name);
                    }

                    if (!String.IsNullOrEmpty(seller))
                    {
                        if (name.Equals(seller))
                        {
                            books.Add(item);
                        }
                    }
                }
                if (!String.IsNullOrEmpty(seller))
                {
                    notes = books.ToList();
                }

                ViewBag.Sellers = Sellers;

                return View(notes);
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        public ActionResult SetInReview(int noteid)
        {
            if (adminid != 0)
            {
                SellerNote note = db.SellerNotes.FirstOrDefault(n => n.ID == noteid);
                note.Status = "In Review";
                note.ActionedBy = adminid;
                db.Entry(note).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("NotesUnderReview");
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }
        
        public ActionResult RejectNote(int noteid, string RejectRemark)
        {
            if (adminid != 0)
            {
                SellerNote note = db.SellerNotes.FirstOrDefault(n => n.ID == noteid);
                note.Status = "Rejected";
                note.AdminRemarks = RejectRemark;
                note.ActionedBy = adminid;
                db.Entry(note).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("NotesUnderReview");
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        public ActionResult ManageAdministrator()
        {
            if(superadmin != 0)
            {
                ViewBag.superadmin = true;
                List<AdminProfile> admins = db.AdminProfiles.ToList();
                return View(admins);
            }
            else
            {
                return RedirectToAction("dashboard");
            }
        }

        [HttpGet]
        public ActionResult AddAdministrator()
        {
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
                ViewBag.Country = db.Countries.ToList();
                AdminProfile admin = new AdminProfile();
                admin.CreatedDate = DateTime.Now;
                return View(admin);
            }
            else
            {
                return RedirectToAction("dashboard");
            }
        }

        [HttpPost]
        public ActionResult AddAdministrator(AdminProfile admin)
        {
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
                
                if (db.Users.Any(x => x.EmailID == admin.User.EmailID))
                {
                    ViewBag.Country = db.Countries.ToList();
                    ViewBag.DuplicateMsg = "Email ID already exist.";
                    return View("AddAdministrator", admin);
                }
                User user = new User();
                user.FirstName = admin.User.FirstName;
                user.LastName = admin.User.LastName;
                user.EmailID = admin.User.EmailID;
                user.Password = admin.User.FirstName;
                user.ConfirmPassword = user.Password;
                user.RoleID = 2;
                user.IsActive = true;
                user.IsEmailVerified = true;
                user.CreatedDate = DateTime.Now;
                user.ModifiedDate = DateTime.Now;
                db.Users.Add(user);
                db.SaveChanges();

                User user1 = db.Users.Where(u => u.EmailID.Equals(user.EmailID)).FirstOrDefault();
                admin.User = user1;
                admin.AdminID = user1.ID;
                admin.CreatedDate = user1.CreatedDate;
                admin.ModifiedDate = user1.CreatedDate;
                db.AdminProfiles.Add(admin);
                db.SaveChanges();

                return RedirectToAction("ManageAdministrator");
            }
            else
            {
                return RedirectToAction("dashboard");
            }
        }

        [HttpGet]
        public ActionResult EditAdministrator(int editid)
        {
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
                ViewBag.Country = db.Countries.ToList();
                AdminProfile admin = db.AdminProfiles.Where(a => a.Id == editid).FirstOrDefault();
                return View("AddAdministrator",admin);
            }
            else
            {
                return RedirectToAction("dashboard");
            }
        }

        [HttpPost]
        public ActionResult EditAdministrator(AdminProfile profile)
        {
            if (superadmin != 0)
            {
                User user = db.Users.Where(u => u.ID == profile.AdminID).FirstOrDefault();
                user.FirstName = profile.User.FirstName;
                user.LastName = profile.User.LastName;
                user.EmailID = profile.User.EmailID;
                user.ConfirmPassword = user.Password;
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                profile.User = user;
                db.Entry(profile).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("ManageAdministrator");
            }
            else
            {
                return RedirectToAction("dashboard");
            }
        }

        public ActionResult DeleteAdministrator(int deleteid)
        {
            if (superadmin != 0)
            {
                AdminProfile admin = db.AdminProfiles.Where(a => a.Id == deleteid).FirstOrDefault();
                User user = db.Users.Where(u => u.ID == admin.AdminID).FirstOrDefault();
                user.IsActive = false;
                user.ConfirmPassword = user.Password;
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ManageAdministrator");
            }
            else
            {
                return RedirectToAction("dashboard");
            }
        }

        public ActionResult ManageCategory()
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid != 0)
            {
                List<NoteCategory> categories = db.NoteCategories.ToList();
                ViewBag.Users = db.Users.ToList();
                return View(categories);
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        public ActionResult DeleteCategory(int deleteid)
        {
            if (adminid != 0)
            {
                NoteCategory category = db.NoteCategories.Where(a => a.ID == deleteid).FirstOrDefault();
                category.IsActive = false;
                category.ModifiedDate = DateTime.Now;
                category.ModifiedBy = adminid;
                db.Entry(category).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ManageCategory");
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        [HttpGet]
        public ActionResult EditCategory(int editid)
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid != 0)
            {
                NoteCategory category = db.NoteCategories.Where(a => a.ID == editid).FirstOrDefault();
                return View("AddCategory", category);
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        [HttpPost]
        public ActionResult EditCategory(NoteCategory category)
        {
            if (adminid != 0)
            {
                category.ModifiedDate = DateTime.Now;
                category.ModifiedBy = adminid;
                db.Entry(category).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ManageCategory");
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        [HttpGet]
        public ActionResult AddCategory()
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid != 0)
            {
                return View();
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        [HttpPost]
        public ActionResult AddCategory(NoteCategory category)
        {
            if (adminid != 0)
            {
                category.IsActive = true;
                category.CreatedDate = DateTime.Now;
                category.CreatedBy = adminid;
                db.NoteCategories.Add(category);
                db.SaveChanges();
                return RedirectToAction("ManageCategory");
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        public ActionResult ManageCountry()
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid != 0)
            {
                List<Country> countries = db.Countries.ToList();
                ViewBag.Users = db.Users.ToList();
                return View(countries);
            }
            else
            {
                return RedirectToAction("dashboard");
            }
        }

        public ActionResult DeleteCountry(int deleteid)
        {
            if (adminid != 0)
            {
                Country country = db.Countries.Where(a => a.ID == deleteid).FirstOrDefault();
                country.IsActive = false;
                country.ModifiedDate = DateTime.Now;
                country.ModifiedBy = adminid;
                db.Entry(country).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ManageCountry");
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        [HttpGet]
        public ActionResult EditCountry(int editid)
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid != 0)
            {
                Country country = db.Countries.Where(a => a.ID == editid).FirstOrDefault();
                return View("AddCountry", country);
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        [HttpPost]
        public ActionResult EditCountry(Country country)
        {
            if (adminid != 0)
            {
                country.ModifiedDate = DateTime.Now;
                country.ModifiedBy = adminid;
                db.Entry(country).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ManageCountry");
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        [HttpGet]
        public ActionResult AddCountry()
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid != 0)
            {
                return View();
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        [HttpPost]
        public ActionResult AddCountry(Country country)
        {
            if (adminid != 0)
            {
                country.IsActive = true;
                country.CreatedDate = DateTime.Now;
                country.CreatedBy = adminid;
                db.Countries.Add(country);
                db.SaveChanges();
                return RedirectToAction("ManageCountry");
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        public ActionResult ManageType()
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid != 0)
            {
                List<NoteType> noteTypes = db.NoteTypes.ToList();
                ViewBag.Users = db.Users.ToList();
                return View(noteTypes);
            }
            else
            {
                return RedirectToAction("dashboard");
            }
        }

        public ActionResult DeleteType(int deleteid)
        {
            if (adminid != 0)
            {
                NoteType type = db.NoteTypes.Where(a => a.ID == deleteid).FirstOrDefault();
                type.IsActive = false;
                type.ModifiedDate = DateTime.Now;
                type.ModifiedBy = adminid;
                db.Entry(type).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ManageType");
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        [HttpGet]
        public ActionResult EditType(int editid)
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid != 0)
            {
                NoteType type = db.NoteTypes.Where(a => a.ID == editid).FirstOrDefault();
                return View("AddType", type);
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        [HttpPost]
        public ActionResult EditType(NoteType type)
        {
            if (adminid != 0)
            {
                type.ModifiedDate = DateTime.Now;
                type.ModifiedBy = adminid;
                db.Entry(type).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ManageType");
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        [HttpGet]
        public ActionResult AddType()
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid != 0)
            {
                return View();
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        [HttpPost]
        public ActionResult AddType(NoteType type)
        {
            if (adminid != 0)
            {
                type.IsActive = true;
                type.CreatedDate = DateTime.Now;
                type.CreatedBy = adminid;
                db.NoteTypes.Add(type);
                db.SaveChanges();
                return RedirectToAction("ManageType");
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

        [HttpGet]
        public ActionResult ManageSystemConfiguration()
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
                List<SystemConfiguration> list = db.SystemConfigurations.ToList();
                SystemConfigurationData data = new SystemConfigurationData();
                data.SupportEmail = list.Where(l => l.Key == "SupportEmail").FirstOrDefault().Value;
                data.SupportMobileNo = list.Where(l => l.Key == "SupportPhoneNo").FirstOrDefault().Value;
                data.NotifyEmails = list.Where(l => l.Key == "NotifyEmails").FirstOrDefault().Value;
                data.FacebookURL = list.Where(l => l.Key == "FacebookURL").FirstOrDefault().Value;
                data.TwitterURL = list.Where(l => l.Key == "TwitterURL").FirstOrDefault().Value;
                data.LinekedinURL = list.Where(l => l.Key == "LinkedinURL").FirstOrDefault().Value;
                data.DefaultNoteImg = list.Where(l => l.Key == "DefaultNoteImg").FirstOrDefault().Value;
                data.DefaultProfileImg = list.Where(l => l.Key == "DefaultProfileImg").FirstOrDefault().Value;
                return View(data);
            }
            else
            {
                return RedirectToAction("dashboard");
            }
        }

        [HttpPost]
        public ActionResult ManageSystemConfiguration(SystemConfigurationData sc)
        {
            if (superadmin != 0)
            {
                List<SystemConfiguration> list = db.SystemConfigurations.ToList();
                list.Where(l => l.Key == "SupportEmail").FirstOrDefault().Value = sc.SupportEmail;
                list.Where(l => l.Key == "SupportPhoneNo").FirstOrDefault().Value = sc.SupportMobileNo;
                list.Where(l => l.Key == "NotifyEmails").FirstOrDefault().Value = sc.NotifyEmails;
                list.Where(l => l.Key == "FacebookURL").FirstOrDefault().Value = sc.FacebookURL;
                list.Where(l => l.Key == "TwitterURL").FirstOrDefault().Value = sc.TwitterURL;
                list.Where(l => l.Key == "LinkedinURL").FirstOrDefault().Value = sc.LinekedinURL;

                if (sc.DefaultProfileImgFile != null)
                {
                    var ProfileImageName = "DefaultProfileImage.jpg";
                    string ProfileImageExtension = Path.GetExtension(sc.DefaultProfileImgFile.FileName);

                    var imageSupportedTypes = new[] { ".png", ".jpg", ".jpeg", ".PNG", ".JPG", ".JPEG" };

                    if (imageSupportedTypes.Contains(ProfileImageExtension))
                    {
                        var ProfileImagePath = Path.Combine(Server.MapPath("~/Uploads/Default/"), ProfileImageName);
                        sc.DefaultProfileImg = ProfileImagePath;
                        sc.DefaultProfileImgFile.SaveAs(ProfileImagePath);
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "Please select proper formate image.";
                        return View("ManageSystemConfiguration", sc);
                    }
                }

                if (sc.DefaultNoteImgFile != null)
                {
                    var NoteImageName = "DefaultNoteImage.jpg";
                    string NoteImageExtension = Path.GetExtension(sc.DefaultNoteImgFile.FileName);

                    var imageSupportedTypes = new[] { ".png", ".jpg", ".jpeg", ".PNG", ".JPG", ".JPEG" };

                    if (imageSupportedTypes.Contains(NoteImageExtension))
                    {
                        var NoteImagePath = Path.Combine(Server.MapPath("~/Uploads/Default/"), NoteImageName);
                        sc.DefaultNoteImg = NoteImagePath;
                        sc.DefaultNoteImgFile.SaveAs(NoteImagePath);
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "Please select proper formate image.";
                        return View("ManageSystemConfiguration", sc);
                    }
                }

                list.Where(l => l.Key == "DefaultNoteImg").FirstOrDefault().Value = sc.DefaultNoteImg;
                list.Where(l => l.Key == "DefaultProfileImg").FirstOrDefault().Value = sc.DefaultProfileImg;
                db.SaveChanges();

                ViewBag.SuccessMsg = "Data updated successfully.";
                return View("ManageSystemConfiguration",sc);
            }
            else
            {
                return RedirectToAction("dashboard");
            }
        }

        public ActionResult SpamReports()
        {
            ViewBag.superadmin = false;
            if (superadmin != 0)
            {
                ViewBag.superadmin = true;
            }
            if (adminid != 0)
            {
                List<SellerNotesReportedIssue> reports = db.SellerNotesReportedIssues.ToList();
                return View(reports);
            }
            else
            {
                return RedirectToAction("login","User");
            }
        }

        public ActionResult DeleteReport(int reportid)
        {
            SellerNotesReportedIssue report = db.SellerNotesReportedIssues.Find(reportid);
            db.SellerNotesReportedIssues.Remove(report);
            db.SaveChanges();
            return RedirectToAction("SpamReports");
        }

        public ActionResult logout()
        {
            adminid = 0;
            superadmin = 0;
            return RedirectToAction("home", "User");
        }
        
    }
}