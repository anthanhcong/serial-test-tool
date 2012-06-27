using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WindowsFormsApplication1
{
    partial class Test_Form
    {
        private void Load_Snif_Tab(object sender, SerialDataReceivedEventArgs e)
        {
            //SnifPort_Name.DataSource = Enum.GetValues(typeof(BarcodeType));
            //SnifPort_Name.DataSource = SerialPort.GetPortNames();
 
        }

        private void Snif_receiveData(object sender, SerialDataReceivedEventArgs e)
        {
            int len, i;
            SerialPort thisCom = (SerialPort)sender;
            byte[] rxBuffer = new byte[BUF_LEN];

            len = thisCom.BytesToRead >= BUF_LEN ? BUF_LEN : thisCom.BytesToRead;
            thisCom.Read(rxBuffer, 0, len);       // Read Data from COMPORT

            for (i = 0; i < len; i++)
            {
                PKB_rxCharEvent(rxBuffer[i]);
            }
        }
    }
}
