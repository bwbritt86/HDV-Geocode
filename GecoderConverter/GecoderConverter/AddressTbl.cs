using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GecoderConverter
{
    class AddressTbl
    {
        String offenseType;
        String number;
        String street;
        String type;
        String suffix;


        public String OffenseType
        {
            get { return offenseType; }
            set { offenseType = value; }
        }

        public String Number
        {
            get { return number; }
            set { number = value; }
        }

        public String Street
        {
            get { return street; }
            set { street = value; }
        }

        public String Type
        {
            get { return type; }
            set { type = value; }
        }

        public String Suffix
        {
            get { return suffix; }
            set { suffix = value; }
        }

    }
}
