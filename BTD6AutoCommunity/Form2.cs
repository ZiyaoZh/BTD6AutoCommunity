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
        private readonly System.Windows.Forms.Label MousePosLB;
        public OverlayForm()
        {
            MousePosLB = new System.Windows.Forms.Label { 
                AutoSize = true,
                BackColor = Color.FromArgb(30, 30, 30), // 深灰背景
                ForeColor = Color.White,                // 白色字体
                Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Regular),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleLeft,
            };
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
            MousePosLB.Invoke((MethodInvoker)delegate
            {
                MousePosLB.Text = $"{text}\nEnter自动输入";
                Location = new Point(screenPos.X + 10, screenPos.Y + 10);
            });
        }
    }
}