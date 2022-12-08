using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LM35tempAndClock.Model;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using Windows.Devices.Enumeration;

namespace LM35tempAndClock.View;

[ObservableObject]
public partial class UserInterface
{
    
    private string newPacket = "";
    private int oldPacketNumber = -1;
    private int newPacketNumber = 0;
    private int lostPacketCount = 0;
    private int packetRollover = 0;
    private int chkSumError = 0;

    [ObservableProperty]
    bool bPortOpen;
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
    string lblBug;

    public TempData tempData { get; set; } = new TempData();

    SerialPort serialPort = new SerialPort();
    StringBuilder stringBuilderSend = new StringBuilder("###1111196");

    LMclass lmClass = new LMclass();
    public UserInterface()
    {
        InitializeComponent();

        string[] ports = SerialPort.GetPortNames();
        ItemsSource = ports;
        SelectedIndex = ports.Length;
        Loaded += UserInterface_Loaded;
    }

    private void UserInterface_Loaded(object sender, EventArgs e)
    {
        debug.Text = tempData.FootBug;
        tempData.PropertyChanged += TempData_PropertyChanged;
 
    }

    private void TempData_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {

    }


    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        newPacket = serialPort.ReadLine();
        MainThread.BeginInvokeOnMainThread(MyMainThreadCode);
       // lblBug.Text = "You made it!";
    }

    private void MyMainThreadCode()
    {
        //int calChkSum = 0;
        //if (newPacket.Length > 37)
        //{

        //    if (newPacket.Substring(0, 3) == "###")
        //    {
        //        newPacketNumber = Convert.ToInt32(newPacket.Substring(3, 3));

        //        if (oldPacketNumber > -1)
        //        {
        //            if (newPacketNumber < oldPacketNumber)
        //            {
        //                packetRollover++;
        //                if (oldPacketNumber != 999)
        //                {
        //                    lostPacketCount += 999 - oldPacketNumber + newPacketNumber;
        //                }
        //            }
        //            else
        //            {
        //                if (newPacketNumber != oldPacketNumber + 1)
        //                {
        //                    lostPacketCount += newPacketNumber - oldPacketNumber;
        //                }
        //            }
        //        }
        //        for (int i = 3; i < 34; i++)
        //        {
        //            calChkSum += (byte)newPacket[i];
        //        }
        //        calChkSum %= 1000;
        //        int recChkSum = Convert.ToInt32(newPacket.Substring(34, 3));
        //        if (recChkSum == calChkSum)
        //        {
        //            Temperature(newPacket);
        //            HighSensor(lmClass.avgAnalogValue(newPacket, 0));
        //            oldPacketNumber = newPacketNumber;
        //        }
        //        else
        //        {
        //            chkSumError++;
        //        }

        //        string parsedData = $"{newPacket.Length,-14}" +
        //                           $"{newPacket.Substring(0, 3),-14}" +
        //                           $"{newPacket.Substring(3, 3),-14}" +
        //                           $"{newPacket.Substring(6, 4),-14}" +
        //                           $"{newPacket.Substring(10, 4),-14}" +
        //                           $"{newPacket.Substring(14, 4),-14}" +
        //                           $"{newPacket.Substring(18, 4),-14}" +
        //                           $"{newPacket.Substring(22, 4),-14}" +
        //                           $"{newPacket.Substring(26, 4),-14}" +
        //                           $"{newPacket.Substring(30, 4),-14}" +
        //                           $"{newPacket.Substring(34, 3),-17}" +
        //                           $"{calChkSum,-19}" +
        //                           $"{lostPacketCount,-11}" +
        //                           $"{chkSumError,-14}" +
        //                           $"{packetRollover,-14}\r\n";

        //    }

        //}
    }

    private void Temperature(string validPacket)
    {
        double temperature = lmClass.GetTemperature(lmClass.avgAnalogValue(validPacket, 0));
        labelTemperature.Text = temperature.ToString("  00.0") + " °C";
    }

    public void HighSensor(double voltage)
    {
        double temperature = (voltage / 10);
        if (temperature > 25)
        {
            stringBuilderSend[3] = '0';
            labelWarning.Text = "  To Hot!";
            imgLED1.Source = "ledon.png";
        }
        else if (temperature < 24.7)
        {
            stringBuilderSend[3] = '1';
            labelWarning.Text = "  Okay";
            imgLED1.Source = "ledoff.png";
        }
        sendPacket();
    }


    private void serialOpenClosed()
    {
        if (!bPortOpen && buttonPressed)
        {
            serialPort.PortName = SelectedItem.ToString();
            serialPort.Open();
            LblOpenClose = "Close";
            bPortOpen = true;
            LblBug = "Open";
        }
        else
        {
            serialPort.Close();
            LblOpenClose = "Open";
            bPortOpen = false;
            LblBug = "Close";
        }
    }

    private async void btnSend_Clicked(object sender, EventArgs e)
    {
        try
        {
            string messageOut = entrySend.Text;
            messageOut += "\r\n";
            byte[] messageBytes = Encoding.UTF8.GetBytes(messageOut);
            serialPort.Write(messageBytes, 0, messageBytes.Length);
        }
        catch (Exception ex)
        {
            DisplayAlert("Alert", ex.Message, "OK");
        }

    }
    private void sendPacket()
    {
        int calSendChkSum = 0;
        try
        {
            for (int i = 3; i < 7; i++)
            {
                calSendChkSum += (byte)stringBuilderSend[i];
            }
            calSendChkSum %= 1000;
            stringBuilderSend.Remove(7, 3);
            stringBuilderSend.Insert(7, calSendChkSum.ToString());
            string messageOut = stringBuilderSend.ToString();
            entrySend.Text = stringBuilderSend.ToString();
            messageOut += "\r\n";
            byte[] messageBytes = Encoding.UTF8.GetBytes(messageOut);
            serialPort.Write(messageBytes, 0, messageBytes.Length);
        }
        catch (Exception ex)
        {
            DisplayAlert("Alert", ex.Message, "OK");
        }

    }
}

