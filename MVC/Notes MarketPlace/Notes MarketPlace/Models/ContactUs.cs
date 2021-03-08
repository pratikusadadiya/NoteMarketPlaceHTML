using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Notes_MarketPlace.Models
{
    public class ContactUs
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Please enter your name.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please enter your email address.")]
        public string EmailID { get; set; }

        [Required(ErrorMessage = "Please enter subject.")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Please enter comment.")]
        public string Comment { get; set; }
    }
}