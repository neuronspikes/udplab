using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UDPScope
{
    public partial class UDPScope : Form
    {
        public UDPScope()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
           
        }
        public PictureBox getScreen()
        {
            return this.screen;
        }

        private void screen_Paint(object sender, PaintEventArgs e)
        {
            updateImage();
        }
    }
}
