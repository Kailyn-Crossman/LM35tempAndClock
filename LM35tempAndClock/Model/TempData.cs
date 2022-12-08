using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM35tempAndClock.Model
{
    [ObservableObject]
    public partial class TempData
    {
        [ObservableProperty]
        string footBug;

        public TempData()
        {
            FootBug = "things";
        }
        public void parsePacket(string newPacket)
        {

        }

        public string getPacketNumber(string newPacket)
        {
            return newPacket.Substring(3, 3);
        }
    }
}
