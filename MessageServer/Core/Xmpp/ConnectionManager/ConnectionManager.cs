using agsXMPP;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Core.Xmpp
{
 public  partial  class XmppServer
    {
     public ConcurrentDictionary<string, ConcurrentDictionary<string, XmppSeverConnection>> XmppConnectionDic { get; private set; }


    }
}
