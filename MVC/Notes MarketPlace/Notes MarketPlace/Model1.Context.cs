﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Notes_MarketPlaceEntities : DbContext
    {
        public Notes_MarketPlaceEntities()
            : base("name=Notes_MarketPlaceEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AdminProfile> AdminProfiles { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Download> Downloads { get; set; }
        public virtual DbSet<NoteCategory> NoteCategories { get; set; }
        public virtual DbSet<NoteType> NoteTypes { get; set; }
        public virtual DbSet<ReferenceDate> ReferenceDates { get; set; }
        public virtual DbSet<SellerNote> SellerNotes { get; set; }
        public virtual DbSet<SellerNotesAttachement> SellerNotesAttachements { get; set; }
        public virtual DbSet<SellerNotesReportedIssue> SellerNotesReportedIssues { get; set; }
        public virtual DbSet<SellerNotesReview> SellerNotesReviews { get; set; }
        public virtual DbSet<SystemConfiguration> SystemConfigurations { get; set; }
        public virtual DbSet<UserProfile> UserProfiles { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<User> Users { get; set; }
    }
}
