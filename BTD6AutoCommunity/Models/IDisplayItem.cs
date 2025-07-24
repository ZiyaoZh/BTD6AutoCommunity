using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.Models
{
    public interface IDisplayItem
    {
        Type ValueType { get; }

        object Value { get; }

        string Name { get; }

        string ToString();


    }
}
