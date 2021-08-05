using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {
        public class ThreadPoolTcpSrvr
        {
            private TcpListener client;

            public ThreadPoolTcpSrvr()
            {

                client = new TcpListener(IPAddress.Any, 9050);
                client.Start();
                while (true)
                {
                    while (!client.Pending())
                    {
                        Thread.Sleep(1000);
                    }
                    ConnectionThread newconnection = new ConnectionThread();
                    newconnection.threadListener = this.client;
                    ThreadPool.QueueUserWorkItem(new
                               WaitCallback(newconnection.HandleConnection));
                }
            }

          
        }

        class ConnectionThread
        {
            public TcpListener threadListener;
            public void HandleConnection(object state)
            {
                byte[] data = new byte[1024];
                TcpClient client = threadListener.AcceptTcpClient();
                NetworkStream ns = client.GetStream();

                string command;
                command = "wmic nic where (AdapterTypeId=0 AND netConnectionStatus=2) get MACAddress";
                // execute command
                Process p = new Process();
                p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/C " + command;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                data = Encoding.ASCII.GetBytes(output);
                ns.Write(data, 0, data.Length);
                ns.Close();
                client.Close();
            }
        }
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ThreadPoolTcpSrvr tpts = new ThreadPoolTcpSrvr();
        }

        protected override void OnStop()
        {
        }
    }
}
