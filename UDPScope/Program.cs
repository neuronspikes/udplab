using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.Net.Sockets;
using System.Net;

namespace UDPScope
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        public static Thread workerThread;

        [STAThread]
        static void Main()
        {




            // Start Window
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            UDPScope form = new UDPScope();

            // start thread to receive data and update screen
            int port = 11000;
            UdpClient udpClient = new UdpClient(port);

            //IPEndPoint object will allow us to read datagrams sent from any source.
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            Console.WriteLine("Receiving UDP packets on port " + port);
            UdpState s = new UdpState();
            s.e = RemoteIpEndPoint;
            s.u = udpClient;
            udpClient.BeginReceive(new AsyncCallback(form.UDPPacketReceivedCallback), s);

            
            Application.Run(form);

        }
    }

    public class UdpState
    {
        public UdpClient u;
        public IPEndPoint e;
    }

}
