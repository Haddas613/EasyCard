using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Shared.Integration;
using Shared.Integration.Models;
using System.Text.RegularExpressions;

namespace Shared.Tests.CreditCard
{
    public class Class1
    {
        [Fact]
        public void CardVendorResolverTest()
        {
            var s = "MjEwNTI0MTA0MTUwMDg4NzAzMzI3NjZ/J4I1lWew+/q2t08bmyI0Ph8p//XMlKcIO2ezLqdvbol1M70rwpY2OpSKrInepf2rZo9Vd5K5zk0Y/amZUzmPELI+uzrMp30MeZihR7LRh6V4VxZ2hmwGO+hjpasMxZmKBHlMK6GheNcaUgN3vs3sgjk8NROFTRJ+KWX+2XekHHOeRU8e4A+FBSaPVD+/gDcm69KemFiFmt+QJsCrOUqZIg==";

            var b = Convert.FromBase64String(s);

            Console.WriteLine(b);
        }

        [Fact]
        public void TestReplace()
        {
            var requestStr = "<Envelope xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns='http://www.w3.org/2003/05/soap-envelope'><Body><AshStart xmlns='http://shva.co.il/xmlwebservices/'><MerchantNumber>0887033016</MerchantNumber><UserName>FLJHW</UserName><Password>SS085930</Password><inputObj><bAnalysisXmlStr>false</bAnalysisXmlStr><expirationDate>2203</expirationDate><tranType>01</tranType><amount>200</amount><panEntryMode>50</panEntryMode><cvv2>424</cvv2><id>122222227</id><currency>376</currency><creditTerms>1</creditTerms><status>OK</status><parameterJ>4</parameterJ><clientInputPan>375510190000062</clientInputPan></inputObj><pinpad><tag9A>0</tag9A><tag9F21>0</tag9F21><tag9F02>0</tag9F02><tag5F2A>0</tag5F2A><tag9F03>0</tag9F03><authTxnAuthType>Purchase</authTxnAuthType><statusCode>PromptForCardEntry</statusCode><endTransaction>0</endTransaction><txnStatus>OK</txnStatus><result>0</result><fallback>None</fallback><mobile>false</mobile><authForceManualEntry>false</authForceManualEntry><cardEntryMode>NotEntered</cardEntryMode><bForceOnline>false</bForceOnline><bBlackList>false</bBlackList><bFloorLimit>false</bFloorLimit><bRandomSelection>false</bRandomSelection><originalAuthTxnAuthType>Purchase</originalAuthTxnAuthType><hostResult>None</hostResult><txnOutcome>NotCompleted</txnOutcome><bStopExamine>false</bStopExamine><status>OK</status><serverOffDemand>DEFAULT</serverOffDemand><resourceTransferCard>DEFAULT_VALUE</resourceTransferCard><bAshReason22>false</bAshReason22><bIgnorePP>false</bIgnorePP><type>None</type><connectionType>SerialPort</connectionType><ipPort>0</ipPort><tag9C>0</tag9C><ashStatus>OK</ashStatus><ashReason>DEFAULT</ashReason></pinpad><globalObj /></AshStart></Body></Envelope>";
            requestStr = Regex.Replace(requestStr, "\\<clientInputPan\\>\\d{9,16}\\</clientInputPan\\>", "<clientInputPan>****************</clientInputPan>");
            requestStr = Regex.Replace(requestStr, "\\<cvv2\\>\\d{3,4}\\</cvv2\\>", "<cvv2>***</cvv2>");

            Console.WriteLine(requestStr);
        }
    }
}
