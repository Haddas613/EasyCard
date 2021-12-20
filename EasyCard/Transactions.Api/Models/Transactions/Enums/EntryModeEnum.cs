using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions.Enums
{
    public enum EntryModeEnum
    {
        Magnetic = 00,
        ContaclessMagnetic = 04,
        ContaclessEmv = 05,
        MagneticMobileCTL = 06,
        EMVMobileCTL = 07,
        CellularPhoneNum = 10,
        RFU11 = 11,
        RFU12 = 12,
        RFU13 = 13,
        EMVContact = 40,
        PhoneTran = 50,
        SignatureOnlyTran = 51,
        InternetTran = 52,
        FallBackMagnetic = 80,
        EmptyCandidateListMagnetic = 81
    }
}
