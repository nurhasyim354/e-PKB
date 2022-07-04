namespace ePKBModel.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TaxPayment")]
    public partial class TaxPayment
    {
        public string Id { get; set; }

        [Required]
        [StringLength(128)]
        public string IdUserProfile { get; set; }

        [Required]
        [StringLength(128)]
        public string IdVehicle { get; set; }

        [Required]
        [StringLength(50)]
        public string RegNumber { get; set; }

        public decimal BBNKB { get; set; }

        public decimal PKB { get; set; }

        public decimal SWDKLLJ { get; set; }

        public decimal ADMSTNK { get; set; }

        public decimal ADMTNKB { get; set; }

        public decimal BBNKB_add { get; set; }

        public decimal PKB_add { get; set; }

        public decimal SWDKLLJ_add { get; set; }

        public decimal ADMSTNK_add { get; set; }

        public decimal ADMTNKB_add { get; set; }

        public DateTime ExpireDate { get; set; }

        public DateTime CreateDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        public virtual UserProfile UserProfile { get; set; }

        public virtual Vehicle Vehicle { get; set; }
    }
}
