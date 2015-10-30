using SilverlightMessageClient.MessageManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightMessageSample
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Message msg = new Message();
            msg.Command.Name = "sl client test";
            msg.FromUser = new User("user3", "sl5");
            msg.ToUser = new User("user1", "winform");
            MessageManager.Instance.SendMessage(msg);

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageManager.Instance.MessageServerHost = serverHost.Text;
            MessageManager.Instance.MessageServerPort = Convert.ToInt32(serverPort.Text);
            MessageManager.Instance.Start();

        }

        private void Instance_OnMessage(Message msg)
        {
            p(msg.ToJson());
        }

        private void Instance_OnLogin(LoginEvent msg)
        {
            if (msg.Success)
                p("login success");
            else
                p("login failed");
        }
        void p(string msg)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                textBlock.Text += msg + "\r\n";
            });
        }

        private void Instance_OnError(ErrorEvent msg)
        {
            p(msg.ErrT + msg.EventData);
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            MessageManager.Instance.OnError += Instance_OnError;
            MessageManager.Instance.OnLogin += Instance_OnLogin;
            MessageManager.Instance.OnMessage += Instance_OnMessage;
        }
    }
}
