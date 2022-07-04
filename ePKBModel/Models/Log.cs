namespace ePKBModel.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Log")]
    public partial class Log
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        public DateTime CreateDate { get; set; }

        [StringLength(128)]
        public string IdObject { get; set; }

        [StringLength(512)]
        public string ValueBefore { get; set; }

        [StringLength(512)]
        public string ValueAfter { get; set; }
    }
}
