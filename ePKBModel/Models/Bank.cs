namespace ePKBModel.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Bank")]
    public partial class Bank
    {
        public int Id { get; set; }

        [StringLength(128)]
        public string BankName { get; set; }

        [StringLength(50)]
        public string AccountNumber { get; set; }

        [StringLength(128)]
        public string AccountName { get; set; }

        [StringLength(128)]
        public string Branch { get; set; }

        [StringLength(128)]
        public string IdProvince { get; set; }

        public virtual Province Province { get; set; }
    }
}
