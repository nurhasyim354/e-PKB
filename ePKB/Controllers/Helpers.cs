using ePKBModel.Models;
using MessagingToolkit.QRCode.Codec;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;

public static class Helpers
{
    public static string NewId()
    {
        return DateTime.UtcNow.Ticks + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6);
    }
    public static string EmailTemplate(string body, string template = "generic")
    {
        StreamReader reader = File.OpenText(HttpContext.Current.Server.MapPath("/Guest/email/master.html"));
        string emailText = reader.ReadToEnd();
        reader.Close();

        if (template != "generic")
        {
            reader = File.OpenText(HttpContext.Current.Server.MapPath("/Guest/email/" + template + ".html"));
            var emailTextInside = reader.ReadToEnd();
            reader.Close();
            body = emailTextInside.Replace("\"", "'");
        }

        return String.Format(emailText.Replace("\"", "'"), body, HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority));
    }

    public static void SaveCompressed(this Image image, string filename, Size newSize, long compressingLevel)
    {
        Image resultImg = new Bitmap(newSize.Width, newSize.Height);
        using (Graphics g = Graphics.FromImage(resultImg))
        {
            g.Clear(System.Drawing.Color.White);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.DrawImage(image, 0, 0, newSize.Width, newSize.Height);
        }
        var encoder = GetImageEncoder(filename);
        var encParams = new EncoderParameters(1);
        encParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, compressingLevel);
        resultImg.Save(filename, encoder, encParams);
    }
    public static ImageCodecInfo GetImageEncoder(string file)
    {
        ImageFormat format = format = ImageFormat.Jpeg;
        string ext = Path.GetExtension(file);
        switch (ext.ToLower())
        {
            case "jpg": format = ImageFormat.Jpeg; break;
            case "jpeg": format = ImageFormat.Jpeg; break;
            case "png": format = ImageFormat.Png; break;
            case "bmp": format = ImageFormat.Bmp; break;
        }
        return ImageCodecInfo.GetImageEncoders().ToList().Find(delegate (ImageCodecInfo codec)
        {
            return codec.FormatID == format.Guid;
        });
    }

    public static string getRandom(int length, string charSet = "")
    {
        string ALPHA = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string NUMERIC = "0123456789";
        string ALPHA_NUMERIC = ALPHA + NUMERIC;

        switch (charSet)
        {
            case "ALPHA":
                charSet = ALPHA;
                break;
            case "NUMERIC":
                charSet = NUMERIC;
                break;
            default:
                charSet = ALPHA_NUMERIC;
                break;
        }
        string randomData = "";
        int position = 0;
        byte[] data = new byte[length];
        int characterSetLength = charSet.Length;
        System.Security.Cryptography.RandomNumberGenerator random = System.Security.Cryptography.RandomNumberGenerator.Create();
        random.GetBytes(data);
        for (int index = 0; (index < length); index++)
        {
            position = data[index];
            position = (position % characterSetLength);
            randomData = (randomData + charSet.Substring(position, 1));
        }
        return randomData;
    }

    internal static Object GetImages(string root, string dir)
    {
        string destination = HttpContext.Current.Server.MapPath(Path.Combine(root, dir));
        if (!Directory.Exists(destination))
            Directory.CreateDirectory(destination);

        var files = Directory.GetFiles(destination).Where(p => Path.GetFileNameWithoutExtension(p).ToLower().StartsWith(string.Format("{0}", dir).ToLower())).ToList();
        var ktp = Path.GetFileName(files.FirstOrDefault(p => Path.GetFileName(p).Contains("ktp")));
        var stnk = Path.GetFileName(files.FirstOrDefault(p => Path.GetFileName(p).Contains("stnk")));
        var ssp = Path.GetFileName(files.FirstOrDefault(p => Path.GetFileName(p).Contains("ssp")));
        var bpkb = Path.GetFileName(files.FirstOrDefault(p => Path.GetFileName(p).Contains("bpkb")));
        var proof = Path.GetFileName(files.FirstOrDefault(p => Path.GetFileName(p).Contains("proof")));
        var qrcode = Path.GetFileName(files.FirstOrDefault(p => Path.GetFileName(p).Contains("qrcode")));

        return new
        {
            ktp = string.IsNullOrEmpty(ktp) ? null : new FileModel() { Path = string.Format("{0}/{1}/{2}", root, dir, ktp) },
            stnk = string.IsNullOrEmpty(stnk) ? null : new FileModel() { Path = string.Format("{0}/{1}/{2}", root, dir, stnk) },
            ssp = string.IsNullOrEmpty(ssp) ? null :  new FileModel() { Path = string.Format("{0}/{1}/{2}", root, dir, ssp) },
            bpkb = string.IsNullOrEmpty(bpkb) ? null : new FileModel() { Path = string.Format("{0}/{1}/{2}", root, dir, bpkb) },
            proof = string.IsNullOrEmpty(proof) ? null : new FileModel() { Path = string.Format("{0}/{1}/{2}", root, dir, proof) },
            qrcode = string.IsNullOrEmpty(qrcode) ? null : new FileModel() { Path = string.Format("{0}/{1}/{2}", root, dir, qrcode) },
        };
    }

    internal static string GenerateQRCode(string path, string data, int width, int height)
    {
        QRCodeEncoder encoder = new QRCodeEncoder();
        encoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.H;
        encoder.QRCodeScale = 5;
        int borderThickness = 2;
        
        Size resultSize = new System.Drawing.Size(width + borderThickness, height + borderThickness);
        Bitmap result = new Bitmap(resultSize.Width, resultSize.Height);
        using (Bitmap img = encoder.Encode(data))
        {
            var resized = new Bitmap(img, new Size(width - 10, height - 10));
            Size logoSize = new Size(resized.Width * 10 / 100, resized.Height * 10 / 100);
            //System.Drawing.Image logo = new Bitmap(Image.FromFile(logoPath), logoSize);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(System.Drawing.Color.White);
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(resized, new System.Drawing.Point { X = (result.Width - resized.Width) / 2, Y = (result.Height - resized.Height) / 2 });
                //g.DrawImage(logo, new System.Drawing.Point { X = (result.Width - logo.Width) / 2, Y = (result.Height - logo.Height) / 2 });
            }
        }
        result.Save(path);
        result.Dispose();
        return path;
    }

    public static decimal GetTotal(this TaxPayment tp)
    {
        var total_pokok = tp.PKB + tp.SWDKLLJ + tp.ADMSTNK + tp.ADMTNKB;
        var total_add = tp.PKB_add + tp.SWDKLLJ_add + tp.ADMSTNK_add + tp.ADMTNKB_add;
        return total_pokok + total_add;
    }
}
