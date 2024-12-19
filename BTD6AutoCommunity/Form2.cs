using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTD6AutoCommunity
{
    public partial class OverlayForm : Form
    {
        private System.Windows.Forms.Label MousePosLB;
        public OverlayForm()
        {
            MousePosLB = new System.Windows.Forms.Label { AutoSize = true };
            ;
            MousePosLB.BackColor = Color.LightYellow;
            MousePosLB.Font = new System.Drawing.Font("宋体", 12, FontStyle.Regular); 
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(1, 1); // Small size to avoid taking much space
            BackColor = Color.LimeGreen; // Arbitrary color
            TransparencyKey = BackColor; // Set the transparency key to the form's back color

            Controls.Add( MousePosLB );

            TopMost = true;
            ShowInTaskbar = false;
        }

        public void UpdateLabelPosition(Point screenPos, string text)
        {
            MousePosLB.Text = text;
            Location = new Point(screenPos.X + 10, screenPos.Y + 10);
        }
    }
}