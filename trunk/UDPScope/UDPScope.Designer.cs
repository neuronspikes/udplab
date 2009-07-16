using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace UDPScope
{
    partial class UDPScope
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }


        public static bool messageReceived = false;
        private Byte[] lastData;

        public void UDPPacketReceivedCallback(IAsyncResult ar)
        {
            UdpClient u = (UdpClient)((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = (IPEndPoint)((UdpState)(ar.AsyncState)).e;

            Byte[] receiveBytes = u.EndReceive(ar, ref e);
            lastData = (Byte[])receiveBytes.Clone();
            screen.Invalidate();
            //Console.WriteLine("received " + lastData.Length + " bytes");
            u.BeginReceive(new AsyncCallback(this.UDPPacketReceivedCallback), ar.AsyncState);
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
                    this.screen.Image = new Bitmap(1024, 256,PixelFormat.Format32bppArgb);
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




        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.screen = new System.Windows.Forms.PictureBox();
            this.Start = new System.Windows.Forms.Button();
            this.portNumber = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.screen)).BeginInit();
            this.SuspendLayout();
            // 
            // screen
            // 
            this.screen.Location = new System.Drawing.Point(12, 29);
            this.screen.Name = "screen";
            this.screen.Size = new System.Drawing.Size(1024, 256);
            this.screen.TabIndex = 0;
            this.screen.TabStop = false;
            this.screen.Paint += new System.Windows.Forms.PaintEventHandler(this.screen_Paint);
            // 
            // Start
            // 
            this.Start.Location = new System.Drawing.Point(118, 0);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(75, 23);
            this.Start.TabIndex = 1;
            this.Start.Text = "Start";
            this.Start.UseVisualStyleBackColor = true;
            // 
            // portNumber
            // 
            this.portNumber.Location = new System.Drawing.Point(12, 3);
            this.portNumber.Name = "portNumber";
            this.portNumber.Size = new System.Drawing.Size(100, 20);
            this.portNumber.TabIndex = 2;
            this.portNumber.Text = "11000";
            this.portNumber.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // UDPScope
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1044, 291);
            this.Controls.Add(this.portNumber);
            this.Controls.Add(this.Start);
            this.Controls.Add(this.screen);
            this.Name = "UDPScope";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.screen)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox screen;
        private System.Windows.Forms.Button Start;
        private System.Windows.Forms.TextBox portNumber;
    }
}

