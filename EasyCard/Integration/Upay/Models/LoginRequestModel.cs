using System;
using System.Collections.Generic;
using System.Text;

namespace Upay.Models
{
    public  class LoginRequestModel
    {
        public MsgModel msg { get; set; }
        public LoginRequestModel(MsgModel _msg)
        {
            this.msg = _msg;
        }
        public LoginRequestModel()
        {

        }
    }
}
