namespace ePKBModel.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TaxMapping")]
    public partial class TaxMapping
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Model { get; set; }

        public int AssembleYear { get; set; }

        public int Cylinder { get; set; }

        [Required]
        [StringLength(50)]
        public string FuelType { get; set; }

        public decimal BBNKB { get; set; }

        public decimal PKB { get; set; }

        public decimal SWDKLLJ { get; set; }

        public decimal ADMSTNK { get; set; }

        public decimal ADMTNKB { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
