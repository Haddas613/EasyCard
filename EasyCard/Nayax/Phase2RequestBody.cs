using Nayax.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Nayax
{
    public class Phase2RequestBody
    {
        
            public string clientID { get; set; }
            public string terminalID { get; set; }
            public string posID { get; set; }

            public Object[] paramss { get; set; }
            public dynamic Phase2FixMeUp(Phase2RequestBody phase2Request)
            {
                var t = phase2Request.GetType();
                var returnClass = new ExpandoObject() as IDictionary<string, object>;
                foreach (var pr in t.GetProperties())
                {
                    var val = pr.GetValue(phase2Request);
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
                            var valvalpr = valpr.GetValue(phase2Request.paramss[1]);
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
            public Phase2RequestBody(string clientID, string terminalID, string posID)
            {
                this.clientID = clientID;
                this.terminalID = terminalID;
                this.posID = posID;
                this.paramss = new object[2];
                paramss[0] = "ashrait";
                paramss[1] = new ObjectInPhase2RequestParams();
            }
        
    }
}
