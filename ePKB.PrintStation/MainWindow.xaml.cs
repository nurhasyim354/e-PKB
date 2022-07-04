using ePKBModel.Models;
using Microsoft.Reporting.WinForms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ePKB.PrintStation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string server = ConfigurationManager.AppSettings["server"];

        public bool isProcessing { get; private set; }
        public DispatcherTimer dispatcherTimer { get; private set; }
        public int counter { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            PageSettings pg = new PageSettings();
            pg.Margins.Top = 0;
            pg.Margins.Bottom = 0;
            pg.Margins.Left = 0;
            pg.Margins.Right = 0;
            pg.Landscape = false;

            _reportViewer.ShowBackButton = false;
            _reportViewer.ShowContextMenu = false;
            _reportViewer.ShowPageNavigationControls = false;
            _reportViewer.ShowFindControls = false;
            _reportViewer.ShowProgress = false;
            _reportViewer.ShowStopButton = false;
            _reportViewer.ShowRefreshButton = false;
            _reportViewer.ZoomMode = ZoomMode.PageWidth;
            _reportViewer.SetPageSettings(pg);

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);

        }

        private void camSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            webCam.CameraIndex = camSelect.SelectedIndex;
        }

        private void QrWebCamControl_QrDecoded(object sender, string e)
        {
            dtext.Text = e;
            GetData(dtext.Text);
        }

        private async Task GetData(string e)
        {
            if (isProcessing)
                return;

            isProcessing = true;
            Cursor = Cursors.Wait;
            var param = new
            {
                id = e,
            };
            string url = string.Format("{0}/api/epkb/print", server);
            var res = await PostWebRequestAsync(url, JsonConvert.SerializeObject(param), true);
            Cursor = Cursors.Arrow;

            if (res.IsSuccessStatusCode)
            {
                var content = res.Content.ReadAsStringAsync().Result;
                var taxpayment = JsonConvert.DeserializeObject<TaxPayment>(content);
                ShowReport(taxpayment);
                isProcessing = false;
            }
            else
            {
                isProcessing = false;
                MessageBox.Show("DATA TIDAK DITEMUKAN!");
            }
               

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            camSelect.ItemsSource = webCam.CameraNames;
            if (webCam.CameraNames.Count > 0)
                camSelect.SelectedIndex = 0;
        }

        private void _reportViewer_RenderingComplete(object sender, Microsoft.Reporting.WinForms.RenderingCompleteEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _reportViewer.PrintDialog();
        }

        private void ShowReport(TaxPayment data)
        {
            try
            {
                CustomTP customTP = new CustomTP()
                {
                    Id = data.Id,
                    IdUserProfile = data.IdUserProfile,
                    IdVehicle = data.IdVehicle,
                    RegNumber = data.RegNumber,
                    BBNKB = data.BBNKB,
                    PKB = data.PKB,
                    SWDKLLJ = data.SWDKLLJ,
                    ADMSTNK = data.ADMSTNK,
                    ADMTNKB = data.ADMTNKB,
                    BBNKB_add = data.BBNKB_add,
                    PKB_add = data.PKB_add,
                    SWDKLLJ_add = data.SWDKLLJ_add,
                    ADMSTNK_add = data.ADMSTNK_add,
                    ADMTNKB_add = data.ADMTNKB_add,
                    ExpireDate = data.ExpireDate,
                    CreateDate = data.CreateDate,
                    Status = data.Status,
                    LastUpdateDate = data.LastUpdateDate,
                    PoliceNumber = data.Vehicle.PoliceNumber,
                    Category = data.Vehicle.Category,
                    Type = data.Vehicle.Type,
                    FuelType = data.Vehicle.FuelType,
                    Color = data.Vehicle.Color,
                    EngineNumber = data.Vehicle.EngineNumber,
                    BodyNumber = data.Vehicle.BodyNumber,
                    AssembleYear = data.Vehicle.AssembleYear,
                    Cylinder = data.Vehicle.Cylinder,
                    Merk = data.Vehicle.Merk,
                    Model = data.Vehicle.Model,
                    TNKBColor = data.Vehicle.TNKBColor,
                    BPKBNo = data.Vehicle.BPKBNo,
                    RegistrationYear = data.Vehicle.RegistrationYear,
                    Province = data.Vehicle.Province,
                    Region = data.Vehicle.Region.Name,
                    FirstName = data.UserProfile.FirstName,
                    Address = data.UserProfile.Address,
                    Contact = data.UserProfile.Contact,
                    Email = data.UserProfile.Email,
                    NIK = data.UserProfile.NIK,

                };


                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("Id", typeof(string)));
                dt.Columns.Add(new DataColumn("IdUserProfile", typeof(string)));
                dt.Columns.Add(new DataColumn("IdVehicle", typeof(string)));
                dt.Columns.Add(new DataColumn("RegNumber", typeof(string)));
                dt.Columns.Add(new DataColumn("BBNKB", typeof(string)));
                dt.Columns.Add(new DataColumn("PKB", typeof(string)));
                dt.Columns.Add(new DataColumn("SWDKLLJ", typeof(string)));
                dt.Columns.Add(new DataColumn("ADMSTNK", typeof(string)));
                dt.Columns.Add(new DataColumn("ADMTNKB", typeof(string)));
                dt.Columns.Add(new DataColumn("BBNKB_add", typeof(string)));
                dt.Columns.Add(new DataColumn("PKB_add", typeof(string)));
                dt.Columns.Add(new DataColumn("SWDKLLJ_add", typeof(string)));
                dt.Columns.Add(new DataColumn("ADMSTNK_add", typeof(string)));
                dt.Columns.Add(new DataColumn("ADMTNKB_add", typeof(string)));
                dt.Columns.Add(new DataColumn("ExpireDate", typeof(string)));
                dt.Columns.Add(new DataColumn("CreateDate", typeof(string)));
                dt.Columns.Add(new DataColumn("Status", typeof(string)));
                dt.Columns.Add(new DataColumn("LastUpdateDate", typeof(string)));
                dt.Columns.Add(new DataColumn("PoliceNumber", typeof(string)));
                dt.Columns.Add(new DataColumn("Category", typeof(string)));
                dt.Columns.Add(new DataColumn("Type", typeof(string)));
                dt.Columns.Add(new DataColumn("FuelType", typeof(string)));
                dt.Columns.Add(new DataColumn("Color", typeof(string)));
                dt.Columns.Add(new DataColumn("EngineNumber", typeof(string)));
                dt.Columns.Add(new DataColumn("BodyNumber", typeof(string)));
                dt.Columns.Add(new DataColumn("AssembleYear", typeof(string)));
                dt.Columns.Add(new DataColumn("Cylinder", typeof(string)));
                dt.Columns.Add(new DataColumn("Merk", typeof(string)));
                dt.Columns.Add(new DataColumn("Model", typeof(string)));
                dt.Columns.Add(new DataColumn("TNKBColor", typeof(string)));
                dt.Columns.Add(new DataColumn("BPKBNo", typeof(string)));
                dt.Columns.Add(new DataColumn("RegistrationYear", typeof(string)));
                dt.Columns.Add(new DataColumn("Province", typeof(string)));
                dt.Columns.Add(new DataColumn("Region", typeof(string)));
                //dt.Columns.Add(new DataColumn("IdRegion", typeof(string)));
                //dt.Columns.Add(new DataColumn("IdUserProfile", typeof(string)));
                //dt.Columns.Add(new DataColumn("CreateDate", typeof(string)));
                //dt.Columns.Add(new DataColumn("LastUpdateDate", typeof(string)));
                //dt.Columns.Add(new DataColumn("ExpireSTNKDate", typeof(string)));
                //dt.Columns.Add(new DataColumn("VerificationDate", typeof(string)));
                dt.Columns.Add(new DataColumn("FirstName", typeof(string)));
                dt.Columns.Add(new DataColumn("Address", typeof(string)));
                dt.Columns.Add(new DataColumn("Contact", typeof(string)));
                dt.Columns.Add(new DataColumn("Email", typeof(string)));
                dt.Columns.Add(new DataColumn("NIK", typeof(string)));


                DataRow dr = dt.NewRow();
                dr["Id"] = customTP.Id;
                dr["IdUserProfile"] = customTP.IdUserProfile;
                dr["IdVehicle"] = customTP.IdVehicle;
                dr["RegNumber"] = customTP.RegNumber;
                dr["BBNKB"] = customTP.BBNKB;
                dr["PKB"] = customTP.PKB;
                dr["SWDKLLJ"] = customTP.SWDKLLJ;
                dr["ADMSTNK"] = customTP.ADMSTNK;
                dr["ADMTNKB"] = customTP.ADMTNKB;
                dr["BBNKB_add"] = customTP.BBNKB_add;
                dr["PKB_add"] = customTP.PKB_add;
                dr["SWDKLLJ_add"] = customTP.SWDKLLJ_add;
                dr["ADMSTNK_add"] = customTP.ADMSTNK_add;
                dr["ADMTNKB_add"] = customTP.ADMTNKB_add;
                dr["ExpireDate"] = customTP.ExpireDate;
                dr["CreateDate"] = customTP.CreateDate;
                dr["Status"] = customTP.Status;
                dr["LastUpdateDate"] = customTP.LastUpdateDate;
                dr["PoliceNumber"] = customTP.PoliceNumber;
                dr["Category"] = customTP.Category;
                dr["Type"] = customTP.Type;
                dr["FuelType"] = customTP.FuelType;
                dr["Color"] = customTP.Color;
                dr["EngineNumber"] = customTP.EngineNumber;
                dr["BodyNumber"] = customTP.BodyNumber;
                dr["AssembleYear"] = customTP.AssembleYear;
                dr["Cylinder"] = customTP.Cylinder;
                dr["Merk"] = customTP.Merk;
                dr["Model"] = customTP.Model;
                dr["TNKBColor"] = customTP.TNKBColor;
                dr["BPKBNo"] = customTP.BPKBNo;
                dr["RegistrationYear"] = customTP.RegistrationYear;
                dr["Province"] = customTP.Province;
                dr["Region"] = customTP.Region;
                //dr["IdRegion"] = customTP.IdRegion;
                //dr["IdUserProfile"] = customTP.IdUserProfile;
                //dr["CreateDate"] = customTP.CreateDate;
                //dr["LastUpdateDate"] = customTP.LastUpdateDate;
                //dr["ExpireSTNKDate"] = customTP.ExpireSTNKDate;
                //dr["VerificationDate"] = customTP.VerificationDate;
                dr["FirstName"] = customTP.FirstName;
                dr["Address"] = customTP.Address;
                dr["Contact"] = customTP.Contact;
                dr["Email"] = customTP.Email;
                dr["NIK"] = customTP.NIK;

                dt.Rows.Add(dr);

                ReportDataSource reportDataSource = new ReportDataSource();
                reportDataSource.Name = "DataSet1"; // Name of the DataSet we set in .rdlc
                reportDataSource.Value = dt;
                _reportViewer.LocalReport.ReportEmbeddedResource = "ePKB.PrintStation.Report2.rdlc"; // Path of the rdlc file
                _reportViewer.LocalReport.DataSources.Add(reportDataSource);

               
                _reportViewer.RefreshReport();
                host_viewer.Visibility = Visibility.Visible;

               
                dispatcherTimer.Start();
                counter = 0;
            }
            catch (Exception ex)
            {

            }

        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (counter >= 10)
            {
                dispatcherTimer.Stop();
                host_viewer.Visibility = Visibility.Collapsed;
                dtext.Text = "";
                dispatcherTimer.Stop();
            }
            else
                counter++;
         
        }

        public static async Task<HttpResponseMessage> PostWebRequestAsync(string url, string data, bool isJson = false, int timeout = 20)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(timeout);
                    return await client.PostAsync(url, new StringContent(data, Encoding.UTF8, isJson ? "application/json" : "application/x-www-form-urlencoded"));
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            GetData(dtext.Text);
        }
    }
}
