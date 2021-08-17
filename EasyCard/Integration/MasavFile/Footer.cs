using System;
using System.Collections.Generic;
using System.Text;

namespace PoalimOnlineBusiness
{
    public class Footer
    {
        public Footer()
        {

        }

        public Footer(int instituteNumber, DateTime paymentDate, int totalAmount, int transactionsCount)
        {
            this.InstituteNumber = instituteNumber;
            this.PaymentDate = paymentDate;
            this.TransactionsCount = transactionsCount;
            this.TotalAmount = totalAmount;
        }

        //1	1	1	 X Record Type	 'S'
        [FixedLengthFile(1, 1, 1, DataType.X)]
        public string RecordType { get; set; } = "S";

        //2	2	8	 N Institute   Same as header
        [FixedLengthFile(2, 2, 8, DataType.N)]
        public int InstituteNumber { get; set; }

        //3	10	2	 N Currency Code Same as header 
        [FixedLengthFile(3, 10, 2, DataType.N)]
        public int Currency { get; set; } = 0;

        //4	12	6	 N Payment Date YYMMDD
        [FixedLengthFile(4, 12, 6, DataType.N, "{0:yyMMdd}")]
        public DateTime PaymentDate { get; set; }

        //5	18	1	 N Filler Same as header
        [FixedLengthFile(5, 18, 1, DataType.N)]
        public int Filler1 { get; set; } = 0;

        //6	19	3	 N Serial Number Same as header
        [FixedLengthFile(6, 19, 3, DataType.N)]
        public int SerialNumber { get; set; } = 001;

        //7	22	15	 N Total Amount	13 digits followed by two additional digits(for after decimal point)
        [FixedLengthFile(7, 22, 15, DataType.N)]
        public int TotalAmount { get; set; }

        //8	37	15	 N Filler ZEROS 
        [FixedLengthFile(8, 37, 15, DataType.N)]
        public int Filler2 { get; set; } = 0; //5 ?

        //9	52	7	 N Transactions Count	
        [FixedLengthFile(9, 52, 7, DataType.N)]
        public int TransactionsCount { get; set; }

        //10	59	7	 N Filler ZEROS
        [FixedLengthFile(10, 59, 7, DataType.N)]
        public int Filler3 { get; set; } = 0;

        //11	66	63	 X Filler  BLANK
        [FixedLengthFile(11, 66, 63, DataType.X)]
        public string Filler4 { get; set; } = string.Empty;

    }
}
