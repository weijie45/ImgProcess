using DapperExtensions.Mapper;
using System;

namespace Service.Model
{
    public class UploadLog
    {
        private static DateTime _Now = DateTime.Now;

        public int UploadLogSN { get; set; }

        /// <summary>
        /// Photo/Video/Audio
        /// </summary>
        public string Type { get; set; }

        public string LocalIP { get; set; }

        public string Device { get; set; }

        public string Browser { get; set; }

        public string OS { get; set; }

        public string PhotoSNList { get; set; }

        public int TotalNum { get; set; }

        public string CompletedTime { get; set; }

        public DateTime CreateDateTime { get; set; } = _Now;

    }

    public class UploadLoglMapper : ClassMapper<UploadLog>
    {
        public UploadLoglMapper()
        {
            Table("UploadLog");

            Map(p => p.UploadLogSN).Key(KeyType.Identity);

            AutoMap();
        }
    }
}
