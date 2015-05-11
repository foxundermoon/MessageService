using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.Xml.Dom;
using System.Collections.Concurrent;
using System.Configuration;
using MessageService.Core.Config;
using MessageService.Core.Utils;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
namespace MessageService.Core.Xmpp {
    public partial class XmppServer {
        static object _lock = new object();
        // Thread signal.
        private ManualResetEvent allDone = new ManualResetEvent(false);
        private Socket listener;
        private bool m_Listening;
        static XmppServer instance;
        public ConcurrentDictionary<string, XmppSeverConnection> XmppConnectionDic { get; private set; }
        //public event EventHandler<int> ConnectionEncrease;
        //public event EventHandler ConnectionDecrease;
        public static Jid ServerJid;
        public Config.ServerConfig Config;
        public static XmppServer Instance {
            get {
                lock(_lock) {
                    if(instance==null) {
                        instance = new XmppServer();
                    }
                    return instance;
                }
            }
        }

        private XmppServer( ) {
            initConfig();
            XmppConnectionDic = new ConcurrentDictionary<string, XmppSeverConnection>();// new Dictionary<int, XmppSeverConnection>();
            ServerJid = new agsXMPP.Jid(Config.ServerUid.ToString(), Config.ServerIp, Config.ServerResource);
        }
        private void initConfig( ) {
            Config = new ServerConfig();
            Config.FileCollection = ConfigurationManager.AppSettings["FileCollection"].ToString();
            Config.FileServer = ConfigurationManager.AppSettings["FileServer"].ToString();
            Config.FileServerPort =int.Parse(ConfigurationManager.AppSettings["FileServerPort"].ToString());
            Config.LogLevel=int.Parse(ConfigurationManager.AppSettings["FileServerPort"].ToString());
            Config.MessageCollection=  ConfigurationManager.AppSettings["MessageCollection"].ToString();
            Config.MongoDatabase=  ConfigurationManager.AppSettings["MongoDatabase"].ToString();
            Config.MongoServer=  ConfigurationManager.AppSettings["MongoServer"].ToString();
            Config.UserCollection= ConfigurationManager.AppSettings["UserCollection"].ToString();
            Config.XmppPort =int.Parse(ConfigurationManager.AppSettings["XmppServerPort"].ToString());
            Config.ServerResource = ConfigurationManager.AppSettings["ServerResource"].ToString();
            Config.ServerIp=  ConfigurationManager.AppSettings["ServerIp"].ToString();
            Config.ServerUid=int.Parse(ConfigurationManager.AppSettings["ServerUid"].ToString());
            //ConfigurationManager.AppSettings["ServerResource"].ToString();
            //ConfigurationManager.AppSettings["ServerResource"].ToString();
            //ConfigurationManager.AppSettings["ServerResource"].ToString();
            //ConfigurationManager.AppSettings["ServerResource"].ToString();
            //ConfigurationManager.AppSettings["ServerResource"].ToString();
            //ConfigurationManager.AppSettings["ServerResource"].ToString();
            //ConfigurationManager.AppSettings["ServerResource"].ToString();

        }
        public static XmppServer GetInstance( ) {
            return Instance;
        }

        public void StartUp( ) {
            try {
                ThreadStart myThreadDelegate = new ThreadStart(Listen);
                Thread myThread = new Thread(myThreadDelegate);
                Console.WriteLine("开始监听 xmpp服务");
                myThread.Start();
            } catch(Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }
        private void Listen( ) {
            try {
                int port =Config.XmppPort;
                if(port<1024)
                    port = 5222;
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);

                // Create a TCP/IP socket.
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


                // Bind the socket to the local endpoint and listen for incoming connections.
                try {
                    listener.Bind(localEndPoint);
                    int loglevel = Config.LogLevel;
                    if(loglevel<0 || loglevel>10)
                        loglevel=10;
                    listener.Listen(loglevel);

                    m_Listening = true;

                    while(m_Listening) {
                        // Set the event to nonsignaled state.
                        allDone.Reset();

                        // Start an asynchronous socket to listen for connections.
                        //Console.WriteLine("Waiting for a connection...");
                        listener.BeginAccept(new AsyncCallback(AcceptCallback), null);

                        // Wait until a connection is made before continuing.
                        allDone.WaitOne();
                    }

                } catch(Exception ex) {
                    Console.WriteLine(ex.ToString());
                    //ExceptionCollection.InsertOneAsync(MongoUtil.GetExceptionBsonDocument(ex));
                }

            } catch(Exception e) {
                Console.WriteLine(e.ToString());
                //ExceptionCollection.InsertOneAsync(MongoUtil.GetExceptionBsonDocument(e));
            }
        }
        private void AcceptCallback( IAsyncResult ar ) {
            // Signal the main thread to continue.
            allDone.Set();
            // Get the socket that handles the client request.
            Socket newSock = listener.EndAccept(ar);

            Console.WriteLine("从 "+newSock.RemoteEndPoint.ToString() +"建立了一条tcp连接");

            XmppSeverConnection con = new XmppSeverConnection(newSock, this);
            con.OnNode+= new NodeHandler(OnNode);
            con.OnIq += new IqHandler(OnIQ);
            con.OnMessage += new MessageHandler(OnMessage);
            con.OnPresence += new PresenceHandler(OnPresence);
            /// you can  register other handler  here
            //listener.BeginReceive(buffer, 0, BUFFERSIZE, 0, new AsyncCallback(ReadCallback), null);
        }
        private void stop( ) {
            m_Listening = false;
            allDone.Set();
            //allDone.Reset();
        }
    }
}