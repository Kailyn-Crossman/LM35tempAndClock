using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LM35tempAndClock.Classes
{
    [ObservableObject]
    public partial class LMclass
    {
        // Function to get the analog value from a specific pin
        public double analogValue(string newPacket, int indexOfAnalog)
        {
            string[] analog;
            double analogReading = 0;
            analog = new string[5];

                for (int e = 0; e < 5; e++)
                {
                    //Read Pin
                    analog[e] = $"{newPacket.Substring(6 + e * 4, 4)}";
                }
                analogReading += Convert.ToInt32(analog[indexOfAnalog]);
            return analogReading;
        }

        //Maths up the analog value and converts it to a temperature 
        public double GetTemperature(double voltage)
        {
            double temperature = voltage / 10;
            return temperature;
        }

    }
}
