using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Drawing.Imaging;
namespace UDPScope
{
    public partial class UDPScope : Form
    {
        public UDPScope()
        {
            InitializeComponent();
        }

        public static bool messageReceived = false;
        private Byte[] lastData;
        private bool stopReception = false;
        UdpClient udpClient;

        public void UDPPacketReceivedCallback(IAsyncResult ar)
        {
            UdpClient u = (UdpClient)((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = (IPEndPoint)((UdpState)(ar.AsyncState)).e;
            try
            {
                Byte[] receiveBytes = u.EndReceive(ar, ref e);
                lastData = (Byte[])receiveBytes.Clone();
                screen.Invalidate();
            }
            catch (Exception ex)
            { }
            if (!stopReception) // continue listening
            {
                u.BeginReceive(new AsyncCallback(this.UDPPacketReceivedCallback), ar.AsyncState);
            }
        }

        public void updateImage()
        {
            //Console.Write("redrawing:");
            if (lastData != null)
            {
                //Console.WriteLine(" OK");
                int width = lastData.Length;

                Image image = this.screen.Image;
                if (image == null)
                {
                    this.screen.Image = new Bitmap(1024, 256, PixelFormat.Format32bppArgb);
                    image = this.screen.Image;
                }

                if (image.Width < width) width = image.Width;
                //  todo: trace graph now

                //Console.WriteLine("Working width=" + width);

                lock (this.screen)
                {
                    Rectangle srcRect = new Rectangle(0, 0, image.Width, image.Height);
                    BitmapData see = ((Bitmap)image).LockBits(srcRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                    // process
                    int PixelSize = 4; //A,R,G,B
                    int x = 0, y = 0;
                    int w = image.Width, h = image.Height;

                    unsafe
                    {
                        for (int yScan = y; yScan < (y + h); yScan++)
                        {

                            byte* row = (byte*)see.Scan0 + yScan * see.Stride;
                            for (int xScan = x; xScan < (x + w); xScan++)
                            {
                                int xRef = xScan * PixelSize;

                                row[xRef] = (yScan == 128 ? (byte)255 : (byte)0); // B
                                row[xRef + 1] = (lastData[xScan] > yScan ? (byte)0 : (byte)255); // G
                                row[xRef + 2] = (yScan % 16 == 0 || xScan % 16 == 0 ? (byte)255 : (byte)0);  // R = Grid
                                row[xRef + 3] = 255;  // A
                            }
                        }
                    }
                    ((Bitmap)image).UnlockBits(see);
                }
            }
            else
            {
                Console.WriteLine(" Redrawing aborted");

            }
        }



        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                StartReception(int.Parse(this.portNumber.Text));
                this.portNumber.BackColor = Color.Green;
            }
            catch (FormatException  ex)
            {
                    this.portNumber.BackColor = Color.Red;
            }
            catch (SocketException ex)
            {
                this.portNumber.BackColor = Color.Orange;
            }
        }
        public void StartReception(int port)
        {
            udpClient = new UdpClient(port);
            this.portNumber.Text = ""+port;
            
            //IPEndPoint object will allow us to read datagrams sent from any source.
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            Console.WriteLine("Receiving UDP packets on port " + port);
            UdpState s = new UdpState();
            s.e = RemoteIpEndPoint;
            s.u = udpClient;
            udpClient.BeginReceive(new AsyncCallback(this.UDPPacketReceivedCallback), s);


        }
        public PictureBox getScreen()
        {
            return this.screen;
        }

        private void screen_Paint(object sender, PaintEventArgs e)
        {
            updateImage();
        }

        private void Start_Click(object sender, EventArgs e)
        {
            if (stopReception)
            {
                stopReception = false;
                this.Start.Text = "Stop";
                this.textBox1_TextChanged(sender, e);
            }
            else
            {
                stopReception = true;
                this.Start.Text = "Start";
                udpClient.Close();
            }
        }
    }
}
