namespace ePKBModel.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class AppModel : DbContext
    {
        public AppModel()
            : base("name=AppModel")
        {
        }

        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<Bank> Banks { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<Province> Provinces { get; set; }
        public virtual DbSet<Region> Regions { get; set; }
        public virtual DbSet<TaxMapping> TaxMappings { get; set; }
        public virtual DbSet<TaxPayment> TaxPayments { get; set; }
        public virtual DbSet<UserProfile> UserProfiles { get; set; }
        public virtual DbSet<Vehicle> Vehicles { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AspNetRole>()
                .HasMany(e => e.AspNetUsers)
                .WithMany(e => e.AspNetRoles)
                .Map(m => m.ToTable("AspNetUserRoles").MapLeftKey("RoleId").MapRightKey("UserId"));

            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.AspNetUserClaims)
                .WithRequired(e => e.AspNetUser)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.AspNetUserLogins)
                .WithRequired(e => e.AspNetUser)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.UserProfiles)
                .WithOptional(e => e.AspNetUser)
                .HasForeignKey(e => e.IdAspNetUser);

            modelBuilder.Entity<Province>()
                .HasMany(e => e.Banks)
                .WithOptional(e => e.Province)
                .HasForeignKey(e => e.IdProvince);

            modelBuilder.Entity<Province>()
                .HasMany(e => e.Regions)
                .WithOptional(e => e.Province)
                .HasForeignKey(e => e.IdProvince);

            modelBuilder.Entity<Region>()
                .HasMany(e => e.Vehicles)
                .WithOptional(e => e.Region)
                .HasForeignKey(e => e.IdRegion);

            modelBuilder.Entity<TaxMapping>()
                .Property(e => e.BBNKB)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxMapping>()
                .Property(e => e.PKB)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxMapping>()
                .Property(e => e.SWDKLLJ)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxMapping>()
                .Property(e => e.ADMSTNK)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxMapping>()
                .Property(e => e.ADMTNKB)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxPayment>()
                .Property(e => e.BBNKB)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxPayment>()
                .Property(e => e.PKB)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxPayment>()
                .Property(e => e.SWDKLLJ)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxPayment>()
                .Property(e => e.ADMSTNK)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxPayment>()
                .Property(e => e.ADMTNKB)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxPayment>()
                .Property(e => e.BBNKB_add)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxPayment>()
                .Property(e => e.PKB_add)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxPayment>()
                .Property(e => e.SWDKLLJ_add)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxPayment>()
                .Property(e => e.ADMSTNK_add)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxPayment>()
                .Property(e => e.ADMTNKB_add)
                .HasPrecision(18, 0);

            modelBuilder.Entity<UserProfile>()
                .HasMany(e => e.TaxPayments)
                .WithRequired(e => e.UserProfile)
                .HasForeignKey(e => e.IdUserProfile)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserProfile>()
                .HasMany(e => e.Vehicles)
                .WithOptional(e => e.UserProfile)
                .HasForeignKey(e => e.IdUserProfile);

            modelBuilder.Entity<Vehicle>()
                .HasMany(e => e.TaxPayments)
                .WithRequired(e => e.Vehicle)
                .HasForeignKey(e => e.IdVehicle)
                .WillCascadeOnDelete(false);
        }
    }
}
