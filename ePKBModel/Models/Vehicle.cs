namespace ePKBModel.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Vehicle")]
    public partial class Vehicle
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Vehicle()
        {
            TaxPayments = new HashSet<TaxPayment>();
        }

        public string Id { get; set; }

        [Required]
        [StringLength(50)]
        public string PoliceNumber { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        [StringLength(50)]
        public string Type { get; set; }

        [StringLength(50)]
        public string FuelType { get; set; }

        [StringLength(50)]
        public string Color { get; set; }

        [StringLength(50)]
        public string EngineNumber { get; set; }

        [StringLength(50)]
        public string BodyNumber { get; set; }

        public int? AssembleYear { get; set; }

        public int? Cylinder { get; set; }

        [StringLength(50)]
        public string Merk { get; set; }

        [StringLength(50)]
        public string Model { get; set; }

        [StringLength(128)]
        public string TNKBColor { get; set; }

        [StringLength(128)]
        public string BPKBNo { get; set; }

        public int? RegistrationYear { get; set; }

        [StringLength(128)]
        public string Province { get; set; }

        [StringLength(128)]
        public string IdRegion { get; set; }

        [StringLength(128)]
        public string IdUserProfile { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        public DateTime? ExpireSTNKDate { get; set; }

        public DateTime? VerificationDate { get; set; }

        public virtual Region Region { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TaxPayment> TaxPayments { get; set; }

        public virtual UserProfile UserProfile { get; set; }
    }
}
