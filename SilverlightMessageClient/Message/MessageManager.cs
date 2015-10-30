using Microsoft.AspNet.SignalR.Client;
using System;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading.Tasks;
namespace SilverlightMessageClient.MessageManager
{
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
        public ErrorType ErrT
        {
            get
            {
                if (errT == null)
                    return ErrorType.Common;
                return errT;
            }
            set
            {
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
    public class MessageManager
    {
        static MessageManager instance;
        public static MessageManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (instance == null)
                        instance = new MessageManager();
                    return instance;
                }
            }
        }
        static object _lock = new object();
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string Resource { get; set; }
        public string MessageServerHost { get; set; }
        public int MessageServerPort { get; set; }
        public event LoginHandler OnLogin;
        public event MessageHandle OnMessage;
        public event ErrorHandler OnError;
        Connection connection;

        public bool Start()
        {

            //if (string.IsNullOrEmpty(UserName))
            //{
            //    if (OnError != null)
            //        OnError(new ErrorEvent("用户名为空"));
            //    return false;

            //}
            //if (string.IsNullOrEmpty(UserPassword))
            //{
            //    if (OnError != null)
            //        OnError(new ErrorEvent("用户名密码为空"));
            //    return false;
            //}
            if (string.IsNullOrEmpty(MessageServerHost))
            {
                if (OnError != null)
                    OnError(new ErrorEvent("消息服务主机名为空"));
                return false;
            }
            if (MessageServerPort < 1)
            {
                if (OnError != null)
                    OnError(new ErrorEvent("消息服务端口号没设置"));
                return false;
            }
            connection = new Connection(getConnectionUrl());
            //连接 signalR
            connection.Start().ContinueWith(task =>
            {
                
                if (task.IsFaulted)
                {
                    if (OnLogin != null)
                    {
                        //OnError(new ErrorEvent("服务器连接错误"));
                        OnLogin(new LoginEvent(false));
                    }
                }
                if (task.IsCompleted)
                {
                    if (OnLogin != null)
                        OnLogin(new LoginEvent(true));
                }
            }
            );
            return false;
        }

        private string getConnectionUrl()
        {
            var builder = new UriBuilder();
            builder.Scheme = Uri.UriSchemeHttp;
            builder.Host = MessageServerHost;
            builder.Port = MessageServerPort;
            builder.Path = "signalr";

            return builder.Uri.ToString();
        }






        //void XmppClient_OnXmppError(ErrorEvent msg)
        //{
        //    if (OnError != null)
        //        OnError(msg);
        //}


        //void XmppClient_OnLogin(object sender)
        //{
        //    if (OnLogin != null)
        //        OnLogin(new LoginEvent(true));
        //}

        //void XmppClient_OnMessage(object sender, agsXMPP.protocol.client.Message msg)
        //{
        //    var body = "";
        //    var subject = "";
        //    if (msg != null)
        //    {
        //        if ("BASE64".Equals(msg.Language) && !string.IsNullOrEmpty(msg.Body))
        //        {
        //            body = FoxundermoonLib.Encrypt.EncryptUtil.DecryptBASE64ByGzip(msg.Body);
        //        }
        //        else
        //        {
        //            body = msg.Body;
        //        }
        //        subject = msg.Subject;
        //        Message msgEntity = new Message();
        //        try
        //        {
        //            if (!string.IsNullOrEmpty(body))
        //                msgEntity.SetJsonMessage(body);
        //            if (!string.IsNullOrEmpty(subject))
        //                msgEntity.SetJsonCommand(subject);
        //            if (OnMessage != null)
        //                OnMessage(msgEntity);
        //        }
        //        catch (Exception e)
        //        {
        //            var err = new ErrorEvent(e.Message);
        //            err.ErrT = ErrorEvent.ErrorType.ParseMessageFailed;
        //            if (OnError != null)
        //                OnError(err);
        //        }
        //        if (msgEntity.Command.NeedResponse)
        //        {
        //            Message response = new Message();
        //            response.Command.Name = Cmd.Response;
        //            response.ToUser = msgEntity.FromUser;
        //            response.FromUser = msgEntity.ToUser;
        //            SendMessage(response);
        //        }

        //    }
        //}




        public void SendMessage(Message m)
        {
            //xmppMsg.Body = FoxundermoonLib.Encrypt.EncryptUtil.EncryptBASE64ByGzip(m.ToJson());
            //if (null == m.FromUser || string.IsNullOrEmpty(m.FromUser.Name))
            //    m.FromUser = new User(XmppClient.Name, Resource);
            //if (null == m.ToUser || string.IsNullOrEmpty(m.ToUser.Name))
            //    m.ToUser = new User("0", "server");
            //xmppMsg.From = new agsXMPP.Jid(m.FromUser.Name + "@" + MessageServerHost + ":" + MessageServerPort + "/" + m.FromUser.Resource);
            //xmppMsg.To = new agsXMPP.Jid(m.ToUser.Name + "@" + MessageServerHost + ":" + MessageServerPort + "/" + m.ToUser.Resource);
            //XmppClient.XmppConnection.Send(xmppMsg);
            try {
                connection.Send(m.ToJson());
            }catch(Exception e)
            {
                if (OnError != null)
                    OnError(new ErrorEvent("send message error:"+e.Message));
            }
        }
    }
}