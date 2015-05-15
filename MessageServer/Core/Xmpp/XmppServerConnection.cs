using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using agsXMPP.protocol;
using agsXMPP.protocol.iq;
using agsXMPP.protocol.iq.auth;
using agsXMPP.protocol.iq.roster;

using agsXMPP.Xml;
using agsXMPP.Xml.Dom;
using MessageService.Core.Account;
using MessageService.Core.Xmpp;
using agsXMPP.protocol.client;
using System.Configuration;

namespace agsXMPP
{
    /// <summary>
    /// Zusammenfassung  XMPPSeverConnection.
    /// </summary>
    public class XmppSeverConnection
    {
        public event MessageService.Core.Xmpp.IqHandler OnIq;
        public event MessageService.Core.Xmpp.MessageHandler OnMessage;
        public event MessageService.Core.Xmpp.PresenceHandler OnPresence;
        public event NodeHandler OnNode;
        #region << Constructors >>
        public XmppSeverConnection()
        {
            streamParser = new StreamParser();
            streamParser.OnStreamStart += new StreamHandler(streamParser_OnStreamStart);
            streamParser.OnStreamEnd += new StreamHandler(streamParser_OnStreamEnd);
            streamParser.OnStreamElement += new StreamHandler(streamParser_OnStreamElement);

        }

        public XmppSeverConnection(Socket sock)
            : this()
        {
            m_Sock = sock;
            m_Sock.BeginReceive(buffer, 0, BUFFERSIZE, 0, new AsyncCallback(ReadCallback), null);
        }

        public XmppSeverConnection(Socket sock, MessageService.Core.Xmpp.XmppServer server)
            : this(sock)
        {
            xmppServer = server;
        }
        #endregion
        private StreamParser streamParser;
        public string UserName { get; set; }
        private Socket m_Sock;
        private const int BUFFERSIZE = 2048;
        private byte[] buffer = new byte[BUFFERSIZE];
        private MessageService.Core.Xmpp.XmppServer xmppServer;
        public bool IsAuthentic { get; set; }

        public void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object

            // Read data from the client socket. 
            try
            {
                int bytesRead = m_Sock.EndReceive(ar);
                if (bytesRead > 0)
                {
                    streamParser.Push(buffer, 0, bytesRead);

                    // Not all data received. Get more.
                    m_Sock.BeginReceive(buffer, 0, BUFFERSIZE, 0, new AsyncCallback(ReadCallback), null);
                }
                else
                {
                    m_Sock.Shutdown(SocketShutdown.Both);
                    m_Sock.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("@XmppServerConnection.ReadCallback:" + e.Message);
                if (!string.IsNullOrEmpty(UserName))
                    xmppServer.UserOffline(UserName);
            }
        }

        public void Send(string data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            // Begin sending the data to the remote device.
            m_Sock.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), null);
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Complete sending the data to the remote device.
                int bytesSent = m_Sock.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to client.", bytesSent);

            }
            catch (Exception e)
            {
                Console.WriteLine("@XmppServerConnection.SendCallback:" + e.Message);
                //Console.WriteLine(e.ToString());
            }
        }


        public void Stop()
        {
            Send("</stream:stream>");
            //			client.Close();
            //			_TcpServer.Stop();

            m_Sock.Shutdown(SocketShutdown.Both);
            m_Sock.Close();
            if (!string.IsNullOrEmpty(UserName))
                xmppServer.UserOffline(UserName);
        }


        #region << Properties and Member Variables >>
        //		private int			m_Port			= 5222;		
        private string m_SessionId = null;

        public string SessionId
        {
            get
            {
                return m_SessionId;
            }
            set
            {
                m_SessionId = value;
            }
        }
        #endregion

        private void streamParser_OnStreamStart(object sender, Node e)
        {
            try
            {
                SendOpenStream();
            }
            catch (Exception ignore) { }
        }
        private void streamParser_OnStreamEnd(object sender, Node e)
        {
            if (!string.IsNullOrEmpty(UserName))
                xmppServer.UserOffline(UserName);

        }

        private void streamParser_OnStreamElement(object sender, Node node)
        {
            try
            {
                if (OnNode != null)
                    OnNode(this, node);
                if (OnIq != null && node.GetType() == typeof(IQ))
                    OnIq(this, node as IQ);
                if (OnMessage != null && node.GetType() == typeof(Message))
                    OnMessage(this, node as Message);
                if (OnPresence != null && node.GetType() == typeof(Presence))
                    OnPresence(this, node as Presence);
            }
            catch (Exception)
            {

            }

        }
        private void SendOpenStream()
        {

            // Recv:<stream:stream xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' from='myjabber.net' id='1075705237'>

            // Send the Opening Strem to the client
            string ServerDomain = ConfigurationManager.AppSettings["XmppServer"];

            this.SessionId = agsXMPP.SessionId.CreateNewId();


            StringBuilder sb = new StringBuilder();

            sb.Append("<stream:stream from='");
            sb.Append(ServerDomain);

            sb.Append("' xmlns='");
            sb.Append(Uri.CLIENT);

            sb.Append("' xmlns:stream='");
            sb.Append(Uri.STREAM);

            sb.Append("' id='");
            sb.Append(this.SessionId);

            sb.Append("'>");

            Send(sb.ToString());
        }

        public void Send(Element el)
        {
            Send(el.ToString());
        }


    }
}