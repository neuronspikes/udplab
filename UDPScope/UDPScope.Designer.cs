using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;

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

        public void UDPPacketReceivedCallback(IAsyncResult ar)
        {
            UdpClient u = (UdpClient)((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = (IPEndPoint)((UdpState)(ar.AsyncState)).e;

            Byte[] receiveBytes = u.EndReceive(ar, ref e);

            int width = receiveBytes.Length;

            if (this.screen.Image.Width < width) width = this.screen.Image.Width;
            //  todo: trace graph now
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
            this.screen.Location = new System.Drawing.Point(-2, 30);
            this.screen.Name = "screen";
            this.screen.Size = new System.Drawing.Size(906, 232);
            this.screen.TabIndex = 0;
            this.screen.TabStop = false;
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
            this.ClientSize = new System.Drawing.Size(903, 262);
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

