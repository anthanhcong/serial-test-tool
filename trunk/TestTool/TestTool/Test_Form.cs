using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Test_Form : Form
    {
        #region Public Enumerations
        public enum DataMode { Text, Hex };
        public enum Tab1ReportMode { RunTime, Transaction, FirstRSP, Receive };
        public enum LogMsgType { Incoming, Outgoing, Normal, Warning, Error, Coment };
        public enum Tab1Interface { Key, Ser };
        public enum Tab2Stauts { Init, Run, Pause, Stop };
        public enum DataType {Receive, Send };
        public enum TabNum { Tab1, Tab2, Tab3, Tab4, Tab5 };
        #endregion

        #region 
        public Tab1ReportMode Tab1CurrMode;
        public DataMode Tab1DataViewMode;
        public DataMode Tab2DataViewMode;
        public Tab2ComPort[] ComControlArray;
        public RichTextBox currentBox;
        public Random ran = new Random();
        #endregion

        public Test_Form()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string promtMess;

            // Check all COMPORT
            if (ValidateCOMPORT() == false)
            {
                promtMess = "Error: No COMPORT Available \n";
                Display_prompt(promtMess, LogMsgType.Error);
            }

            // Init Value
            AppInit();

        }


        private void AppInit()
        {
            Tab1_Init();
            Tab2Init();
            Tab3_Init();
            Update_Status_bar(1, false);
        }

        
        private bool ValidateCOMPORT()
        {
            byte index;
            string promptMess;

            Tab1SendBT.Enabled = false;
            // Declare Tab2 Componet
            ComControlArray = new Tab2ComPort[16];
            Delay_index = new int[16];
            Data_index = new int[16];

            Tab1ComPortSelect.Items.Clear();
            index = totalPort = 0;
            foreach (string portName in System.IO.Ports.SerialPort.GetPortNames())
            {
                try
                {
                    Tab1serialPort.PortName = portName;
                    Tab1serialPort.Open();

                    // Init for Tab1 & Tab3
                    Tab1ComPortSelect.Items.Add(portName);
                    Tab3_Set_Port.Items.Add(portName);
                    SnifPort_Name.Items.Add(portName);

                    // Init for Tab2
                    Tab2ComPortInit(ComControlArray, index, portName);
                    totalPort++;
                    index++;
                    Tab1serialPort.Close();
                }
                catch {
                    Add_logs(portName + ": Not available \n", LogMsgType.Error, TabNum.Tab1);
                }
            }
            Tab1ComPortSelect.SelectedIndex = 0;
            if (totalPort != 0)
                return true;
            else
            {
                promptMess = "Error: Do not have any Comport on system";
                Display_prompt(promptMess, LogMsgType.Error);
                return false;
            }
        }

        private bool App_close()
        {
            Tab2Stop_Click(null, null);
            return true;
        }

        private void Test_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            App_close();
        }

        private bool Update_Status_bar(int tabnum, bool run)
        {
            string baudrate;
            string databit;
            string stopbit;
            string parity;

            baudrate = Tab1serialPort.BaudRate.ToString();
            databit = Tab1serialPort.DataBits.ToString();
            stopbit = Tab1serialPort.StopBits.ToString();
            parity = Tab1serialPort.Parity.ToString();

            PortSelectStatus.Text = Tab1serialPort.PortName;
            ConfigStatus.Text = baudrate + "," +
                databit + "," +
                stopbit + "," +
                parity;
            ProgressBar.Visible = false;
            TotalPort.Text = "Total Port: " + totalPort.ToString();
            TotalConnet.Text = "Total Connect:  0";
            return true;
        }

        private void tab1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("..\\..\\..\\data.txt");
        }

        private void tab2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"C:\vangogh\build_all.bat");
        }

        private void abboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string log_mess;

            log_mess = "Serial Test Tool\n";
            log_mess += "Author           : Kien Nguyen\n";
            log_mess += "Release Version  : 0.0.0.5\n";
            log_mess += "Release Date     : Feb 08 2012\n";
            MessageBox.Show(log_mess, "About");
        }
    }
}
