using FoxundermoonLib.XmppEx.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmppEx
{
    public delegate void XmppConnectionErrorHandler(ErrorEvent msg);
    public delegate void ReLoginFailedHandler(ErrorEvent msg);
    public delegate void LoginHandler(LoginEvent msg);
    public delegate void MessageHandle(Message msg);
    public delegate void ErrorHandler(ErrorEvent msg);

    public class LoginEvent : BaseEvent<string>
    {
        public bool Success { get; set; }
        public LoginEvent(bool success, string msg)
            : base(msg)
        {
            Success = success;
        }
        public LoginEvent(bool success)
            : base("")
        {
            Success = success;
        }

    }
    public class MessageEvent : BaseEvent<Message>
    {
        public MessageEvent(Message msg)
            : base(msg)
        {

        }

    }
    public class ErrorEvent : BaseEvent<string>
    {
        private ErrorType errT;
        public ErrorType ErrT {
            get {
                if (errT == null)
                    return ErrorType.Common;
                return errT;
            }
            set {
                errT = value;
            }
        }
        public ErrorEvent(string msg)
            : base(msg)
        {
            ErrT = ErrorType.System;
        }
        public ErrorEvent(string msg, ErrorType et)
            : base(msg)
        {
            ErrT = et;
        }
        public enum ErrorType
        {
            NoneResponse,
            Closed,
            AuthernitedFailed,
            ParseMessageFailed,
            Common,
            System,
        }
    }
    public class BaseEvent<DataType> : EventArgs
    {
        public DataType EventData { get; set; }
        public BaseEvent(DataType data)
        {
            EventData = data;
        }
    }
}
