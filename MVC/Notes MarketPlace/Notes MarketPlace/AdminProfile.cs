//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Notes_MarketPlace
{
    using System;
    using System.Collections.Generic;
    using System.Web;

    public partial class AdminProfile
    {
        public int Id { get; set; }
        public int AdminID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailID { get; set; }
        public string ScondaryEmailAddress { get; set; }
        public string Phone_number_country_code { get; set; }
        public string phone_number { get; set; }
        public string Profile_Picture { get; set; }
        public HttpPostedFileBase ProfilePictureFile { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
    
        public virtual User User { get; set; }
    }
}
