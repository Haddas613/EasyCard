using Nayax.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Nayax
{
    public class Phase1RequestBody
    {
        public string clientID { get; set; }
        public string terminalID { get; set; }
        public string posID { get; set; }
        #region example_request
        //    {   "clientID":"600344",
        //   "terminalID":"1234567890",
        //   "posID":"87512349", clientid_terminalid from terminalsettings
        //   "params":[
        //        "ashrait",
        //        {
        //             "amount":2000,
        //             "vuid":"1234567890",
        //             "currency":"376",
        //             "creditTerms":1,
        //             "tranCode":1,
        //             "tranType":1,
        //             "cardNumber":"4580000000000000",
        //             "expDate":"2308",
        //             "cvv":"666"
        //        }
        //   ]
        //}
        #endregion
        [JsonProperty("params")]
        public Object[] paramss { get; set; }

        public Phase1RequestBody()
        {

        }
        public Phase1RequestBody(string clientID, string terminalID, string posID)
        {
            this.clientID = clientID;
            this.terminalID = terminalID;
            this.posID = posID;
            this.paramss = new object[2];
            paramss[0] = "ashrait";
            paramss[1] = new ObjectInPhase1RequestParams();
        }
        public dynamic Phase1FixMeUp(Phase1RequestBody phase1Request)
        {
            var t = phase1Request.GetType();
            var returnClass = new ExpandoObject() as IDictionary<string, object>;
            foreach (var pr in t.GetProperties())
            {
                var val = pr.GetValue(phase1Request);
                if (val is string && string.IsNullOrWhiteSpace(val.ToString()))
                {
                }
                else if (val == null)
                {
                }
                else if (!pr.Name.Equals("paramss"))
                {
                    returnClass.Add(pr.Name, val);
                }
                else if (pr.Name.Equals("paramss"))
                {
                    Object[] arr = (object[])val;
                    var s = arr[1].GetType();
                    var returnClassinner = new ExpandoObject() as IDictionary<string, object>;
                    returnClassinner.Add("ashrait", arr[0]);
                    foreach (var valpr in s.GetProperties())
                    {
                        var valvalpr = valpr.GetValue(phase1Request.paramss[1]);
                        if (valvalpr is string && string.IsNullOrWhiteSpace(valvalpr.ToString()))
                        {
                        }
                        else if (valvalpr == null)
                        {
                        }
                        else if (valvalpr is int && (int)valvalpr == 0)
                        {
                        }
                        else
                        {
                            returnClassinner.Add(valpr.Name, valvalpr);
                        }
                    }
                    returnClass.Add(pr.Name, returnClassinner);
                }
            }
            return returnClass;
        }
    }
}
