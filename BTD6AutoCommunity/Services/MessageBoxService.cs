using BTD6AutoCommunity.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTD6AutoCommunity.Services
{
    public class MessageBoxService : IMessageBoxService
    {
        public void ShowMessage(string message, string caption = "提示")
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public DialogResult ShowConfirmation(string message, string caption = "确认")
        {
            return MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        public DialogResult ShowError(string message, string caption = "错误")
        {
            return MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
