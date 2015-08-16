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
    
    partial class Test_Form
    {
        public enum TAB1_STATE
        {
            NORMAL,
            ENTER_SERVICE,
            SCAN_INFO,
            EXIT_SERVICE,
        }

        private Tab1Interface Tab1InfStatus = Tab1Interface.Ser;

        #region Public Tab1_Time_check
        public DateTime tab1_start;
        public DateTime tab1_first_send;
        public DateTime tab1_last_send;
        public DateTime tab1_curr_send;
        public DateTime tab1_first_receive;
        public DateTime tab1_last_receive;
        public DateTime tab1_curr_receive;
        public DateTime tab1_stop;
        public bool Tab1_wait_receive;
        public TAB1_STATE Receive_State = TAB1_STATE.NORMAL;
        

        DataTable Misread_table;
        DataTable Goodread_table;
        #endregion

        /***************************************************************
            ##   ####        ##    ##  #############
            ##   #####       ##    ##        ##     
            ##   ##  ##      ##    ##        ##     
            ##   ###  ##     ##    ##        ##     
            ##   ###   ##    ##    ##        ##     
            ##   ###    ###  ##    ##        ##     
            ##   ###     ### ##    ##        ##     
            ##   ###      #####    ##        ##     
            ##   ###        ###    ##        ##                      */
        /*************************************************************/
        private bool Tab1_Init()
        {
            //@Note (Kien): Init Misread Table
            Misread_table = new DataTable();
            Misread_table.Columns.Add("Data");
            Misread_table.Columns.Add("Cnt");

            //@Note (Kien): Init Good read Table
            Goodread_table = new DataTable();
            Goodread_table.Columns.Add("Data");
            Goodread_table.Columns.Add("Cnt");

            Receive_State = TAB1_STATE.NORMAL;
            
            Tab1SerOption.Checked = true;
            Tab1TextView.Checked = true;
            Tab1ReportRun.Checked = true;
            Tab1_wait_receive = true;
            Tab1_Receive_Buffer = "";

            return true;
        }

        /***************************************************************************/
        /*   #####   #####  ##   ##    ######  #####  # # # ######
            #       #     # ###  ##    #    # #     # #   #    #  
            #       #     # # # # #    #####  #     # # # #    #  
             #####   #####  #  ## #    #       #####  #   #    #                   */
        /***************************************************************************/
        /// <summary>
        /// Name            : GetTab1SerialConfig
        /// Function        : Save all config of Comport
        /// </summary>
        private void GetTab1SerialConfig()
        {
            Tab1serialPort.PortName = Tab1ComPortSelect.Text;
            Tab1serialPort.BaudRate = int.Parse(Tab1SetBaudrate.Text);
            Tab1serialPort.DataBits = int.Parse(Tab1SetDatabit.Text);
            Tab1serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), Tab1SetParity.Text);
            Tab1serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), Tab1SetStopbit.Text);
            Tab1serialPort.ReceivedBytesThreshold = int.Parse(Tab1SetThreshold.Text);
        }

        /// <summary>
        /// Name: OpenComTab1
        /// Function: 
        ///     + Open Comport of Tab1
        ///     + Disable Setting
        /// </summary>
        private bool OpenComTab1()
        {
            try
            {
                Tab1serialPort.Open();
            }
            catch 
            {
                MessageBox.Show(("Can not Open " + Tab1serialPort.PortName), "Error");
                return false;
            }

            // update status
            if (Tab1serialPort.IsOpen == true)
            {
                Tab1_Enable_setting(false);

                // Enable send button
                Tab1SendBT.Enabled = true;
            }
            return true;
        }

        /// <summary>
        /// Name: CloseComTab1
        /// Function: 
        ///     + Close Comport of Tab1
        ///     + Enable Setting
        /// </summary>
        private void CloseComTab1()
        {
            try
            {
                Tab1serialPort.Close();
            }
            catch
            {
                MessageBox.Show(("Can not Close " + Tab1serialPort.PortName), "Error");
            }

            // update status
            if (Tab1serialPort.IsOpen == false)
            {
                // Enable All Setting
                Tab1_Enable_setting(true);

                // Disable send button
                Tab1SendBT.Enabled = false;
            }
        }


        /***********************************************************
         *  ######  #######  ######   ######   ######  #######
            #    ## ##       #    ## #      #  #    ##    #   
            ######  #######  ######  #      #  ######     #   
            #    ## ##       #       #      #  #    ##    #   
            #    ## #######  #        ######   #    ##    #   
         ***********************************************************/
        /// <summary>
        /// Name: Time_Report
        /// Function: 
        /// </summary>
        private void Time_Report()
        {
            string report_mes;
            TimeSpan duration;

            switch (Tab1CurrMode)
            {
                case Tab1ReportMode.RunTime:
                    // Stop at
                    report_mes = "Finish at: " + tab1_stop.ToString("yyyy/mm/dd hh:mm:ss") + " \n";
                    Add_logs(report_mes, LogMsgType.Normal,TabNum.Tab1);

                    // Run Time
                    duration = tab1_stop - tab1_start;
                    report_mes = "Stop - Start = " + duration.ToString() + "s \n \n";
                    Add_logs(report_mes, LogMsgType.Normal, TabNum.Tab1);
                    break;

                case Tab1ReportMode.Receive:
                    duration = tab1_curr_receive - tab1_first_receive;
                    report_mes = "Current Receive - FirstReceive = " + duration.ToString() + " s \n";
                    Add_logs(report_mes, LogMsgType.Normal, TabNum.Tab1);
                    break;

                case Tab1ReportMode.FirstRSP:
                    duration = tab1_curr_receive - tab1_curr_send;
                    report_mes = "Current Receive - Current Send = " + duration.TotalSeconds + " s \n";
                    Add_logs(report_mes, LogMsgType.Normal, TabNum.Tab1);
                    break;
                case Tab1ReportMode.Transaction:
                    duration = tab1_curr_receive - tab1_last_send;
                    report_mes = "LastReceive - CurrSendTime = " + duration.TotalSeconds + " s \n";
                    
                    // Add to logs
                    Add_logs(report_mes, LogMsgType.Normal, TabNum.Tab1);
                    break;
                default:
                    break;
            }
            // ResetTimer(2);  // reset transaction timer
        }


        /*************************************************
         *        ####### ## ##    ##  ######       
                     #    ## ###   ##  #            
                     #    ## # #  # #  ######       
                     #    ## #  # # #  #            
                     #    ## ## ##  #  ######       
                                                    
                                                    
                                                    
             ######  #    ## #######  ###### ##  ## 
            ##    ## #    #  #       #     # #  ##  
            #        ######  ######  #       ###    
            #     ## #    #  #       #     # #  ##  
             ######  #    ## #######  ###### #    ##
         ************************************************/
        /// <summary>
        /// Name: 
        /// </summary>
        /// <param name="mode"></param>1: transaction, 0: Run Time
        private void ResetTimeCheck()
        {
            tab1_start = tab1_stop
                = tab1_first_receive
                = tab1_first_send
                = tab1_curr_receive
                = tab1_curr_send
                = tab1_last_receive
                = tab1_last_send = DateTime.Now;
        }

        /*************************************************
         *      ###     #######   #######  
               # ##    #     ##  ##     ##
              ##  #    #      ## ##      #
             ##   ##   #      ## ##      #
             ########  #      #  ##     ##
            ##      #  #######   ######## 
                                          
                            
             #        #######     ####### 
             #       ##     ##   #      ##
             #       #       ## ##        
             #       #       ## ##   #####
             #       ##      #   #      ##
             #######   ######     ########
         **************************************************/
        /// <summary>
        /// Funtion to write data to an object not in current thread
        /// </summary>
        /// <param name="text"></param>
        private void Add_logs(String text, LogMsgType type, TabNum tabNum)
        {
            switch (tabNum)
            {
                case TabNum.Tab1:
                    currentBox = Tab1DataReceive;
                    break;
                case TabNum.Tab2:
                    currentBox = Tab2ReceiveData;
                    break;
                case TabNum.Tab3:
                    currentBox = Tab3_richText;
                    break;
                default:
                    break;
            }

            if (currentBox.InvokeRequired)
            {
                currentBox.BeginInvoke(
                    new MethodInvoker(
                        delegate() { Add_logs(text, type, tabNum); }));
            }
            else
            {
                currentBox.Focus();
                // Set up message
                switch (type)
                {
                    case LogMsgType.Error:
                        currentBox.SelectionColor = Color.Red;
                        break;
                    case LogMsgType.Incoming:
                        currentBox.SelectionColor = Color.Blue;
                        break;
                    case LogMsgType.Normal:
                        currentBox.SelectionColor = Color.Black;
                        break;
                    case LogMsgType.Outgoing:
                        currentBox.SelectionColor = Color.Green;
                        break;
                    case LogMsgType.Warning:
                        currentBox.SelectionColor = Color.Gold;
                        break;
                    case LogMsgType.Coment:
                        currentBox.SelectionColor = Color.DarkOrange;
                        break;
                    default:
                        currentBox.SelectionColor = Color.Black;
                        break;
                }
                currentBox.AppendText(text);
            }
        }

        /// <summary>
        /// Function Display Prompt Message
        /// </summary>
        /// <param name="text"></param>
        private void Display_prompt(String text, LogMsgType type)
        {
            if (Tab1PromptText.InvokeRequired)
            {
                Tab1PromptText.BeginInvoke(
                    new MethodInvoker(
                        delegate() { Display_prompt(text, type); }));
            }
            else
            {
                // Set up message
                switch (type)
                {
                    case LogMsgType.Error:
                        Tab1PromptText.ForeColor = Color.Red;
                        break;
                    case LogMsgType.Incoming:
                        Tab1PromptText.ForeColor = Color.Blue;
                        break;
                    case LogMsgType.Normal:
                        Tab1PromptText.ForeColor = Color.Black;
                        break;
                    case LogMsgType.Outgoing:
                        Tab1PromptText.ForeColor = Color.Green;
                        break;
                    case LogMsgType.Warning:
                        Tab1PromptText.ForeColor = Color.Peru;
                        break;
                    default:
                        Tab1PromptText.ForeColor = Color.Black;
                        break;
                }
                Tab1PromptText.Text = text;
            }
        }

        
        /*****************************************************************
         * 
         *  ##     ##  ########  #######       ###    ######### ########
            ##     ##  ##     #  ##     #     ## #        #     #       
            ##     ##  ##     #  ##      #    #  ##       #     ####### 
            ##     ##  ########  ##      #   #    ##      #     ## #### 
            ##     ##  ##        ##     ##  ########      #     ##      
             #### ##   ##        ## #####   #      ##    ##     ## #### 
                                                                        
                                                                        
             #######  #########    ###    ######### ##     ##  ######## 
            ##            #       ## ##       #     ##     ##  #        
             ######       #      ##   #       #     ##     ##  #######  
                  ###     #      #    ##      #     ##     ##        ## 
            ##     ##     #     #########     #     ##     ## ##      # 
             #######      #     #       #     #      #######   ######## 
         * 
         *****************************************************************/
        private void Tab1_Enable_setting(bool enable)
        {
            // Enable All Setting
            Tab1ComPortSelect.Enabled = enable;
            Tab1SetBaudrate.Enabled = enable;
            Tab1SetDatabit.Enabled = enable;
            Tab1SetParity.Enabled = enable;
            Tab1SetStopbit.Enabled = enable;
            Tab1SetThreshold.Enabled = enable;
            Tab1KeyOption.Enabled = enable;
            Tab1SerOption.Enabled = enable;
            Tab1ReportRun.Enabled = enable;
            Tab1ReportFirstRsp.Enabled = enable;
            Tab1ReportTransaction.Enabled = enable;
            Tab1ReportReceiving.Enabled = enable;
        }
        private void Update_Statistic()
        {
            Tab1NumCorrect.Text = Right_num.ToString();
            Tab1NumWrong.Text = Wrong_num.ToString();
            Tab1_NotRead.Text = NotRead_num.ToString();
            Tab1_TotalRead.Text = Total_read.ToString();
            Tab1_ReadSpeed.Text = ReadSpeed.ToString();
        }

        private void Tab1DataReceiveLine_KeyDown(object sender, KeyEventArgs e)
        {
            string Text;
            LogMsgType type = LogMsgType.Error;
            Text = Tab1DataReceiveLine.Text + "\n";
            string outData;

            if (e.KeyCode == Keys.Enter)
            {
                // Add_logs(Text, LogMsgType.Incoming, TabNum.Tab1);
                // Tab1DataReceiveLine.Text = "";
                
                Reset_NotRead_Timer();
                // Check for misread
                Total_read++;
                if (Is_new_Item(Text, Tab1_Expect_Data_List) == false)
                {
                    Right_num++;
                    Add_Goodread_statistic(Text);
                    type = LogMsgType.Incoming;
                }
                else
                {
                    Wrong_num++;
                    Add_Misread_statistic(Text);
                    type = LogMsgType.Error;
                }
                Update_Statistic();
                outData = FormatData(Text, DataType.Receive, tab1_curr_receive, TabNum.Tab1, true);
                if (type == LogMsgType.Error)
                {
                    Add_logs("Misread ", LogMsgType.Error, TabNum.Tab1);
                }
                Add_logs(outData, type, TabNum.Tab1);
                Tab1DataReceiveLine.Text = "";
            }
            Tab1DataReceiveLine.Focus();
        }

        private bool Is_new_Item (string text, ListBox list)
        {
            int num_item = list.Items.Count;
            int i;
            if (text == "") return false;
            for (i = 0; i < num_item; i++)
            {
                if (text.Trim() == list.Items[i].ToString().Trim())
                {
                    return false;
                }
            }
            return true;
        }

        private bool Add_Misread_statistic(string data_receive)
        {
            string data_check;
            int cnt;
            bool existed = false;
            DataRow new_row;
            foreach (DataRow row in Misread_table.Rows)
            {
                data_check = row["Data"].ToString();
                cnt = Convert.ToInt32(row["Cnt"]);
                if (data_receive == data_check)
                {
                    cnt++;
                    row["Cnt"] = cnt.ToString();
                    existed = true;
                }
            }

            if (existed == false)
            {
                new_row = Misread_table.NewRow();
                new_row["Data"] = data_receive;
                new_row["Cnt"] = 1;
                Misread_table.Rows.Add(new_row);
            }

            return true;
        }

        private bool Add_Goodread_statistic(string data_receive)
        {
            string data_check;
            int cnt;
            bool existed = false;
            DataRow new_row;
            foreach (DataRow row in Goodread_table.Rows)
            {
                data_check = row["Data"].ToString();
                cnt = Convert.ToInt32(row["Cnt"]);
                if (data_receive == data_check)
                {
                    cnt++;
                    row["Cnt"] = cnt.ToString();
                    existed = true;
                }
            }

            if (existed == false)
            {
                new_row = Goodread_table.NewRow();
                new_row["Data"] = data_receive;
                new_row["Cnt"] = 1;
                Goodread_table.Rows.Add(new_row);
            }

            return true;
        }
        private bool Reset_NotRead_Timer()
        {
            // Re-start timer for check can not read
            Tab1_WaitNextLbl_Timer.Stop();
            if (Tab1_CheckNotRead.Checked == true)
            {
                try
                {
                    Tab1_WaitNextLbl_Timer.Interval = Convert.ToInt32(Tab1_CircleRead.Text.Trim());
                    Tab1_WaitNextLbl_Timer.Enabled = true;
                    Tab1_WaitNextLbl_Timer.Start();
                }
                catch
                {
                    MessageBox.Show("Can not start Check Not Read.", "Error");
                }
            }
            return true;
        }
        
    }
}