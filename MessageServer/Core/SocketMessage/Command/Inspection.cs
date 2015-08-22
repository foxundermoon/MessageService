using SuperSocket.SocketBase.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MessageService.Core.SocketMessage.Command
{
    public class Inspection : CommandBase<SocketSession, SocketRequestInfo>
    {
        public const string Response = "aloha";
        public const string Request = "$helo$";
        bool first = true;
        Timer timer;
        SocketSession session;
        public override string Name => Keys.Inspection;
        public override void ExecuteCommand(SocketSession session, SocketRequestInfo requestInfo)
        {
            session.Send(Response);
            //if (first)
            //{
            //    this.session = session;
            //    timer = new Timer();
            //    timer.Elapsed += Timer_Elapsed;
            //    timer.Interval = 20 * 1000;
            //    timer.Enabled = true;
            //}
            //first = false;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("throw exception! and stop server!");
            session.AppServer.Stop();
            //throw new Exception("test error");

        }
    }
}
