using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;


[RoutePrefix("api/file")]
public class FileController : ApiController
{
    string root = "/uploads";
    [Route("open")]
    public IHttpActionResult PostOpen(ItemInDirectory param)
    {
        List<ItemInDirectory> Items = new List<ItemInDirectory>();
        string path = HttpContext.Current.Server.MapPath(root + "/" + param.dirpath);

        List<string> protected_dirs = new List<string>() { "assets", "products", "promo", "review", "slideshow" };
        List<string> protected_files = new List<string>() { "_default.jpg" };

        //get directory
        foreach (var dir in Directory.GetDirectories(path))
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            Items.Add(new ItemInDirectory()
            {
                name = dirInfo.Name,
                type = "dir",
                fullpath = root + param.dirpath + '/' + dirInfo.Name,
                isProtected = protected_dirs.Where(p=>p.ToLower() == dirInfo.Name.ToLower()).Any(),
            });
        }

        //get files
        foreach (var file in Directory.GetFiles(path))
        {
            FileInfo info = new FileInfo(file);
            Items.Add(new ItemInDirectory()
            {
                name = info.Name,
                type = IsImageFile(info.FullName) ? "image" : "file",
                fullpath = root + param.dirpath + '/' + info.Name,
                isProtected = protected_files.Where(p => p.ToLower() == info.Name.ToLower()).Any(),
            });
        }
        return Ok(Items);
    }

    [Route("deletefm")]
    public IHttpActionResult PostDeleteFM(ItemInDirectory param)
    {
        string path = Path.Combine(HttpContext.Current.Server.MapPath(root + param.dirpath), param.name);

        if (param.type == "dir")
            Directory.Delete(path, true);
        else
            File.Delete(path);

        return Ok();
    }

    [Route("rename")]
    public IHttpActionResult PostRename(ItemInDirectory param)
    {
        string path = Path.Combine(HttpContext.Current.Server.MapPath(root + param.dirpath), param.name);
        string newname = Path.Combine(HttpContext.Current.Server.MapPath(root + param.dirpath), param.newname);

        if (param.type == "dir")
        {
            Directory.Move(path, newname);
        }
        else
        {
            if (Path.GetFileNameWithoutExtension(path).ToLower() == Path.GetFileNameWithoutExtension(newname).ToLower())
                return Ok();

            string ext = Path.GetExtension(path);
            if (string.IsNullOrEmpty(Path.GetExtension(newname)))
                newname = newname + ext;

            File.Copy(path, newname);
            File.Delete(path);
        }
        return Ok();
    }

    [Route("addfolder")]
    public IHttpActionResult PostAddFolder(ItemInDirectory param)
    {
        string newname = Path.Combine(HttpContext.Current.Server.MapPath(root + param.dirpath), param.newname);
        try
        {
            Directory.CreateDirectory(newname);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        return Ok();
    }

    [Route("upload")]
    public async Task<IHttpActionResult> PostUpload()
    {
        try
        {
            int max_width = 600;
            string dir = HttpContext.Current.Request.Form["dir"];
            string prefix = HttpContext.Current.Request.Form["prefix"];
            string w_str = HttpContext.Current.Request.Form["width"];
            if (!string.IsNullOrEmpty(w_str))
            {
                try
                {
                    max_width = int.Parse(w_str);
                }
                catch (Exception ex)
                {
                }
            }

            string c = Path.Combine(root, dir);

            string destination = HttpContext.Current.Server.MapPath(c);
            string fileName = "";
            if (!Directory.Exists(destination)) Directory.CreateDirectory(destination);

            //TEMP Folder
            string temp = Path.Combine(HttpContext.Current.Server.MapPath(root), "temp" + DateTime.UtcNow.Ticks);
            string finalfilename = "";
            if (!Directory.Exists(temp))
                Directory.CreateDirectory(temp);

            if (Request.Content.IsMimeMultipartContent())
            {
                var streamProvider = new MultipartFormDataStreamProvider(temp);
                await Request.Content.ReadAsMultipartAsync(streamProvider);

                if (streamProvider.FileData.Count() == 0)
                {
                    Directory.Delete(temp, true);
                    return BadRequest("No file uploaded");
                }

                foreach (MultipartFileData fileData in streamProvider.FileData)
                {
                    if (string.IsNullOrEmpty(fileData.Headers.ContentDisposition.FileName))
                    {
                        Directory.Delete(temp, true);
                        return BadRequest("Empty filename");
                    }
                    fileName = fileData.Headers.ContentDisposition.FileName;
                    fileName = fileName.Trim().Trim('"');

                    if (!string.IsNullOrEmpty(prefix))
                    {
                        var ext = Path.GetExtension(fileName);
                        fileName = Helpers.getRandom(6) + ext;
                        fileName = string.Format("{0}_{1}", prefix, fileName);
                    }
                    finalfilename = Path.Combine(destination, fileName);
                    try
                    {
                        if (IsImageFile(finalfilename))
                        {
                            Image img = new Bitmap(fileData.LocalFileName);
                            int limit_w = max_width;
                            if (img.Width > limit_w)
                            {
                                int w = limit_w;
                                int h = limit_w * img.Height / img.Width;
                                img.SaveCompressed(finalfilename, new Size(w, h), 80L);
                            }
                            else
                                File.Copy(fileData.LocalFileName, finalfilename, true);
                            img.Dispose();
                        }
                        else
                            File.Copy(fileData.LocalFileName, finalfilename, true);
                    }
                    catch (Exception ex)
                    {
                        Directory.Delete(temp, true);
                        return BadRequest(ex.Message);
                    }
                }
                Directory.Delete(temp, true);

                if(!string.IsNullOrEmpty(prefix))
                {
                    prefix = prefix.Split(new string[] { Constant.prefix_sort }, StringSplitOptions.None)[0];
                    var res = Helpers.GetImages(root, dir);
                    return Ok(res);
                }
                return Ok();
            }
            else
            {
                Directory.Delete(temp, true);
                return BadRequest("This request is not properly formatted");
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private bool IsImageFile(string finalfilename)
    {
        string ext = Path.GetExtension(finalfilename).ToLower();
        if (ext == ".jpg"
            || ext == ".jpeg"
            || ext == ".png"
            || ext == ".bmp"
            )
            return true;
        else
            return false;
    }

    [Route("sort")]
    public IHttpActionResult PostSort(List<FileModel> param)
    {
        try
        {
            //file name structure {im}[IDMODEL]{sk}[PO_SKU]{so}[ORDER]_[FILENAME]

            int order = 0;
            foreach (var f in param)
            {
                var v_filepath = f.Path;
                var v_dir = v_filepath.Replace(Path.GetFileName(v_filepath), "");
                var file = HttpContext.Current.Server.MapPath(v_filepath);
                string path = new FileInfo(file).DirectoryName;
                string newfilename = Constant.prefix_idmodel;
                string filename = Path.GetFileName(file);
                string ext = Path.GetExtension(file);
                var split = filename.Split(new string[] { Constant.prefix_sort }, StringSplitOptions.None);
                if (split.Count() == 1)
                {
                    split = filename.Split('_');
                    string idmodel = split[0];
                    string sku = split[1];
                    newfilename = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", newfilename, idmodel, Constant.prefix_sku, sku, Constant.prefix_sort, string.Format("{0:000}", order), Constant.prefix_filename, Helpers.getRandom(4), ext);
                }
                else
                {
                    string prefix = split[0];
                    var split_order = split[1].Split(new string[] { Constant.prefix_filename }, StringSplitOptions.None);
                    if (split_order.Count() > 1)
                    {
                        string fi = Constant.prefix_filename + split_order[1];
                        string so = Constant.prefix_sort + string.Format("{0:000}", order);
                        newfilename = string.Format("{0}{1}{2}", prefix, so, fi);
                    }
                }
                order++;
                string src = file;
                //create temp first to avoid file used if in same position
                string temp = src + "t";
                File.Copy(src, temp);
                File.Delete(src);

                string dest = Path.Combine(path, newfilename);
                File.Copy(temp, dest, true);
                File.Delete(temp);

                f.Path = string.Format("{0}{1}", v_dir, newfilename);

            }
            return Ok(param);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Route("delete")]
    public IHttpActionResult PostDeleteFile(FileModel param)
    {
        var path = HttpContext.Current.Server.MapPath(param.Path);
        if (File.Exists(path))
            File.Delete(path);
        return Ok();
    }

    public class ItemInDirectory
    {
        public string type { get; set; }
        public string name { get; set; }
        public string newname { get; set; }
        public string fullpath { get; set; }
        public string dirpath { get; set; }
        public bool isProtected { get; set; }
    }

}

public class ItemInDirectory
{
    public string type { get; set; }
    public string name { get; set; }
    public string newname { get; set; }
    public string fullpath { get; set; }
    public string dirpath { get; set; }
    public bool isProtected { get; set; }
}

public class FileModel
{
    public FileModel()
    {
        IsDeletable = true;
    }
    public string Path { get; set; }
    public string Name { get; set; }
    public bool IsDeletable { get; set; }
}

public class Constant
{
    public static string prefix_sort { get; internal set; }
    public static string prefix_idmodel { get; internal set; }
    public static object prefix_sku { get; internal set; }
    public static string prefix_filename { get; internal set; }
}