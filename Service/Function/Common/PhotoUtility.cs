using ImageMagick;
using Service.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Service.Function.Common
{
    public class PhotoUtility
    {

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
        public static void PhotoExif(ref Image image, ref PhotoInfoModel t)
        {
            Encoding ascii = Encoding.ASCII;
            string picDate;          
            short orientation = 0;
            var now = DateTime.Now;
            t.OrgModifyDateTime = now;
            t.OrgCreatDateTime = now;
            t.Location = "";

            try {
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
                t.Height = image.Height;
                t.Orientation = orientation;

            } catch (Exception e) {
                throw e;
            }
        }

        /// <summary>
        /// 製作縮圖
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="path"></param>
        /// <param name="maxPx"></param>
        /// <param name="quality"></param>
        public static void MakeThumbnail(Stream stream, string path, int maxPx = 0, int quality = 75)
        {
            try {
                var image = Image.FromStream(stream, true, true);
                ImageFormat thisFormat = image.RawFormat;
                int fixWidth = 0;
                int fixHeight = 0;

                maxPx = maxPx == 0 ? 300 : maxPx;
                if (image.Width > maxPx || image.Height > maxPx) {
                    if (image.Width <= image.Height) {
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

    }
}
