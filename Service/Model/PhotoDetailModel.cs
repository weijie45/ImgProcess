using DapperExtensions.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Model
{
    public class PhotoDetailModel
    {
        public int PhotoDetailSN { get; set; }

        /// <summary>
        /// Origin/Small
        /// </summary>
        public string Type { get; set; }

        public byte[] Photo { get; set; }

        public DateTime CreateDateTime { get; set; }

    }

    public class PhotoDetailMapper : ClassMapper<PhotoDetailModel>
    {
        public PhotoDetailMapper()
        {
            Table("PhotoDetail");

            Map(p => p.PhotoDetailSN).Key(KeyType.Assigned);

            AutoMap();
        }
    }
}
