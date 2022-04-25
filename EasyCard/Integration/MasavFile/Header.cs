using System;
using System.Collections.Generic;
using System.Text;

namespace PoalimOnlineBusiness
{
    public class Header
    {
        public Header()
        {

        }

        public Header(int instituteNumber, DateTime paymentDate, DateTime creationDate, int sendingInstitute, string institueName)
        {
            this.InstituteNumber = instituteNumber;
            this.PaymentDate = paymentDate;
            this.CreationDate = creationDate;
            this.SendingInstitute = sendingInstitute;
            this.InstitueName = institueName;
        }

        //1	1	1	 X	Record Type	 'K'
        [FixedLengthFile(1, 1, 1, DataType.X)]
        public string RecordType { get; set; } = "K";

        //2	2	3	 N Institute Number Must be 8 digits.Provided by MASAV.
        [FixedLengthFile(2, 2, 3, DataType.N)]
        public int InstituteNumber { get; set; }

        //3	10	2	 N Currency 	 '55'
        [FixedLengthFile(3, 5, 2, DataType.N)]
        public int Currency { get; set; } = 0;

        //4	12	6	 N Payment Date YYMMDD – REQUIRED FIELD 
        [FixedLengthFile(4, 7, 6, DataType.N, "{0:yyMMdd}")]
        public DateTime PaymentDate { get; set; }

        //5	18	1	 N Filler	 '5'
        [FixedLengthFile(5, 13, 1, DataType.N)]
        public int Filler1 { get; set; } = 0;

        //6	19	3	 N Serial Number	 '552'
        [FixedLengthFile(6, 14, 3, DataType.N)]
        public int SerialNumber { get; set; } = 001;

        //7	22	1	 N Filler	 '5'
        [FixedLengthFile(7, 17, 1, DataType.N)]
        public int Filler2 { get; set; } = 0;

        //8	23	6	 N Creation Date YYMMDD
        [FixedLengthFile(8, 18, 6, DataType.N, "{0:yyMMdd}")]
        public DateTime CreationDate { get; set; }

        //9	29	5	 N Sending Institute Must be 5 numbers, provided by Masav.
        [FixedLengthFile(9, 24, 5, DataType.N)]
        public int SendingInstitute { get; set; }

        //10	34	6	 N Filler ZEROS 
        [FixedLengthFile(10, 29, 6, DataType.N)]
        public int Filler3 { get; set; } = 0;

        //11	40	30	 X Institue Name Aligned to right 
        [FixedLengthFile(11, 35, 30, DataType.X, Align = Align.Right)]// without hebrew to do  space instead
        public string InstitueName { get; set; }

        //12	70	56	 X Filler  BLANK
        [FixedLengthFile(12, 65, 56, DataType.X)]
        public string Filler4 { get; set; } = string.Empty;

        //13	126	3	 X Header ID 	 'KOT'
        [FixedLengthFile(13, 121, 3, DataType.X)]
        public string HeaderID { get; set; } = "KOT";

    }
}
