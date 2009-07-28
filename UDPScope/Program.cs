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
        private static UDPScope form;

        [STAThread]
        static void Main(string[] args)
        {




            // Start Window
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new UDPScope();

            // start thread to receive data and update screen
            int port = 11000;
            if(args.Length>0){
                port = int.Parse(args[0]);
        }
            form.StartReception(port);
            Application.Run(form);

        }



    }

    public class UdpState
    {
        public UdpClient u;
        public IPEndPoint e;
    }

}
