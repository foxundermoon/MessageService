using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using agsXMPP.Xml.Dom;
using agsXMPP;
using agsXMPP.protocol.client;

namespace MessageService.Core.Xmpp
{
    public delegate void NodeHandler(XmppSeverConnection contextConnection,Node node);
    public delegate void IqHandler(XmppSeverConnection contextConnection,IQ iq);
    public delegate void MessageHandler(XmppSeverConnection contextConnection,Message message);
    public delegate void PresenceHandler(XmppSeverConnection contextContection,Presence presence);
    public delegate void UserOfflineHandler(string userName);

}
