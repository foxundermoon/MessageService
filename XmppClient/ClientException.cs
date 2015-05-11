using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmppClient
{
    class ClientException:Exception
    {
        public string  Message {get;set;}
        public Exception InnerException{get;set;}
        public ClientException(string msg):base(msg)
        {
            Message = msg;
        }
        public ClientException(string msg,Exception inner):base(msg,inner)
        {
            Message =msg;
            InnerException = inner;

        }
    }
}
