using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTD6AutoCommunity.Services.Interfaces
{
    public interface IMessageBoxService
    {
        void ShowMessage(string message, string caption = "提示");
        DialogResult ShowConfirmation(string message, string caption = "确认");
        DialogResult ShowError(string message, string caption = "错误");
    }
}
