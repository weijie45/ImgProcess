using FilesIO;
using ImageMagick;
using ImgProcess.Hubs;
using Log;
using Newtonsoft.Json;
using Service.Function.Base;
using Service.Function.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace ImgProcess.Controllers
{
    public class HomeController : Controller
    {
        // Fields
        private LogContext _logContext = null;

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
            public string Width { get; set; }
            public string Heigth { get; set; }
            public string OrgCreatDateTime { get; set; }
            public string OrgModifyDateTime { get; set; }
            public string Location { get; set; }
            public string ErrMsg { get; set; }
            public string Orientation { get; set; }
        }

        /// <summary>
        /// 圖檔轉為JPG格式
        /// </summary>
        /// <returns></returns>
        public ActionResult FormatPhoto()
        {
            var rtn = new string[2] { "", "" };
            var files = Request.Files;
            if (files.AllKeys.Count() == 0) {
                rtn[0] = "查無檔案 !";
                return this.ToJsonNet(rtn);
            }

            var file = files[0];
            Stream fs = file.InputStream;

            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            var path = Server.MapPath($"~/Temp/{fileName}.jpg");
            try {
                using (MagickImage image = new MagickImage(fs)) {
                    image.Write(path);
                }
                rtn[1] = "data:image/jpeg;base64," + Convert.ToBase64String(Files.ReadBinaryFile(path, true));
            } catch (Exception e) {
                rtn[0] = "請確認上傳檔案是否為圖片格式 !";
                this.LogContext.LogRepoistory.SysLog(e);
            }

            return this.ToJsonNet(rtn);
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
                var files = Request.Files;
                var t = new ThumbnailInfo();
                for (var i = 0; i < files.AllKeys.Count(); i++) {

                    var file = files[i];
                    t.FileExt = Path.GetExtension(file.FileName);
                    t.FileName = Path.GetFileNameWithoutExtension(file.FileName);

                    Stream fs = file.InputStream;
                    Image img = null;

                    try {
                        var path = $"~/Temp/{t.FileName}_org.jpg";
                        var thbPath = $"~/Temp/{t.FileName}_300.jpg";
                        using (Stream stream = new FileStream(Server.MapPath(path), FileMode.Create)) {
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
                            MakeThumbnail(stream, thbPath, 300, 100);

                            t.Url = "data:image/" + t.FileExt.Replace(".", "") + ";base64," + Convert.ToBase64String(Files.ReadBinaryFile(path));
                            t.Thumbnail = "data:image/" + t.FileExt.Replace(".", "") + ";base64," + Convert.ToBase64String(Files.ReadBinaryFile(thbPath));
                        }

                        Files.DelFile(path);
                        Files.DelFile(thbPath);

                        rtn[1] = JsonConvert.SerializeObject(t);

                    } catch (Exception e) {
                        this.LogContext.LogRepoistory.SysLog(e);
                        rtn[0] += $"{t.FileName} - {e.Message} <br>";
                        ProgressHub.SendMessage(Request["Selector"], 0, e.Message, Request["ConnId"]);
                    } finally {
                        if (img != null) img.Dispose();
                        img = null;
                    }

                }

            }

            return Json(rtn);
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
            t.ErrMsg = "";
            t.OrgModifyDateTime = "";
            t.OrgCreatDateTime = "";
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
                        t.OrgModifyDateTime = picDate;
                    }
                }

                p = items.Where(w => w.Id == 36867).FirstOrDefault();
                if (p != null) {
                    // 拍摄建立日期
                    picDate = ascii.GetString(p.Value);
                    if ((!"".Equals(picDate)) && picDate.Length >= 10) {
                        picDate = Regex.Replace(picDate.Substring(0, 10), "[:,-]", "/");
                        t.OrgCreatDateTime = picDate;
                        t.OrgModifyDateTime = picDate;
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


            t.Width = image.Width.FixNull();
            t.Heigth = image.Height.FixNull();
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
                //取得影像的格式

                //第一種縮圖用
                maxPx = maxPx == 0 ? 300 : maxPx;
                //宣告一個最大值，demo是把該值寫到web.config裡
                if (image.Width > maxPx || image.Height > maxPx)
                //如果圖片的寬大於最大值或高大於最大值就往下執行
                {
                    if (image.Width >= image.Height)
                    //圖片的寬大於圖片的高
                    {
                        fixHeight = maxPx;
                        //設定修改後的圖高
                        fixWidth = Convert.ToInt32((Convert.ToDouble(fixHeight) / Convert.ToDouble(image.Height)) * Convert.ToDouble(image.Width));
                        //設定修改後的圖寬
                    } else {

                        fixWidth = maxPx;
                        //設定修改後的圖寬
                        fixHeight = Convert.ToInt32((Convert.ToDouble(fixWidth) / Convert.ToDouble(image.Width)) * Convert.ToDouble(image.Height));
                        //設定修改後的圖高
                    }
                } else
                  //圖片沒有超過設定值，不執行縮圖
                  {
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


    }
}