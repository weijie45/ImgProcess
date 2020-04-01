using DapperExtensions.Mapper;
using System;

namespace Service.Model
{
    public class Photo
    {
        public int PhotoSN { get; set; }
        
        public string FileName { get; set; }

        public string FileExt { get; set; }

        public string FileDesc { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string Location { get; set; }

        public string Person { get; set; }

        public int HitCnt { get; set; } = 0;

        public string Orientation { get; set; } = "0";

        public DateTime OrgCreateDateTime { get; set; }

        public DateTime OrgModifyDateTime { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime ModifyDateTime { get; set; }

    }

    public class PhotoMapper : ClassMapper<Photo>
    {
        public PhotoMapper()
        {
            Table("Photo");

            Map(p => p.PhotoSN).Key(KeyType.Identity);

            AutoMap();
        }
    }

}
