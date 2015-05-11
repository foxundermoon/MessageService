using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using agsXMPP;
using agsXMPP.protocol.client;

namespace XmppClient
{
    public partial class Form1 : Form
    {
        XmppClientConnection xmpp ;
        Jid selfJid;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("不能为空");
            }
            else
            {
                try
                {
                    int uid = int.Parse(textBox3.Text);
                    string selfJidStr = uid+"@10.80.5.222/winformClient";
                    selfJid = new Jid(selfJidStr);
                    string PASSWORD = "secret";   // password of the JIS_SENDER account

                    //string JID_RECEIVER = "456@10.80.5.254";

                    xmpp = new XmppClientConnection(selfJid.Server);
                    xmpp.Open(selfJid.User, PASSWORD);
                    xmpp.OnMessage += new MessageHandler(xmpp_OnMessage);
                    agsXMPP.protocol.client.Message message = new agsXMPP.protocol.client.Message();
                    message.To = new Jid("0@10.80.5.222");
                    message.From = selfJid;
                    message.Body = "notification server";
                    xmpp.Send(message);
                }
                catch (FormatException fe)
                {
                    MessageBox.Show("uid只能是数字!");

                }
            }
           
            //xmpp.OnReadXml += xmpp_OnReadXml;
            //xmpp.OnLogin += delegate(object o) { xmpp.Send(new Message(new Jid(JID_RECEIVER), MessageType.chat, "Hello, how are you?")); };


            //Console.WriteLine("Wait until you get the message and press a key to continue");
            //Console.ReadLine();

        }

        void xmpp_OnReadXml(object sender, string xml)
        {
            textBox1.AppendText(xml);
        }

        private void xmpp_OnMessage(object sender, agsXMPP.protocol.client.Message msg)
        {
            textBox1.AppendText(msg.From);
            textBox1.AppendText(msg.Body);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show("要发送到的uid不能为空!");
            }
            else if(string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("消息不能为空!");   
            }else
            {
                try
                {
                    int touid = int.Parse(textBox4.Text);
                    string JID_RECEIVER = touid + "@10.80.5.254";
                    agsXMPP.protocol.client.Message m = new agsXMPP.protocol.client.Message(new Jid(JID_RECEIVER), MessageType.chat, textBox2.Text);
                    m.To = new Jid(JID_RECEIVER);
                    m.From = selfJid;
                    xmpp.Send(m);
                }catch(FormatException fe)
                {
                    MessageBox.Show("要发送到的uid只能是数字");
                }
            }

          
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
        }
    }
}
