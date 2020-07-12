using Calender;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    public partial class Server : Form
    {
        IPEndPoint IP;
        Socket server;
        List<Socket> clientList;
        public Server()
        {
            InitializeComponent();
            Listen();
        }
        private void Listen()
        {
            clientList = new List<Socket>();

            IPAddress address = IPAddress.Parse(Cons.ip);
            IP = new IPEndPoint(address, 6740);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(IP);
            Thread listen = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        server.Listen(100);
                        Socket client = server.Accept();
                        clientList.Add(client);
                        Thread receive = new Thread(Receive);
                        receive.IsBackground = true;
                        receive.Start(client);
                    }
                }
                catch
                {
                    IP = new IPEndPoint(IPAddress.Any, 6740);
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                }
            });
            listen.IsBackground = true;
            listen.Start();
        }
        private void Disconnect()
        {
            server.Close();
        }
        private void Receive(object obj)
        {
            Socket client = obj as Socket;
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024];
                    client.Receive(data);
                    string message = (string)Deserialize(data);
                    AddMessage(message);
                    foreach (Socket item in clientList)
                    {
                        if (item != null && item != client)
                            item.Send(Serialize(message));
                    }
                }
            }
            catch
            {
                clientList.Remove(client);
                client.Close();
            }
        }
        private void AddMessage(string message)
        {
            string result = string.Empty;
            if (message.StartsWith("Admin"))
            {
                string[] str = message.Split(',');
                result = string.Format("Admin:Đã thêm người dùng {0} công việc {1}", str[1], str[2]);
            }
            else if (message.StartsWith("User"))
            {
                string[] str = message.Split(',');
                result = string.Format("User:{0} đã chuyển trạng thái công việc {1} thành {2}", str[1], str[2], str[3]);
            }
            lvMessage.Items.Add(new ListViewItem() { Text = result });
        }
        byte[] Serialize(object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatte = new BinaryFormatter();
            formatte.Serialize(stream, obj);
            return stream.ToArray();
        }
        object Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatte = new BinaryFormatter();
            return formatte.Deserialize(stream);

        }
        private void Server_FormClosed(object sender, FormClosedEventArgs e)
        {
            Disconnect();
        }
    }
}
