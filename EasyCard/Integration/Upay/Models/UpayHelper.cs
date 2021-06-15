using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Upay;

namespace Upay.Models
{
    public static class UpayHelper
    {
    
        public static MsgModel GetLoginMsgModel(string email, string passMd5, bool isTestTerminal, string keyAuthnticate)
        {
            try
            {
                MsgModel msgModel = new MsgModel();
                HeaderModel header = new HeaderModel();
                header.language = "HE";
                header.livesystem = isTestTerminal ? 0 : 1;
                header.refername = "UPAY";
                msgModel.header = header;

                RequestModel request = new RequestModel();
                request.encoding = "json";
                request.mainaction = "CONNECTION";
                request.minoraction = "LOGIN";
                request.numbertemplate = 1;

                LoginModel param1 = new LoginModel();
                param1.Email = email;
                if (!String.IsNullOrEmpty(keyAuthnticate))
                {
                    param1.Key = keyAuthnticate;
                }
                else
                {
                    param1.Passwordmd5 = passMd5;
                }
                request.parameters = param1;
                msgModel.request = request;
                return msgModel;
            }
            catch (Exception ex)
            {
                //Logger.Write(ex, "Errors"); TODO
            }
            return new MsgModel();
        }

        public static string GetStringInMD5(string str)
        {
            if (String.IsNullOrEmpty(str))
                return null;

            StringBuilder sb = new StringBuilder();
            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));
                var r = md5.Hash;
                foreach (var i in r)
                    sb.Append(i.ToString("x2"));
            }
            catch (Exception ex)
            {
                return null;
            }
            return sb.ToString();
        }
    }
}
