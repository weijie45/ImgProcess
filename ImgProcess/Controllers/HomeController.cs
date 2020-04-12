using Dapper;
using DapperExtensions;
using FilesIO;
using ImageMagick;
using ImgProcess.Hubs;
using ImgProcess.Infrastructure;
using Log;
using Newtonsoft.Json;
using Service.Function.Base;
using Service.Function.Common;
using Service.Function.Extensions;
using Service.Model;
using Settings;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace ImgProcess.Controllers
{
    public class HomeController : BaseController
    {
        // Fields
        private LogContext _logContext = null;
        private SettingsContext _settingsContext = null;
        private string Sql = "";

        // Constructors
        public HomeController()
        {

        }

        // Properties
        public LogContext LogContext
        {
            get
            {
                // Creat
                if (_logContext == null) {
                    // Resolve
                    _logContext = new Log.LogContextModule().Create("Test");
                    if (_logContext == null) throw new InvalidOperationException("_logContextt=null");
                }

                // Return
                return _logContext;
            }
        }

        public SettingsContext SettingsContext
        {
            get
            {
                // Creat
                if (_settingsContext == null) {
                    // Resolve
                    _settingsContext = new Settings.SettingsContextModule().Create("Test");
                    if (_settingsContext == null) throw new InvalidOperationException("_settingsContext=null");
                }

                // Return
                return _settingsContext;
            }
        }

        // Methods  
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Photo()
        {
            var startDate = Request["StartDate"].FixReq();
            var endDate = Request["EndDate"].FixReq();
            startDate = startDate == "" ? "20200301" : startDate;
            endDate = endDate == "" ? "20200404" : endDate;

            using (var db = new SqlConnection(this.SettingsContext.GetValue("Test"))) {
                return View(db.Query<PhotoModel>($"Select * From Photo Where OrgCreateDateTime Between '{startDate}' AND '{endDate}' Order by ModifyDateTime Desc "));
            }
        }

        public ActionResult TimeLIne()
        {
            return View();
        }

        public ActionResult Calander()
        {

            Sql = " Select ";
            Sql += "     LEFT(CONVERT(varchar, OrgCreateDateTime, 111), 7) YYYYMM, ";
            Sql += "     PhotoSN, ";
            Sql += "     FileName, ";
            Sql += "     FileExt, ";
            Sql += "     Width, ";
            Sql += "     Height, ";
            Sql += "     OrgCreateDateTime, ";
            Sql += "     ROW_NUMBER() OVER(PARTITION BY LEFT(CONVERT(varchar, OrgCreateDateTime, 111), 7) ORDER BY OrgCreateDateTime DESC) ";
            Sql += " From Photo ";
            Sql += "  Order by YYYYMM Desc ";
            //Sql += $" Where LEFT(CONVERT(varchar, OrgCreateDateTime,111),7) = '2018/12' ";

            using (var db = new SqlConnection(this.SettingsContext.GetValue("Test"))) {
                ViewBag.PhotoList = db.Query(Sql).ToList();
            }

            return View();
        }

        public ActionResult Slideshow()
        {

            Sql = "Select p.FileName, ";
            Sql += "p.OrgCreateDateTime, ";
            Sql += "d.Photo ";
            Sql += "From Photo p ";
            Sql += "RIGHT JOIN PhotoDetail d ON p.PhotoSN = d.PhotoDetailSN ";
            Sql += "Where p.PhotoSN IN(1,2,3) ";
            Sql += "AND d.Type = 'Origin' ";

            using (var db = new SqlConnection(this.SettingsContext.GetValue("Test"))) {
                var data = db.Query(Sql).ToList();
                //var photoList = new List<string>();
                //foreach (var d in data) {
                //    photoList.Add("data:image/jpg;base64," + Convert.ToBase64String(d.Photo));
                //}
                ViewBag.PhotoList = data;
            }

            return View();
        }

        [HttpGet]
        public void Download(string fileName, string downName)
        {
            //string fileName = "Test.jpg";//客戶端儲存的檔名
            string filePath = $"C:\\inetpub\\wwwroot\\Photo\\Temp\\{fileName}";//路徑

            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists == true) {
                const long ChunkSize = 102400;//100K 每次讀取檔案，只讀取100K，這樣可以緩解伺服器的壓力
                byte[] buffer = new byte[ChunkSize];

                Response.Clear();
                FileStream iStream = System.IO.File.OpenRead(filePath);
                long dataLengthToRead = iStream.Length;//獲取下載的檔案總大小
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(downName));
                while (dataLengthToRead > 0 && Response.IsClientConnected) {
                    int lengthRead = iStream.Read(buffer, 0, Convert.ToInt32(ChunkSize));//讀取的大小
                    Response.OutputStream.Write(buffer, 0, lengthRead);
                    Response.Flush();
                    dataLengthToRead = dataLengthToRead - lengthRead;
                }
                Response.Close();
            }
        }

        [HttpPost]
        /// <summary>
        /// 製作上傳圖片的縮圖
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPhotoInfo()
        {
            string[] rtn = new string[] { "", "", "" };

            if (Request.HttpMethod == "POST") {
                var uaParser = JsonConvert.DeserializeObject<UAParserModel>(Request["UAParser"]);
                var files = Request.Files;
                var fileLen = files.AllKeys.Count();

                if (fileLen == 0) {
                    rtn[0] = "無任何上傳檔案 !";
                    return this.ToJsonNet(rtn);
                }

                var now = DateTime.Now;
                var t = new PhotoInfoModel();
                var sw = new Stopwatch();
                var photoList = new List<int>();
                sw.Start();
                for (var i = 0; i < fileLen; i++) {
                    using (var scope = new TransactionScope()) {
                        using (var db = new SqlConnection(this.SettingsContext.GetValue("Test"))) {
                            var file = files[i];
                            t.FileExt = Path.GetExtension(file.FileName);
                            t.FileName = Path.GetFileNameWithoutExtension(file.FileName);

                            Stream fs = file.InputStream;
                            Image img = null;
                            var guid = Guid.NewGuid();
                            var maxPix = AppConfig.ThumbnailPixel;
                            var path = Server.MapPath($"~/Temp/{guid}.jpg");
                            var thbPath = Server.MapPath($"~/Temp/{guid}_{maxPix}.jpg");
                            var thb1920Path = Server.MapPath($"~/Temp/{guid}_1920.jpg");

                            try {

                                using (Stream stream = new FileStream(path, FileMode.Create)) {
                                    if (t.FileExt.ToLower() == ".heic") {
                                        // heic to jpg              
                                        using (MagickImage mag = new MagickImage(fs)) {
                                            mag.Write(stream, MagickFormat.Jpg);
                                        }
                                        img = Image.FromStream(stream, true, true);
                                    } else {
                                        img = Image.FromStream(fs, true, true);
                                    }

                                    PhotoUtility.PhotoExif(ref img, ref t);
                                    // Origin
                                    img.Save(stream, ImageFormat.Jpeg);
                                    // Thumbnail                           
                                    PhotoUtility.MakeThumbnail(stream, thbPath, maxPix.FixInt());
                                    // Thumbnail                           
                                    PhotoUtility.MakeThumbnail(stream, thb1920Path, 1920);
                                }

                                // 主檔
                                var p = new PhotoModel();
                                p.CreateDateTime = now;
                                p.FileDesc = "";
                                p.FileExt = t.FileExt;
                                p.FileName = t.FileName;
                                p.OrgCreateDateTime = t.OrgCreatDateTime;
                                p.OrgModifyDateTime = t.OrgModifyDateTime;
                                p.ModifyDateTime = t.OrgModifyDateTime;
                                p.Orientation = t.Orientation;
                                p.Location = t.Location;
                                p.Width = t.Width;
                                p.Height = t.Height;
                                p.Person = "";
                                p.CreateDateTime = now;
                                p.ModifyDateTime = now;
                                db.Insert(p);

                                // 原圖
                                var orgPhoto = new PhotoDetailModel();
                                orgPhoto.PhotoDetailSN = p.PhotoSN;
                                orgPhoto.CreateDateTime = now;
                                orgPhoto.Photo = Files.ReadBinaryFile(path);
                                orgPhoto.Type = "Origin";
                                db.Insert(orgPhoto);
                                Files.RenameFile(path, path.Replace(guid.ToString(), p.PhotoSN.ToString()));
                                path = path.Replace(guid.ToString(), p.PhotoSN.ToString());

                                // 縮圖
                                var thumbPhoto = new PhotoDetailModel();
                                thumbPhoto.PhotoDetailSN = p.PhotoSN;
                                thumbPhoto.CreateDateTime = now;
                                thumbPhoto.Photo = Files.ReadBinaryFile(thbPath);
                                thumbPhoto.Type = "Thumbnail";
                                db.Insert(thumbPhoto);
                                Files.RenameFile(thbPath, thbPath.Replace(guid.ToString(), p.PhotoSN.ToString()));
                                thbPath = thbPath.Replace(guid.ToString(), p.PhotoSN.ToString());

                                // 縮圖 1920
                                var thumbPhoto1920 = new PhotoDetailModel();
                                thumbPhoto1920.PhotoDetailSN = p.PhotoSN;
                                thumbPhoto1920.CreateDateTime = now;
                                thumbPhoto1920.Photo = Files.ReadBinaryFile(thb1920Path);
                                thumbPhoto1920.Type = "Thumbnail";
                                db.Insert(thumbPhoto1920);
                                Files.RenameFile(thb1920Path, thb1920Path.Replace(guid.ToString(), p.PhotoSN.ToString()));
                                thb1920Path = thb1920Path.Replace(guid.ToString(), p.PhotoSN.ToString());

                                //t.Url = "data:image/" + t.FileExt.Replace(".", "") + ";base64," + Convert.ToBase64String(Files.ReadBinaryFile(path));
                                t.Thumbnail = "data:image/" + t.FileExt.Replace(".", "") + ";base64," + Convert.ToBase64String(thumbPhoto1920.Photo);

                                var percent = ((double)(i + 1) / (double)fileLen * 100);
                                percent = percent > 99 ? 99 : percent;
                                ProgressHub.SendMessage(Request["Selector"], percent, "", Request["ConnId"], i, t.Thumbnail);

                                photoList.Add(p.PhotoSN);

                            } catch (Exception e) {
                                this.LogContext.LogRepoistory.SysLog(e);
                                rtn[0] += $"{t.FileName} - {e.Message} <br>";
                                Files.DelBulkFiles(new string[] { path, thbPath, thb1920Path });
                                ProgressHub.SendMessage(Request["Selector"], 0, e.Message, Request["ConnId"], i, "");
                                continue;
                            } finally {
                                if (img != null) img.Dispose();
                                img = null;
                            }
                        }
                        scope.Complete();
                    }
                }

                if (rtn[0] == "") {
                    // 上傳記錄
                    var log = new UploadLog();
                    log.Browser = $"{uaParser.BrowserName}  {uaParser.BrowserVersion}";
                    log.Device = $"{uaParser.DeviceType}  {uaParser.DeviceVendor}";
                    log.OS = $"{uaParser.OsName}  {uaParser.OsVersion}";
                    log.LocalIP = "";
                    log.TotalNum = fileLen;
                    log.Type = "Photo";
                    log.CompletedTime = sw.Elapsed.ToString("mm\\:ss\\.ff");
                    log.CreateDateTime = now;
                    log.PhotoSNList = photoList.ToArray().Join(",");

                    using (var db = new SqlConnection(this.SettingsContext.GetValue("Test"))) {
                        db.Insert(log);
                    }
                }

                ProgressHub.SendMessage(Request["Selector"], 100, "", Request["ConnId"], fileLen, "");
            }

            return this.ToJsonNet(rtn);
        }


    }
}