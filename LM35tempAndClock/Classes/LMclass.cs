using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LM35tempAndClock.Classes
{
    [ObservableObject]
    public partial class LMclass
    {

        public double avgAnalogValue(string newPacket, int indexOfAnalog)
        {
            string[] voltage;
            double voltageReading = 0;
            voltage = new string[5];
            for (int i = 0; i < 20; i++)
            {
                for (int e = 0; e < 5; e++)
                {
                    //Read Pin
                    voltage[e] = $"{newPacket.Substring(6 + e * 4, 4)}";
                }
                voltageReading += Convert.ToInt32(voltage[indexOfAnalog]);
            }
            voltageReading = voltageReading / 20;
            return voltageReading;
        }

        public double GetTemperature(double voltage)
        {
            double temperature = voltage / 10;
            return temperature;
        }

    }
}
