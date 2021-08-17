using System;
using System.Collections.Generic;
using System.Text;

namespace PoalimOnlineBusiness
{
    public class TransactionRowWithdraw
    {
        public TransactionRowWithdraw()
        {

        }

        public TransactionRowWithdraw(int instituteNumber, int bankcode, int branchNumber, string beneficiaryNname, decimal amount)
        {
            this.InstituteNumber = instituteNumber;
            this.Bankcode = bankcode;
            this.BranchNumber = branchNumber;
            //this.AccountNumber = accountNumber;
            this.BeneficiaryNname = beneficiaryNname;
            this.Amount = (int)(amount * 100);
        }



        //1	1	1	 X Record Type	2
        [FixedLengthFile(1, 1, 1, DataType.X)]
        public string RecordType { get; set; } = "1";

        //2	2	8	 N Institute   Must be 8 digits.Provided by MASAV.
        [FixedLengthFile(2, 2, 8, DataType.N)]
        public int InstituteNumber { get; set; }

        //3	10	2	 N Currency Code 	5
        [FixedLengthFile(3, 10, 2, DataType.N)]
        public int Currency { get; set; } = 0;

        //4	12	6	 N Filler	 '000000'
        [FixedLengthFile(4, 12, 6, DataType.N)]
        public int Filler1 { get; set; } = 0; 

        //5	18	2	 N Bankcode    Bank of Israel Code
        [FixedLengthFile(5, 18, 2, DataType.N)]
        public int Bankcode { get; set; }

        //6	20	3	 N BranchNumber    Bank of Israel recognized number
        [FixedLengthFile(6, 20, 3, DataType.N)]
        public int BranchNumber { get; set; }



        //7	23	4	 N Account Type Zeros
        [FixedLengthFile(7, 23, 4, DataType.N)]
        public int AccountType { get; set; } = 0;





        //10	37	9	 N National ID Not Required
        [FixedLengthFile(8, 27, 9, DataType.N)]
        public int NationalID { get; set; } = 0;

        //11	46	16	 X Beneficiary name Right aligned
        [FixedLengthFile(9, 36, 16, DataType.X, Align = Align.Right)]
        public string BeneficiaryNname { get; set; }

        //12	62	13	 N Amount	
        [FixedLengthFile(10, 52, 13, DataType.N)]
        public int Amount { get; set; }


        //17	109	18	 N Filler ZEROS 
        [FixedLengthFile(11, 65, 20, DataType.N)]
        public int Filler3 { get; set; } = 0;



        //14	95	8	 N Period Payed For       "dd/MM/yy"
        [FixedLengthFile(12, 85, 8, DataType.X)]
        public int PeriodPayedFor { get; set; } = 0;


        //15	103	3	 N Text Code ZEROS
        [FixedLengthFile(13, 93, 3, DataType.N)]
        public int TextCode { get; set; } = 0;

        //16	106	3	 N Record Type  	556
        [FixedLengthFile(14, 96, 3, DataType.N)]
        public int RecordType2 { get; set; } = 504;



        //18	127	2	 X Filler  BLANK
        [FixedLengthFile(15, 99, 18, DataType.N)]
        public int Filler4 { get; set; } = 0;
    }
}
