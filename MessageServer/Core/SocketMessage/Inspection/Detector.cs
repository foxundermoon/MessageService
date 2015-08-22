using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using SuperSocket.ClientEngine;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using SuperSocket.SocketEngine;
using SuperSocket.SocketBase;
using static FoxundermoonLib.ConsoleLog.L;

namespace MessageService.Core.SocketMessage.Inspection
{
    public class Detector
    {
        IBootstrap defaultBootstrap;
        public int Interval { get; set; } = 10;
        public static Detector Instance = new Detector();
        AsyncTcpSession ClientSession;
        public int RemoteSuperSocketPort { get; set; } = -1;
        Timer timer;
        const int bufferSize = 10;
        public void Start()
        {

            startSuperSocketServer();

        }

        private void ClientSession_DataReceived(object sender, DataEventArgs e)
        {
            string response = ASCIIEncoding.ASCII.GetString(e.Data, e.Offset, e.Length);

            if (response == Command.Inspection.Response)
            {
                received = true;
                I($"Socket服务正常 :{ DateTime.Now.ToString()}");
                return;
            }
            NoticeAdmin($"检测服务端返回的数据不正确 expect:{Command.Inspection.Response} actual:{response}");
        }

        private void NoticeAdmin(string msg)
        {
            //Todo  通知管理员
            E(msg);
        }

        private void ClientSession_Error(object sender, ErrorEventArgs e)
        {
            NoticeAdmin($"检测程序客户端连接错误错误 ： {e.ToString()}  \n Exception: {e.Exception?.ToString()}");

        }

        private void ClientSession_Connected(object sender, EventArgs e)
        {
          
            sendHello();
            I("客户端监测程序连接正常，服务端正常");
        }

        private void sendHello()
        {
            ClientSession.Send(sendData, 0, sendData.Length);
        }

        bool received = false;
        byte[] sendData = ASCIIEncoding.ASCII.GetBytes(Command.Inspection.Request + "\r\n");
        //SocketReceiveFilter.terminator;
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (received)
                sendHello();
            else
            {
                W("--------------------------------------------------------------------------------------------------------");
                I($"检测程序未接收到侦测消息，开始智能诊断并修复服务 {DateTime.Now.ToString()}");
                W("--------------------------------------------------------------------------------------------------------");

                restartSuperSocketServer();
                reconnectToSuperSocket();
            }
            received = false;
        }

        private void reconnectToSuperSocket()
        {
            if (ClientSession == null)
            {
                this.ClientSession = new AsyncTcpSession(new IPEndPoint(LocalIPAddress(), RemoteSuperSocketPort), bufferSize);
                ClientSession.Connected += ClientSession_Connected;
                I($"Create Dectector Connection:  IP:{LocalIPAddress().ToString()}  port:{RemoteSuperSocketPort}");
                ClientSession.Error += ClientSession_Error;
                ClientSession.DataReceived += ClientSession_DataReceived;
                timer = new Timer();
                timer.Elapsed += Timer_Elapsed;
                timer.Interval = Interval * 1000;
                timer.Enabled = true;
            }
            if (!ClientSession.IsConnected)
                ClientSession.Connect();
        }

        private void restartSuperSocketServer()
        {
            defaultBootstrap?.Stop();
            defaultBootstrap = null;
            var bootstrap = BootstrapFactory.CreateBootstrap();
            if (!bootstrap.Initialize())
                NoticeAdmin($"{nameof(restartSuperSocketServer) } :Supersocket  initlialize failed!");
            RemoteSuperSocketPort = bootstrap.AppServers.ElementAtOrDefault(0).Config.Port;
            var result = bootstrap.Start();
            Console.WriteLine("Start result: {0}!", result);
            if (result == StartResult.Failed)
                NoticeAdmin($"{nameof(restartSuperSocketServer) } :Supersocket  start failed! result:{result.ToString()}");
            if (result == StartResult.Success)
                defaultBootstrap = bootstrap;
        }
        private void startSuperSocketServer()
        {
            var bootstrap = BootstrapFactory.CreateBootstrap();
            defaultBootstrap = bootstrap;
            if (!bootstrap.Initialize())
                Program.Exit("Supersocket  initlialize failed!");

            var server = bootstrap.AppServers.ElementAt(0) as SocketServer;
            RemoteSuperSocketPort = server.Listeners[0].EndPoint.Port;
            var result = bootstrap.Start();
            Console.WriteLine("Start result: {0}!", result);
            if (result == StartResult.Failed)
                Program.Exit("Start Socket message service failed!");


            initClient();
        }

        private void initClient()
        {
            if (RemoteSuperSocketPort < 100)
            {
                throw new Exception("请先设置socket端口号");
            }
            reconnectToSuperSocket();
          

        }

        private Detector()
        {


        }

        private IPAddress LocalIPAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            return host
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

    }
}
