using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmppClient
{
    public delegate void XmppConnectionErrorHandler(object sender, ErrorMessage msg);
    public delegate void ReLoginFailedHandler(object sender,ErrorMessage msg);

    public class ErrorMessage :EventArgs
    {
        public ErrorType ErrT { get; set; }
        public string ErrorMsg { get; set; }
        public ErrorMessage(string msg)
        {
            ErrorMsg = msg;
        }
        public ErrorMessage(string msg, ErrorType et)
        {
            ErrorMsg = msg;
            ErrT = et;
        }
        public enum ErrorType
        {
            NoneResponse,
            Closed,
            AuthernitedFailed,
        }
    }
}
