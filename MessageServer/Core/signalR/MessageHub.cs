using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Core.signalR
{
    class MessageHub : Hub
    {
        public void Send(string addr,string message)
        {
            Clients.All.addMessage(addr, message);
        }
    }
}
