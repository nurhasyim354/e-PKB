using ePKBModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ePKB.PrintStation
{
    public class CustomTP
    {
        public string Id { get; set; }
        public string IdUserProfile { get; set; }

       
        public string IdVehicle { get; set; }

      
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

        public string Status { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        public string NIK { get; set; }
        public string FirstName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }


        public string PoliceNumber { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string FuelType { get; set; }
        public string Color { get; set; }
        public string EngineNumber { get; set; }
        public string BodyNumber { get; set; }
        public int? AssembleYear { get; set; }
        public int? Cylinder { get; set; }
        public string Merk { get; set; }
        public string Model { get; set; }
        public string TNKBColor { get; set; }
        public string BPKBNo { get; set; }
        public int? RegistrationYear { get; set; }
        public string Province { get; set; }
        public string Region { get; set; }
    }
}
