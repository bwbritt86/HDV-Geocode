using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GecoderConverter
{
    class LatLongTbl
    {
        String offenseType;
        String latitude;
        String longitude;

        public String OffenseType
        {
            get { return offenseType; }
            set { offenseType = value; }
        }

        public String Latitude
        {
            get { return latitude; }
            set { latitude = value; }
        }

        public String Longitude
        {
            get { return longitude; }
            set { longitude = value; }
        }
    }
}
