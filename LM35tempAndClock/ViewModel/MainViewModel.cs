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
using LM35tempAndClock.Classes;

namespace LM35tempAndClock.ViewModel
{
    [ObservableObject]
    public partial class MainViewModel
    {
        
        private int oldPacketNumber = -1;
        private int newPacketNumber = 0;
        private int lostPacketCount = 0;
        private int packetRollover = 0;
        private int chkSumError = 0;
        string hereParsedData;

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
        string newPacket = "";
        [ObservableProperty]
        string footBug;

        [ObservableProperty]
        string lblWarning;
        [ObservableProperty]
        string led1 = "ledoff.png";


        [ObservableProperty]
        bool rPHistory;
        [ObservableProperty]
        bool pPHistory;
        [ObservableProperty]
        string parsedData;
        [ObservableProperty]
        string receivedData;


        string[] ports;

        //sets up using variables from the TempData class
        public TempData tempData { get; set; } = new TempData();
        //sets up using functions from LMclass
        public LMclass lmClass { get; set; } = new LMclass();

        SerialPort serialPort = new SerialPort();
        StringBuilder stringBuilderSend = new StringBuilder("###1111196");

        public MainViewModel()
        {
            ports = SerialPort.GetPortNames();
            //Sends the selected port through MVVM to the AppSheel.xaml
            ItemsSource = ports;
            SelectedIndex = ports.Length;


            serialPort.BaudRate = 115200;
            serialPort.ReceivedBytesThreshold = 1;
            serialPort.DataReceived += SerialPort_DataReceived;
        }


        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //When the serial port receives data assign it to the newPacket variable
            newPacket = serialPort.ReadLine();
            MainThread.BeginInvokeOnMainThread(MyMainThreadCode);
        }

        private void MyMainThreadCode()
        {
            //RPHistory is a checkbox linked from AppShell.xaml
            if (RPHistory == true)
            {
                //if the checkbox is checked scroll the data
                ReceivedData = NewPacket + ReceivedData;
            }
            else
            {
                //if the checkbox is unchecked just how one line updating
                ReceivedData = NewPacket;
            }

            int calChkSum = 0;
            // check for a vaild packet
            if (NewPacket.Length > 37)
            {

                if (NewPacket.Substring(0, 3) == "###")
                {
                    newPacketNumber = Convert.ToInt32(NewPacket.Substring(3, 3));

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
                        calChkSum += (byte)NewPacket[i];
                    }
                    calChkSum %= 1000;
                    int recChkSum = Convert.ToInt32(NewPacket.Substring(34, 3));
                    // if the packet is valid then
                    if (recChkSum == calChkSum)
                    {
                        Temperature(NewPacket);
                        HighSensor(lmClass.analogValue(NewPacket, 0));
                        oldPacketNumber = newPacketNumber;
                    }
                    else
                    {
                        chkSumError++;
                    }
                    // locally parse all the data
                    hereParsedData = $"{NewPacket.Length,-14}" +
                                       $"{NewPacket.Substring(0, 3),-14}" +
                                       $"{NewPacket.Substring(3, 3),-14}" +
                                       $"{NewPacket.Substring(6, 4),-14}" +
                                       $"{NewPacket.Substring(10, 4),-14}" +
                                       $"{NewPacket.Substring(14, 4),-14}" +
                                       $"{NewPacket.Substring(18, 4),-14}" +
                                       $"{NewPacket.Substring(22, 4),-14}" +
                                       $"{NewPacket.Substring(26, 4),-14}" +
                                       $"{NewPacket.Substring(30, 4),-14}" +
                                       $"{NewPacket.Substring(34, 3),-17}" +
                                       $"{calChkSum,-19}" +
                                       $"{lostPacketCount,-11}" +
                                       $"{chkSumError,-14}" +
                                       $"{packetRollover,-14}\r\n";

                    if (PPHistory == true)
                    {
                        //send the parsed data to debug window and scroll
                        ParsedData = hereParsedData + ParsedData;
                    }
                    else
                    {
                        //send the parsed data to debug window
                        ParsedData = hereParsedData;
                    }

                }

            }

        }
        //When the button in the AppShell.xaml then
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
                //get the observable value SelectedItem from port picker in xaml
                //and assign it to PortName to specify which serialPort to open
                serialPort.PortName = SelectedItem.ToString();
                serialPort.Open();
                LblOpenClose = "Close"; //sets the button text to "Close"
                bPortOpen = true; //toggles button state
                
            }
            else
            {
                serialPort.Close();
                LblOpenClose = "Open";
                bPortOpen = false;
            }

        }

        private void Temperature(string validPacket)
        {
            double temperature0 = lmClass.GetTemperature(lmClass.analogValue(validPacket, 0));
            double temperature1 = lmClass.GetTemperature(lmClass.analogValue(validPacket, 1));
            //set the LblTemperature variable in the tempData class so then it can be referenced in the UserInterface.xaml
            tempData.LblTemperature0 = temperature0.ToString("  00.0") + " °C"; //formats the display of temperature data
            tempData.LblTemperature1 = temperature1.ToString("  00.0") + " °C";
        }

        public void HighSensor(double voltage)
        {
            double temperature = (voltage / 10);
            if (temperature > 25)
            {
                stringBuilderSend[3] = '0'; //gets ready to turn the active low LED on
                Led1 = "ledon.png"; //changes the software led
                LblWarning = "  To Hot!"; //user interface danger warning

            }
            else if (temperature < 24.7)
            {
                stringBuilderSend[3] = '1';
                Led1 = "ledoff.png";
                LblWarning = "  Okay";
            }
            sendPacket(); //sends the serial data specified by the stringBuilderSend
        }

        private void sendPacket()
        {
            int calSendChkSum = 0;
            try
            {
                for (int i = 3; i < 7; i++)
                {
                    calSendChkSum += (byte)stringBuilderSend[i]; //checks that the packet trying to be sent is valid
                }
                calSendChkSum %= 1000;
                stringBuilderSend.Remove(7, 3);
                stringBuilderSend.Insert(7, calSendChkSum.ToString());
                string messageOut = stringBuilderSend.ToString(); //sets up the serial data to be sent
                messageOut += "\r\n"; //adds a carrige return line feed to serial data
                byte[] messageBytes = Encoding.UTF8.GetBytes(messageOut);
                serialPort.Write(messageBytes, 0, messageBytes.Length); //sends the message through the serial port
            }
            catch (Exception ex)
            {
              // DisplayAlert("Alert", ex.Message, "OK");
            }

        }
    }
}
