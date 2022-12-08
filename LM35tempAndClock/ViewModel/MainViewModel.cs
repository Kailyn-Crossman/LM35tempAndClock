using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LM35tempAndClock.Model;
using Microsoft.UI.Composition;

namespace LM35tempAndClock.ViewModel
{
    [ObservableObject]
    public partial class MainViewModel
    {
        
        string newPacket = "";
        private int oldPacketNumber = -1;
        private int newPacketNumber = 0;
        private int lostPacketCount = 0;
        private int packetRollover = 0;
        private int chkSumError = 0;

        [ObservableProperty]
        bool bPortOpen = false;
        [ObservableProperty]
        bool buttonPressed = false;

        [ObservableProperty]
        string[] itemsSource;
        [ObservableProperty]
        int selectedIndex;
        [ObservableProperty]
        object selectedItem;
        [ObservableProperty]
        string lblOpenClose = "Open";
        [ObservableProperty]
        string footBug;

        string[] ports;

        public TempData tempData { get; set; } = new TempData();

        SerialPort serialPort = new SerialPort();

        public MainViewModel()
        {
            ports = SerialPort.GetPortNames();
            ItemsSource = ports;
            SelectedIndex = ports.Length;


            serialPort.BaudRate = 115200;
            serialPort.ReceivedBytesThreshold = 1;
            serialPort.DataReceived += SerialPort_DataReceived;
        }



        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            tempData.FootBug = "im here";
            newPacket = serialPort.ReadLine();
            MainThread.BeginInvokeOnMainThread(MyMainThreadCode);
        }

        private void MyMainThreadCode()
        {

            int calChkSum = 0;
            if (newPacket.Length > 37)
            {

                if (newPacket.Substring(0, 3) == "###")
                {
                    newPacketNumber = Convert.ToInt32(newPacket.Substring(3, 3));

                    if (oldPacketNumber > -1)
                    {
                        if (newPacketNumber < oldPacketNumber)
                        {
                            packetRollover++;
                            if (oldPacketNumber != 999)
                            {
                                lostPacketCount += 999 - oldPacketNumber + newPacketNumber;
                            }
                        }
                        else
                        {
                            if (newPacketNumber != oldPacketNumber + 1)
                            {
                                lostPacketCount += newPacketNumber - oldPacketNumber;
                            }
                        }
                    }
                    for (int i = 3; i < 34; i++)
                    {
                        calChkSum += (byte)newPacket[i];
                    }
                    calChkSum %= 1000;
                    int recChkSum = Convert.ToInt32(newPacket.Substring(34, 3));
                    if (recChkSum == calChkSum)
                    {

                        //Temperature(newPacket);
                        //   HighSensor(lmClass.avgAnalogValue(newPacket, 0));
                        oldPacketNumber = newPacketNumber;
                    }
                    else
                    {
                        chkSumError++;
                    }

                    string parsedData = $"{newPacket.Length,-14}" +
                                       $"{newPacket.Substring(0, 3),-14}" +
                                       $"{newPacket.Substring(3, 3),-14}" +
                                       $"{newPacket.Substring(6, 4),-14}" +
                                       $"{newPacket.Substring(10, 4),-14}" +
                                       $"{newPacket.Substring(14, 4),-14}" +
                                       $"{newPacket.Substring(18, 4),-14}" +
                                       $"{newPacket.Substring(22, 4),-14}" +
                                       $"{newPacket.Substring(26, 4),-14}" +
                                       $"{newPacket.Substring(30, 4),-14}" +
                                       $"{newPacket.Substring(34, 3),-17}" +
                                       $"{calChkSum,-19}" +
                                       $"{lostPacketCount,-11}" +
                                       $"{chkSumError,-14}" +
                                       $"{packetRollover,-14}\r\n";

                        //labelParsedData.Text = parsedData;
                
                }

            }

        }
        [RelayCommand]
        void OpenClose()
        {
            if (!buttonPressed)
            {
                buttonPressed = true;
            } else if (buttonPressed)
            {
                buttonPressed = false;
            }
            if (!bPortOpen && buttonPressed)
            {
                serialPort.PortName = SelectedItem.ToString();
                serialPort.Open();
                LblOpenClose = "Close";
                bPortOpen = true;
                
            }
            else
            {
                serialPort.Close();
                LblOpenClose = "Open";
                bPortOpen = false;
            }

        }
    }
}
