using DapperExtensions;
using FilesIO;
using ImageMagick;
using ImgProcess.Hubs;
using ImgProcess.Infrastructure;
using Log;
using Newtonsoft.Json;
using Service.Function.Base;
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
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web.Mvc;

namespace ImgProcess.Controllers
{
    public class HomeController : BaseController
    {
        // Fields
        private LogContext _logContext = null;
        private SettingsContext _settingsContext = null;

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

        public ActionResult Index()
        {
            return View();
        }

        private class ThumbnailInfo
        {
            public string Url { get; set; }
            public string Thumbnail { get; set; }
            public string FileName { get; set; }
            public string FileExt { get; set; }
            public int Width { get; set; }
            public int Heigth { get; set; }
            public DateTime OrgCreatDateTime { get; set; }
            public DateTime OrgModifyDateTime { get; set; }
            public string Location { get; set; }
            public string Orientation { get; set; }
        }

        private class UAParser
        {
            public string BrowserName { get; set; }
            public string BrowserVersion { get; set; }
            public string EngineName { get; set; }
            public string EngineVersion { get; set; }
            public string OsName { get; set; }
            public string OsVersion { get; set; }
            public string DeviceType { get; set; }
            public string DeviceVendor { get; set; }
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
                var uaParser = JsonConvert.DeserializeObject<UAParser>(Request["UAParser"]);
                var files = Request.Files;
                var fileLen = files.AllKeys.Count();

                if (fileLen == 0) {
                    rtn[0] = "無任何上傳檔案 !";
                    return this.ToJsonNet(rtn);
                }

                var now = DateTime.Now;
                var t = new ThumbnailInfo();
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
                            var maxPix = this.SettingsContext.GetValue("MaxPixel");
                            var path = Server.MapPath($"~/Temp/{guid}_org.jpg");
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
                                    
                                    FormatPhoto(ref img, ref t);
                                    // Origin
                                    img.Save(stream, ImageFormat.Jpeg);
                                    // Thumbnail                           
                                    MakeThumbnail(stream, thbPath, maxPix.FixInt());
                                    // Thumbnail                           
                                    MakeThumbnail(stream, thb1920Path, 1920);
                                }

                                //t.Url = "data:image/" + t.FileExt.Replace(".", "") + ";base64," + Convert.ToBase64String(Files.ReadBinaryFile(path));
                                t.Thumbnail = "data:image/" + t.FileExt.Replace(".", "") + ";base64," + Convert.ToBase64String(Files.ReadBinaryFile(thb1920Path));

                                // 主檔
                                var p = new Photo();
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
                                p.Height = t.Heigth;
                                p.Person = "";
                                p.CreateDateTime = now;
                                p.ModifyDateTime = now;
                                db.Insert(p);

                                // 原圖
                                var pd = new PhotoDetail();
                                pd.PhotoDetailSN = p.PhotoSN;
                                pd.CreateDateTime = now;
                                pd.Photo = Files.ReadBinaryFile(path);
                                pd.Type = "Origin";
                                db.Insert(pd);
                                Files.RenameFile(path, path.Replace(guid.ToString(), p.PhotoSN.ToString()));
                                path = path.Replace(guid.ToString(), p.PhotoSN.ToString());

                                // 縮圖
                                pd = new PhotoDetail();
                                pd.PhotoDetailSN = p.PhotoSN;
                                pd.CreateDateTime = now;
                                pd.Photo = Files.ReadBinaryFile(thbPath);
                                pd.Type = "Thumbnail";
                                db.Insert(pd);
                                Files.RenameFile(thbPath, thbPath.Replace(guid.ToString(), p.PhotoSN.ToString()));
                                thbPath = thbPath.Replace(guid.ToString(), p.PhotoSN.ToString());

                                pd = new PhotoDetail();
                                pd.PhotoDetailSN = p.PhotoSN;
                                pd.CreateDateTime = now;
                                pd.Photo = Files.ReadBinaryFile(thbPath);
                                pd.Type = "Thumbnail";
                                db.Insert(pd);
                                Files.RenameFile(thb1920Path, thb1920Path.Replace(guid.ToString(), p.PhotoSN.ToString()));
                                thb1920Path = thb1920Path.Replace(guid.ToString(), p.PhotoSN.ToString());


                                var percent = ((double)(i + 1) / (double)fileLen * 100);
                                percent = percent > 99 ? 99 : percent;
                                ProgressHub.SendMessage(Request["Selector"], percent, "", Request["ConnId"], i, t.Thumbnail);

                                photoList.Add(p.PhotoSN);

                            } catch (Exception e) {
                                this.LogContext.LogRepoistory.SysLog(e);
                                rtn[0] += $"{t.FileName} - {e.Message} <br>";
                                Files.DelBulkFiles(new string[] { path, thbPath, thb1920Path });
                                ProgressHub.SendMessage(Request["Selector"], 0, e.Message, Request["ConnId"], i, "");
                            } finally {
                                if (img != null) img.Dispose();
                                img = null;
                            }
                        }
                        scope.Complete();
                    }
                }

                if (rtn[0] == "") {
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

        /// <summary>
        /// 格式化GPS經緯度
        /// </summary>
        /// <param name="propItemRef"></param>
        /// <param name="propItem"></param>
        /// <returns></returns>
        private static double ExifGpsToDouble(PropertyItem propItemRef, PropertyItem propItem)
        {
            if (propItem == null || propItemRef == null) return 0;
            double degreesNumerator = BitConverter.ToUInt32(propItem.Value, 0);
            double degreesDenominator = BitConverter.ToUInt32(propItem.Value, 4);
            double degrees = degreesNumerator / (double)degreesDenominator;

            double minutesNumerator = BitConverter.ToUInt32(propItem.Value, 8);
            double minutesDenominator = BitConverter.ToUInt32(propItem.Value, 12);
            double minutes = minutesNumerator / (double)minutesDenominator;

            double secondsNumerator = BitConverter.ToUInt32(propItem.Value, 16);
            double secondsDenominator = BitConverter.ToUInt32(propItem.Value, 20);
            double seconds = secondsNumerator / (double)secondsDenominator;


            double coorditate = degrees + (minutes / 60d) + (seconds / 3600d);
            string gpsRef = System.Text.Encoding.ASCII.GetString(new byte[1] { propItemRef.Value[0] }); //N, S, E, or W
            if (gpsRef == "S" || gpsRef == "W")
                coorditate = coorditate * -1;
            return coorditate;
        }

        /// <summary>
        /// 取得Exif, 檢查ios圖片長寬問題
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="fileName"></param>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        private void FormatPhoto(ref Image image, ref ThumbnailInfo t)
        {
            Encoding ascii = Encoding.ASCII;
            string picDate;
            //Image image = Image.FromStream(fs, true, false);
            short orientation = 0;
            var now = DateTime.Now;
            t.OrgModifyDateTime = now;
            t.OrgCreatDateTime = now;
            t.Location = "";

            var items = image.PropertyItems.Where(w => new int[] { 1, 2, 3, 4, 36867, 306, 274, 5029 }.Contains(w.Id)).Select(s => s);
            if (items.Count() > 0) {
                var latitude = ExifGpsToDouble(items.Where(w => w.Id == 1).FirstOrDefault(), items.Where(w => w.Id == 2).FirstOrDefault());
                var longitude = ExifGpsToDouble(items.Where(w => w.Id == 3).FirstOrDefault(), items.Where(w => w.Id == 4).FirstOrDefault());
                t.Location = $"{latitude},{longitude}";

                var p = items.Where(w => w.Id == 306).FirstOrDefault();

                // 拍摄更新日期
                if (p != null) {
                    picDate = ascii.GetString(p.Value);
                    if ((!"".Equals(picDate)) && picDate.Length >= 10) {
                        picDate = Regex.Replace(picDate.Substring(0, 10), "[:,-]", "/");
                        t.OrgModifyDateTime = Convert.ToDateTime(picDate);
                    }
                }

                p = items.Where(w => w.Id == 36867).FirstOrDefault();
                if (p != null) {
                    // 拍摄建立日期
                    picDate = ascii.GetString(p.Value);
                    if ((!"".Equals(picDate)) && picDate.Length >= 10) {
                        picDate = Regex.Replace(picDate.Substring(0, 10), "[:,-]", "/");
                        t.OrgCreatDateTime = Convert.ToDateTime(picDate);
                        t.OrgModifyDateTime = Convert.ToDateTime(picDate);
                    }
                }

                p = items.Where(w => w.Id == 274).FirstOrDefault();
                if (p != null) {
                    orientation = BitConverter.ToInt16(p.Value, 0);
                    switch (orientation) {
                        case 1:
                            image.RotateFlip(RotateFlipType.RotateNoneFlipNone);
                            break;
                        case 2:
                            image.RotateFlip(RotateFlipType.RotateNoneFlipX);//horizontal flip
                            break;
                        case 3:
                            image.RotateFlip(RotateFlipType.Rotate180FlipNone);//right-top
                            break;
                        case 4:
                            image.RotateFlip(RotateFlipType.RotateNoneFlipY);//vertical flip
                            break;
                        case 5:
                            image.RotateFlip(RotateFlipType.Rotate90FlipX);
                            break;
                        case 6:
                            image.RotateFlip(RotateFlipType.Rotate90FlipNone);//right-top
                            break;
                        case 7:
                            image.RotateFlip(RotateFlipType.Rotate270FlipX);
                            break;
                        case 8:
                            image.RotateFlip(RotateFlipType.Rotate270FlipNone);//left-bottom
                            break;
                    }
                    p.Value[0] = 1;
                    image.SetPropertyItem(p);
                }
            }


            t.Width = image.Width;
            t.Heigth = image.Height;
            t.Orientation = orientation.FixNull();

        }

        /// <summary>
        /// 製作縮圖
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="path"></param>
        /// <param name="maxPx"></param>
        /// <param name="quality"></param>
        public void MakeThumbnail(Stream stream, string path, int maxPx = 0, int quality = 75)
        {
            try {
                var image = Image.FromStream(stream, true, true);
                ImageFormat thisFormat = image.RawFormat;
                int fixWidth = 0;
                int fixHeight = 0;

                maxPx = maxPx == 0 ? 300 : maxPx;
                if (image.Width > maxPx || image.Height > maxPx)
                {
                    if (image.Width <= image.Height)
                    {
                        fixHeight = maxPx;
                        fixWidth = Convert.ToInt32((Convert.ToDouble(fixHeight) / Convert.ToDouble(image.Height)) * Convert.ToDouble(image.Width));

                    } else {

                        fixWidth = maxPx;
                        fixHeight = Convert.ToInt32((Convert.ToDouble(fixWidth) / Convert.ToDouble(image.Width)) * Convert.ToDouble(image.Height));
                    }
                } else {
                    fixHeight = image.Height;
                    fixWidth = image.Width;
                }

                stream.Seek(0, SeekOrigin.Begin);

                using (var mag = new MagickImage(stream)) {
                    mag.Resize(fixWidth, fixHeight);
                    mag.Format = MagickFormat.Jpg;
                    mag.Quality = quality;
                    mag.Write(path);
                }

            } catch (Exception e) {
                throw e;
            }

        }

        public ActionResult FindLog()
        {
            ViewBag.LogList = this.LogContext.LogRepoistory.FindAllByLogDate(DateTime.Now.ToString("yyyyMMdd"));

            return View("LogView");
        }

    }
}