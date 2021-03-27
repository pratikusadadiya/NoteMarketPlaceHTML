using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Notes_MarketPlace.Models
{
    public class changePassword
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Please enter old password.")]
        [Display(Name = "Password")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Please enter new password.")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please enter confirm password.")]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }
    }
}