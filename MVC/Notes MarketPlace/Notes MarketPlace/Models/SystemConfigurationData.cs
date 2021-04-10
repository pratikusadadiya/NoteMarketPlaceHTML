using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Notes_MarketPlace.Models
{
    public class SystemConfigurationData
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Please enter support email address.")]
        [Display(Name = "Support email address")]
        public string SupportEmail { get; set; }

        [Required(ErrorMessage = "Please enter support mobile number.")]
        [Display(Name = "Support phone number")]
        public string SupportMobileNo { get; set; }

        [Required(ErrorMessage = "Please enter email addresses to notify.")]
        [Display(Name = "Email Address(es) (for various events system will send notifications to these users)")]
        public string NotifyEmails { get; set; }

        [Display(Name = "Facebook URL")]
        public string FacebookURL { get; set; }

        [Display(Name = "Twitter URL")]
        public string TwitterURL { get; set; }

        [Display(Name = "Linekedin URL")]
        public string LinekedinURL { get; set; }

        [Required(ErrorMessage = "Please select profile image.")]
        public HttpPostedFileBase DefaultProfileImgFile { get; set; }
        public string DefaultProfileImg { get; set; }

        [Required(ErrorMessage = "Please select note cover image.")]
        public HttpPostedFileBase DefaultNoteImgFile { get; set; }
        public string DefaultNoteImg { get; set; }
    }
}