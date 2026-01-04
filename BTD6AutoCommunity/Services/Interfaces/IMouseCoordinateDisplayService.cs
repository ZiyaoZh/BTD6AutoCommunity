using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.Services.Interfaces
{
    public interface IMouseCoordinateDisplayService
    {
        void StartDisplay(Action<(double, double)> onEnterPressed);
        void StopDisplay();
    }
}
