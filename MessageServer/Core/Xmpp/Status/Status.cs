using agsXMPP;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Core.Xmpp
{
    public partial class XmppServer
    {
        public event UserOnlineStatusHandler UserOffLineHandler;
        public event UserOnlineStatusHandler UserOnLineHandler;
        public void UserOffline(FoxundermoonLib.XmppEx.Data.User user)
        {
            try
            {
                ConcurrentDictionary<string, XmppSeverConnection> cons = null;
                var hasCons = XmppConnectionDic.TryGetValue(user.Name, out cons);
                if (hasCons)
                {
                    XmppSeverConnection con = null;
                    var hasCon = cons.TryGetValue(user.Resource, out con);
                    if (hasCon)
                    {
                        cons.TryRemove(user.Resource,out con);
                    }
                    var offLine = new FoxundermoonLib.XmppEx.Data.Message();
                    offLine.Command.Name = FoxundermoonLib.XmppEx.Command.Cmd.UserOffLine;
                    offLine.AddProperty("UserName", user.Name + "/" + user.Resource);
                    Broadcast(offLine);
                    if (UserOffLineHandler != null)
                        UserOffLineHandler(user);
                }
            }
            catch (Exception e)
            {

            }
          

        }

        public void UserOnline(FoxundermoonLib.XmppEx.Data.User user)
        {
            var onLogin = new FoxundermoonLib.XmppEx.Data.Message();
            onLogin.Command.Name = FoxundermoonLib.XmppEx.Command.Cmd.UserLogin;
            onLogin.AddProperty("UserName", user.Name +"/"+user.Resource);
            Broadcast(onLogin);
            if (UserOnLineHandler != null)
                UserOnLineHandler(user);
        }
    }
}
