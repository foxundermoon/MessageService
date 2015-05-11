using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MessageService.Core.Xmpp.Handler
{
    public class HandlerException :Exception
    {
        public HandlerException(String msg):base(msg)
        {
        }
    }
}