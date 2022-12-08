using CommunityToolkit.Mvvm.ComponentModel;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using Windows.Devices.Enumeration;

namespace LM35tempAndClock.View;

[ObservableObject]
public partial class DebugData
{ 
    private bool bPortOpen = false;
    private string newPacket = "";
    private int oldPacketNumber = -1;
    private int newPacketNumber = 0;
    private int lostPacketCount = 0;
    private int packetRollover = 0;
    private int chkSumError = 0;

    [ObservableProperty]
    string[] itemsSource;
    [ObservableProperty]
    int selectedIndex;
    [ObservableProperty]
    object selectedItem;
    [ObservableProperty]
    string lblOpenClose = "Open";

    SerialPort serialPort = new SerialPort();
    StringBuilder stringBuilderSend = new StringBuilder("###1111196");

    LMclass lmClass = new LMclass();
    public DebugData()
	{
		InitializeComponent();

        string[] ports = SerialPort.GetPortNames();
        ItemsSource = ports;
        SelectedIndex = ports.Length;
        Loaded += MainPage_Loaded;
    }

    private void MainPage_Loaded(object sender, EventArgs e)
    {
        serialPort.BaudRate = 115200;
        serialPort.ReceivedBytesThreshold = 1;
        serialPort.DataReceived += SerialPort_DataReceived;

    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        newPacket = serialPort.ReadLine();
        MainThread.BeginInvokeOnMainThread(MyMainThreadCode);
    }

    private void MyMainThreadCode()
    {
        if (checkBoxHistory.IsChecked == true)
        {
            labelRXdata.Text = newPacket + labelRXdata.Text;
        }
        else
        {
            labelRXdata.Text = newPacket;
        }

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

                if (checkBoxParseHistory.IsChecked == true)
                {
                    labelParsedData.Text = parsedData + labelParsedData.Text;
                }
                else
                {
                    labelParsedData.Text = parsedData;
                }
            }

        }
    }

    private void btnOpenClose_Clicked(object sender, EventArgs e)
    {
        if (!bPortOpen)
        {
            serialPort.PortName = portPicker.SelectedItem.ToString();
            serialPort.Open();
            btnOpenClose.Text = "Close";
            bPortOpen = true;
        }
        else
        {
            serialPort.Close();
            btnOpenClose.Text = "Open";
            bPortOpen = false;
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

    private void btnBit1_Clicked(object sender, EventArgs e)
    {
        ButonClicked(1);
    }

    private void btnBit0_Clicked(object sender, EventArgs e)
    {
        ButonClicked(0);
    }

    private void ButonClicked(int i)
    {
        Button[] btnBits = new Button[] { btnBit0, btnBit1 };
        if (btnBits[i].Text == "0")
        {
            btnBits[i].Text = "1";
            stringBuilderSend[i + 3] = '1';
        }
        else
        {
            btnBits[i].Text = "0";
            stringBuilderSend[i + 3] = '0';
        }
        sendPacket();
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

