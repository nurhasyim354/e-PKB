using ePKBModel.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ePKB.guest.controllers
{
    [Authorize]
    [RoutePrefix("api/epkb")]
    public class epkbController : ApiController
    {
        AppModel context = new AppModel();
        [AllowAnonymous]
        [Route("sendmail")]
        public IHttpActionResult PostSendMail(JObject param)
        {
            var id = param["id"].ToString();
            var msg = param["msg"].ToString();

            var tp1 = context.TaxPayments.FirstOrDefault(p => p.Id == id);
            Mailer.notifEpkb(tp1, msg);
            return Ok();
        }

        [Route("save")]
        public async Task<IHttpActionResult> PostSave(RegParam param)
        {
            try
            {
                var user = await context.UserProfiles.FirstOrDefaultAsync(p => p.Id == param.UserProfile.Id);
                if (user == null)
                {
                    return BadRequest("Data tidak ditemukan!");
                }

                user.FirstName = param.UserProfile.FirstName;
                user.LastName = param.UserProfile.LastName;
                user.Address = param.UserProfile.Address;
                user.Contact = param.UserProfile.Contact;

                var clean_policenumber = param.Vehicle.PoliceNumber.Replace(" ", "");
                var vehicles = await context.Vehicles.Where(p => p.IdUserProfile == user.Id).ToListAsync();

                var e_vehicle =vehicles.FirstOrDefault(p => p.PoliceNumber.Replace(" ","") == clean_policenumber);
                if (e_vehicle == null)
                {
                    e_vehicle = new Vehicle();
                    e_vehicle.Id = Helpers.NewId();
                    e_vehicle.IdUserProfile = user.Id;
                    e_vehicle.CreateDate = DateTime.UtcNow;
                    context.Vehicles.Add(e_vehicle);
                }
                else
                    e_vehicle.LastUpdateDate = DateTime.UtcNow;

                e_vehicle.AssembleYear = param.Vehicle.AssembleYear;
                e_vehicle.BodyNumber = param.Vehicle.BodyNumber;
                e_vehicle.BPKBNo = param.Vehicle.BPKBNo;
                e_vehicle.Color = param.Vehicle.Color;
                e_vehicle.Cylinder = param.Vehicle.Cylinder;
                e_vehicle.EngineNumber = param.Vehicle.EngineNumber;
                e_vehicle.FuelType = param.Vehicle.FuelType;
                e_vehicle.Merk = param.Vehicle.Merk;
                e_vehicle.Model = param.Vehicle.Model;
                e_vehicle.RegistrationYear = param.Vehicle.RegistrationYear;
                e_vehicle.PoliceNumber = clean_policenumber;
                e_vehicle.Province = param.Vehicle.Province;
                e_vehicle.IdRegion = param.Vehicle.IdRegion;
                e_vehicle.TNKBColor = param.Vehicle.TNKBColor;
                e_vehicle.Type = param.Vehicle.Type;
                e_vehicle.Category = param.Vehicle.Category;
                e_vehicle.ExpireSTNKDate = param.Vehicle.ExpireSTNKDate;
                e_vehicle.Category = param.Vehicle.Category;

                var trxpayments = await context.TaxPayments.Where(p => p.IdUserProfile == user.Id && p.IdVehicle == e_vehicle.Id).ToListAsync();
                var trxpayment = trxpayments.Where(p => p.CreateDate.Year == DateTime.UtcNow.Year).FirstOrDefault();

                if (trxpayment != null)
                {
                    await context.SaveChangesAsync();
                    return Ok("EXIST");
                }
                   

                var tax_maping = await context.TaxMappings.Where(p => p.Model == e_vehicle.Model
           && p.AssembleYear == e_vehicle.AssembleYear
           && p.Cylinder == e_vehicle.Cylinder
           && p.FuelType == e_vehicle.FuelType
           ).FirstOrDefaultAsync();
                if (tax_maping == null)
                {
                    tax_maping = new TaxMapping()
                    {
                        ADMSTNK = 100000,
                        AssembleYear = e_vehicle.AssembleYear.GetValueOrDefault(),
                        ADMTNKB = 60000,
                        BBNKB = 0,
                        CreateDate = DateTime.UtcNow,
                        Cylinder = e_vehicle.Cylinder.GetValueOrDefault(),
                        FuelType = e_vehicle.FuelType,
                        Model = e_vehicle.Model,
                        PKB = 190000,
                        SWDKLLJ = 25000,
                    };
                    context.TaxMappings.Add(tax_maping);
                }

                if (trxpayment == null)
                {
                    trxpayment = new TaxPayment()
                    {
                        Id = Helpers.NewId(),
                        IdUserProfile = user.Id,
                        IdVehicle = e_vehicle.Id,
                        CreateDate = DateTime.UtcNow,
                        ExpireDate = DateTime.UtcNow,
                        Status = EPKB_STATUS.UPLOAD_DOCUMENT,
                        RegNumber = string.Format("{0}{1}{2}", DateTime.UtcNow.ToString("yyMMdd"), e_vehicle.PoliceNumber.Replace(" ", ""), Helpers.getRandom(4)).ToUpper(),
                        ADMSTNK = tax_maping.ADMSTNK,
                        ADMTNKB = 0,
                        BBNKB = tax_maping.BBNKB,
                        PKB = tax_maping.PKB,
                        SWDKLLJ = tax_maping.SWDKLLJ,
                    };
                    context.TaxPayments.Add(trxpayment);
                }
                else
                {
                    trxpayment.LastUpdateDate = DateTime.UtcNow;
                    trxpayment.ADMSTNK = tax_maping.ADMSTNK;
                    trxpayment.ADMTNKB = tax_maping.ADMTNKB;
                    trxpayment.BBNKB = tax_maping.BBNKB;
                    trxpayment.PKB = tax_maping.PKB;
                    trxpayment.SWDKLLJ = tax_maping.SWDKLLJ;
                }

                var year = DateTime.UtcNow.Year;
                var expyear = param.Vehicle.ExpireSTNKDate.GetValueOrDefault().Year;
                trxpayment.ExpireDate = param.Vehicle.ExpireSTNKDate.GetValueOrDefault().AddYears(year - expyear);
                await context.SaveChangesAsync();

                var tp1 = context.TaxPayments.FirstOrDefault(p => p.Id == trxpayment.Id);
                Mailer.registerEpkb(tp1);

                return Ok(trxpayment.Id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [Route("taxpayment")]
        public async Task<IHttpActionResult> PostTaxpayment(JObject param)
        {
            var id = param["id"].ToString();
            var status = param["status"] == null ? null : param["status"].ToString();
            var rotc = param["rotc"] == null || param["rotc"].ToString() != "true";
            var taxpayments = await context.TaxPayments.Where(p => p.Id == id).ToListAsync();
            if (taxpayments.Count == 0)
                return BadRequest("Data tidak ditemukan!");
            var res = from a in taxpayments
                      select new
                      {
                          a.Id,
                          a.RegNumber,
                          a.Status,
                          a.ADMSTNK,
                          a.ADMSTNK_add,
                          a.ADMTNKB,
                          a.ADMTNKB_add,
                          a.BBNKB,
                          a.BBNKB_add,
                          a.CreateDate,
                          a.PKB,
                          a.PKB_add,
                          a.SWDKLLJ,
                          a.SWDKLLJ_add,
                          a.ExpireDate,
                          readonlyTC = rotc,
                          UserProfile = new
                          {
                              a.UserProfile.Id,
                              a.UserProfile.FirstName,
                              a.UserProfile.Email,
                              a.UserProfile.Address,
                              a.UserProfile.NIK,
                              a.UserProfile.Contact,
                              a.UserProfile.VerificationDate,
                              IsValidate = status == EPKB_STATUS.VALIDATE_DATA,
                          },
                          Vehicle = new
                          {
                              a.Vehicle.Id,
                              a.Vehicle.PoliceNumber,
                              a.Vehicle.Type,
                              a.Vehicle.Model,
                              a.Vehicle.Merk,
                              a.Vehicle.Category,
                              a.Vehicle.ExpireSTNKDate,
                              a.Vehicle.FuelType,
                              a.Vehicle.Cylinder,
                              a.Vehicle.AssembleYear,
                              a.Vehicle.EngineNumber,
                              a.Vehicle.BodyNumber,
                              a.Vehicle.RegistrationYear,
                              a.Vehicle.VerificationDate,
                              a.Vehicle.Color,
                              a.Vehicle.TNKBColor,
                              IsValidate = status == EPKB_STATUS.VALIDATE_DATA,
                              Region = new
                              {
                                  a.Vehicle.Region.Name,
                                  Province = new
                                  {
                                      a.Vehicle.Region.Province.Name,
                                      Banks = from b in a.Vehicle.Region.Province.Banks
                                              select new
                                              {
                                                  b.AccountName,
                                                  b.AccountNumber,
                                                  b.BankName,
                                                  b.Branch
                                              },
                                  }
                              }
                          },
                          Images = Helpers.GetImages("/uploads", a.RegNumber),
                      };
            return Ok(res.FirstOrDefault());
        }
        [Route("images")]
        public IHttpActionResult PostImages(JObject param)
        {
            var regnumber = param["regnumber"].ToString();
            var res = Helpers.GetImages("/uploads", regnumber);
            return Ok(res);
        }
        [Route("userdata")]
        public async Task<IHttpActionResult> PostUserData(JObject param)
        {
            var id = param["id"].ToString();
            var res = from a in await context.UserProfiles.Where(p => p.Id == id).ToListAsync()
                      select new
                      {
                          a.Id,
                          a.FirstName,
                          a.Email,
                          a.NIK,
                          a.Address,
                          a.Contact,
                          Vehicles = from b in a.Vehicles
                                     select new
                                     {
                                         b.Id,
                                         b.IdRegion,
                                         b.LastUpdateDate,
                                         b.Merk,
                                         b.Model,
                                         b.PoliceNumber,
                                         b.Province,
                                         b.RegistrationYear,
                                         b.TNKBColor,
                                         b.Type,
                                         b.Category,
                                         b.ExpireSTNKDate,
                                         b.AssembleYear,
                                         b.BodyNumber,
                                         b.BPKBNo,
                                         b.Color,
                                         b.CreateDate,
                                         b.Cylinder,
                                         b.EngineNumber,
                                         b.FuelType,
                                         b.VerificationDate,
                                         IsProcessed = b.TaxPayments.Where(p=>p.CreateDate.Year == DateTime.UtcNow.Year).Any(),
                                     },
                          TaxPayments = from b in a.TaxPayments
                                        select new
                                        {
                                            b.Id,
                                            b.CreateDate,
                                            b.Status,
                                            b.RegNumber,
                                            Vehicle = new
                                            {
                                                b.Vehicle.PoliceNumber,
                                                b.Vehicle.Model,
                                            },
                                        }
                      };
            return Ok(res.FirstOrDefault());
        }
        [Route("updatestatus")]
        public async Task<IHttpActionResult> PostUpdateStatus(JObject param)
        {
            try
            {
                var id = param["id"].ToString();
                var status = param["status"].ToString();

                var tp = await context.TaxPayments.FirstOrDefaultAsync(p => p.Id == id);
                tp.Status = status;
                tp.LastUpdateDate = DateTime.UtcNow;
                await context.SaveChangesAsync();

                if (tp.Status == EPKB_STATUS.PRINT)
                {
                    var root = "/uploads";
                    var reg_dir = Path.Combine(root, tp.RegNumber);
                    var phy_dir = HttpContext.Current.Server.MapPath(reg_dir);
                    if (!Directory.Exists(phy_dir))
                        Directory.CreateDirectory(phy_dir);

                    var filename = string.Format("{0}_{1}.jpg", tp.RegNumber, "qrcode");
                    var path = Path.Combine(phy_dir, filename);
                    Helpers.GenerateQRCode(path, tp.RegNumber, 512, 512);
                    var v_path = Path.Combine(reg_dir, filename);
                    var tp1 = context.TaxPayments.FirstOrDefault(p => p.Id == tp.Id);

                    Mailer.paymentreceivedEpkb(tp1);
                }

                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.StackTrace);
            } 
        }
       
        [AllowAnonymous]
        [Route("getoptions")]
        public async Task<IHttpActionResult> PostGetOption(JObject param)
        {
            try
            {
                //var type = param["type"] == null ? null : param["type"].ToString();
                //var model = param["model"] == null ? null : param["model"].ToString();
                //var assembleyear = param["assembleyear"] == null ? null : param["assembleyear"].ToString();
                //var cylinder = param["cylinder"] == null ? null : param["cylinder"].ToString();
                //List<string> options = new List<string>();
                //switch (type)
                //{
                //    case "fueltype":
                //        var int_val_cylinder = int.Parse(cylinder);
                //        var int_val_year = int.Parse(assembleyear);
                //        options = (from a in (await context.TaxMappings.Where(p => p.Model == model && p.AssembleYear == int_val_year && p.Cylinder == int_val_cylinder).ToListAsync()).Select(p => p.FuelType).Distinct()
                //                   select a.ToString()).ToList();
                //        break;
                //    case "cylinder":
                //        int_val_year = int.Parse(assembleyear);
                //        options = (from a in (await context.TaxMappings.Where(p => p.Model == model && p.AssembleYear == int_val_year).ToListAsync()).Select(p => p.Cylinder).Distinct()
                //                   select a.ToString()).ToList();
                //        break;
                //    case "assembleyear":
                //        options = (from a in (await context.TaxMappings.Where(p => p.Model == model).ToListAsync()).Select(p => p.AssembleYear).Distinct()
                //                   select a.ToString()).ToList();
                //        break;
                //    default:
                //        options = await context.TaxMappings.Select(p => p.Model).Distinct().ToListAsync();
                //        break;
                //}

                var taxmappings = await context.TaxMappings.ToListAsync();
                var res = new
                {
                    Models = taxmappings.Select(p => p.Model).Distinct(),
                    AssembleYears = taxmappings.Select(p => p.AssembleYear).Distinct(),
                    Cylinders = taxmappings.Select(p => p.Cylinder).Distinct(),
                    FuelTypes = taxmappings.Select(p => p.FuelType).Distinct(),
                };

                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [Route("provinces")]
        public async Task<IHttpActionResult> GetProvinces()
        {
            var res = from a in await context.Provinces.ToListAsync()
                      select new
                      {
                          a.Id,
                          a.Name,
                          Regions = from b in a.Regions
                                    select new
                                    {
                                        b.Id,
                                        b.Name
                                    }
                      };
            return Ok(res);
        }
        [AllowAnonymous]
        [Route("checkemail")]
        public async Task<IHttpActionResult> PostCheckEmail(JObject param)
        {
            var email = param["email"].ToString();
            var res = await context.AspNetUsers.Where(p => p.UserName.ToLower() == email.ToLower()).FirstOrDefaultAsync();
            return Ok(res != null);
        }

        [AllowAnonymous]
        [Route("print")]
        public async Task< IHttpActionResult> PostPrint(JObject param)
        {
            var id = param["id"].ToString();
            var res = from a in await context.TaxPayments.Where(p => p.RegNumber == id).ToListAsync()
                      select new
                      {
                          a.Id,
                          a.RegNumber,
                          a.Status,
                          a.ADMSTNK,
                          a.ADMSTNK_add,
                          a.ADMTNKB,
                          a.ADMTNKB_add,
                          a.BBNKB,
                          a.BBNKB_add,
                          a.CreateDate,
                          a.PKB,
                          a.PKB_add,
                          a.SWDKLLJ,
                          a.SWDKLLJ_add,
                          a.ExpireDate,
                          UserProfile = new
                          {
                              a.UserProfile.Id,
                              a.UserProfile.FirstName,
                              a.UserProfile.Email,
                              a.UserProfile.Address,
                              a.UserProfile.NIK,
                              a.UserProfile.Contact,
                              a.UserProfile.VerificationDate,
                          },
                          Vehicle = new
                          {
                              a.Vehicle.Id,
                              a.Vehicle.PoliceNumber,
                              a.Vehicle.Type,
                              a.Vehicle.Model,
                              a.Vehicle.Merk,
                              a.Vehicle.Category,
                              a.Vehicle.ExpireSTNKDate,
                              a.Vehicle.FuelType,
                              a.Vehicle.Cylinder,
                              a.Vehicle.AssembleYear,
                              a.Vehicle.EngineNumber,
                              a.Vehicle.BodyNumber,
                              a.Vehicle.RegistrationYear,
                              a.Vehicle.VerificationDate,
                              a.Vehicle.Color,
                              a.Vehicle.TNKBColor,
                              a.Vehicle.BPKBNo,
                              Region = new
                              {
                                  a.Vehicle.Region.Name,
                                  Province = new
                                  {
                                      a.Vehicle.Region.Province.Name,
                                      Banks = from b in a.Vehicle.Region.Province.Banks
                                              select new
                                              {
                                                  b.AccountName,
                                                  b.AccountNumber,
                                                  b.BankName,
                                                  b.Branch
                                              },
                                  }
                              }
                          },
                      };

            if (res.Count() == 0)
                return BadRequest();

            if (res.FirstOrDefault().Status != EPKB_STATUS.PRINT)
                return BadRequest();

            return Ok(res.FirstOrDefault());
        }

    }

    [Authorize(Roles = "admin")]
    [RoutePrefix("api/admin")]
    public class adminController : ApiController
    {
        AppModel context = new AppModel();
        [Route("list")]
        public async Task<IHttpActionResult> GetList()
        {
            var res = from a in await context.TaxPayments.ToListAsync()
                      orderby a.CreateDate descending
                      select new
                      {
                          a.Id,
                          a.RegNumber,
                          a.CreateDate,
                          a.Status,
                          Vehicle = new
                          {
                              a.Vehicle.PoliceNumber,
                              a.Vehicle.Model,
                          },
                          UserProfile = new
                          {
                              a.UserProfile.FirstName,
                          }
                      };

            return Ok(res);
        }
        [Route("validate")]
        public async Task<IHttpActionResult> PostValidate(TaxPayment param)
        {
            var tp = await context.TaxPayments.FirstOrDefaultAsync(p => p.Id == param.Id);
            if (tp == null)
                return BadRequest("Data tidak ditemukan!");

            tp.PKB = param.PKB;
            tp.SWDKLLJ = param.SWDKLLJ;
            tp.ADMSTNK = param.ADMSTNK;
            tp.ADMTNKB = param.ADMTNKB;
            tp.PKB_add = param.PKB_add;
            tp.SWDKLLJ_add = param.SWDKLLJ_add;
            tp.ADMSTNK_add = param.ADMSTNK_add;
            tp.ADMTNKB_add = param.ADMTNKB_add;
            tp.Status = EPKB_STATUS.WAITING_PAYMENT;
            tp.LastUpdateDate = DateTime.UtcNow;
            tp.Vehicle.VerificationDate = DateTime.UtcNow;

            await context.SaveChangesAsync();
            var tp1 = context.TaxPayments.FirstOrDefault(p => p.Id == tp.Id);
            Mailer.datavalidatedEpkb(tp1);
            return Ok();
        }
    }
}

public class RegParam
{
    public UserProfile UserProfile { get; set; }
    public Vehicle Vehicle { get; set; }
    // public string Password { get; set; }
}