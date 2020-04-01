using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Model
{
    public class PhotoInfoModel
    {
        public string Url { get; set; }
        public string Thumbnail { get; set; }
        public string FileName { get; set; }
        public string FileExt { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public DateTime OrgCreatDateTime { get; set; }
        public DateTime OrgModifyDateTime { get; set; }
        public string Location { get; set; }
        public int Orientation { get; set; }
    }
}
