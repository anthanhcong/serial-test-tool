using System;
using System.Linq;
using System.Data;
using System.Text;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace WindowsFormsApplication1
{
    partial class Test_Form
    {
        #region Public Enumerations
        private int Total_read;
        private int Right_num;
        private int Wrong_num;
        private int NotRead_num;
        private int ReadSpeed;
        public string Scan_info_Buf;

        private string Tab1_Receive_Buffer;
        #endregion

        /*
         *       ######  ###### #######  #    ##    #         
                ##       #      ##    #  #   ## #   #         
                  #####  #####  #######  #   #  ##  #         
                #     #  #      ##    #  #  ######  #         
                ####### ####### ##    #  # ##    ## ######    
                                                              
                                                              
                                                              
            ######  #######  ######  ###### ## #     ## ######
            #    #  #       #     ## #       #  #    #  #     
            ######  ###### ##        ######  #  ##  #   ######
            #    #  #       #     #  #       #   # ##   #     
            #    #  #######  ######  ######  #    ##    ######
         */
        private void Tab1SerialPort_ISR(object sender, SerialDataReceivedEventArgs e)
        {
            int len = Tab1serialPort.BytesToRead;
            byte[] data_read = new byte[len];
            Tab1serialPort.Read(data_read, 0, len);
            string InData = Convert_Bytes_to_String(data_read, 0, len);
            string OutData;
            LogMsgType type = LogMsgType.Error;

            // Update Time Check
            tab1_last_receive = tab1_curr_receive;
            tab1_curr_receive = DateTime.Now;
            if (tab1_first_receive == tab1_start)
            {
                tab1_last_receive = tab1_curr_receive;
                tab1_first_receive = tab1_curr_receive;
            }

            Tab1_Receive_Buffer += InData;
            if ((Tab1_Receive_Buffer != "") && (Tab1_Receive_Buffer.Length >= 2))
            {
                if ((Tab1_Receive_Buffer.Substring(Tab1_Receive_Buffer.Length - 2, 2) == "\r\n") || (Tab1_Receive_Buffer.Substring(Tab1_Receive_Buffer.Length - 1, 1) == "\r"))
                {
                    //@NOTE (Kien): Get Scan information for statistic report
                    if ((Receive_State == TAB1_STATE.ENTER_SERVICE) || (Receive_State == TAB1_STATE.EXIT_SERVICE))
                    {
                        Scan_info_Buf = "";
                        Tab1_Receive_Buffer = "";
                        return;
                    }
                    else if (Receive_State == TAB1_STATE.SCAN_INFO)
                    {
                        Scan_info_Buf += Tab1_Receive_Buffer;
                        Tab1_Receive_Buffer = "";
                        return;
                    }

                    //@NOTE (Kien): Change to Hex string for check correct data - not add time stamp
                    OutData = FormatData(Tab1_Receive_Buffer, DataType.Receive, tab1_curr_receive, TabNum.Tab1, false);
                    OutData = OutData.Trim();
                    Tab1DataReceiveLine.Invoke(new EventHandler(delegate
                    {
                        // Enable timer for check can not read
                        Reset_NotRead_Timer();
                        Tab1DataReceiveLine.Text = OutData;
                        Total_read++;
                        if (Is_new_Item(OutData, Tab1_Expect_Data_List) == false)
                        {
                            Right_num++;
                            Add_Goodread_statistic(OutData);
                            type = LogMsgType.Incoming;

                            Send_Click(null, null);
                        }
                        else
                        {
                            Wrong_num++;
                            Add_Misread_statistic(OutData);
                            type = LogMsgType.Error;
                        }
                        Update_Statistic();
                    }));

                    //@NOTE (Kien): Add time stamp & add to log
                    OutData = FormatData(Tab1_Receive_Buffer, DataType.Receive, tab1_curr_receive, TabNum.Tab1, true);
                    if (type == LogMsgType.Error)
                    {
                        Add_logs("Misread ", LogMsgType.Error, TabNum.Tab1);
                    }
                    // OutData += "\n";
                    Add_logs(OutData, type, TabNum.Tab1);
                    Tab1_Receive_Buffer = "";
                }
            }
        }

        /***************************************************************************/
        /*  #######   #      #  ##################   ######    ###     #
            #     ##  #      #      #        #      #      ##  ####    #
            #######   #      #     ##        #     ##       #  ## ##   #
            #     ##  #      #     ##        #     ##       #  ##  ##  #
            #     ##  #      #     ##        #     ##      ##  ##   ## #
            #######    ######      ##        ##      #######   ##     ##
         
                                                          
            ######## ##      ## ########  ###    ## ##########          
            #         #     ##  #         # ##    #     ##              
            #######   ##    #   #######   #  ##   #     ##              
            #          ##  ##   ##        ##  ##  #     ##              
            #           # ##    #         ##   ## #     ##              
            ########    ###     ########  ##     ##     ##                         */
        /***************************************************************************/
        /// <summary>
        /// Name        : Tab1ClearBT_Click
        /// Event       : Click on Button "Clear" of Tab1
        /// Function    : Reset logs and 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab1ClearBT_Click(object sender, EventArgs e)
        {
            Tab1Data4Check.Text = "";
            Tab1NumCorrect.Text = "";
            Tab1NumWrong.Text = "";
            Tab1_NotRead.Text = "";
            Tab1_TotalRead.Text = "";
            Tab1_ReadSpeed.Text = "";
            Tab1DataReceive.Text = "";
            Tab1SendData.Text = "";
            Tab1DataReceiveLine.Text = "";
            Tab1_CircleRead.Text = "";

            Right_num = 0;
            Wrong_num = 0;
            NotRead_num = 0;
            Total_read = 0;
            //Misread_table.Rows.Clear();
            //Goodread_table.Rows.Clear();
            Update_Statistic();
        }


        private void Tab1RestoreBT_Click(object sender, EventArgs e)
        {
            Tab1ComPortSelect.SelectedIndex = 0;
            Tab1SetBaudrate.SelectedIndex = 0;
            Tab1SetDatabit.SelectedIndex = 0;
            Tab1SetParity.SelectedIndex = 0;
            Tab1SetStopbit.SelectedIndex = 0;
            Tab1SetThreshold.Text = "1";
        }

        /// <summary>
        /// Name        : Tab1RunBT_Click
        /// Event       : Click on Button "Run" of Tab1
        /// Function    : 
        ///     + Open/Close Comport
        ///     
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab1RunBT_Click(object sender, EventArgs e)
        {
            string promptMes;

            switch (Tab1InfStatus)
            {
                case Tab1Interface.Ser:
                    if (Tab1ComPortSelect.SelectedIndex >= 0)
                    {
                        // Run Click
                        if (Tab1serialPort.IsOpen == false)
                        {
                            // Get Setting & Open Comport
                            GetTab1SerialConfig();
                            OpenComTab1();

                            // Change status of "Run" button
                            Tab1RunBT.Text = "Stop";
                            promptMes = Tab1serialPort.PortName + " is Opened";
                            Display_prompt(promptMes, LogMsgType.Normal);

                            // Reset run time
                            ResetTimeCheck();
                            Update_Status_bar(1, true);
                            Tab1_wait_receive = true;

                            // Enable timer for check can not read
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
                            
                        }
                        // Stop Click
                        else
                        {
                            // Close ComPort
                            CloseComTab1();

                            // Change status of "Run" button
                            Tab1RunBT.Text = "Run";
                            promptMes = Tab1serialPort.PortName + " is Closed";
                            Display_prompt(promptMes, LogMsgType.Normal);

                            // Update Time Check
                            tab1_stop = DateTime.Now;

                            // Report Run Time Check
                            Time_Report();
                            Update_Status_bar(1, false);
                            // Stop timer for check can not read
                            if (Tab1_CheckNotRead.Checked == true)
                            {
                                try
                                {
                                    Tab1_WaitNextLbl_Timer.Stop();
                                }
                                catch
                                {
                                    MessageBox.Show("Can not stop Check Not Read.", "Error");
                                }
                            }
                        }
                    }
                    else
                    {
                        promptMes = "Please select COM Port !";
                        Display_prompt(promptMes, LogMsgType.Error);
                    }
                    
                    break;
                case Tab1Interface.Key:
                    // Run Click
                    if (Tab1RunBT.Text == "Run")
                    {
                        // Change status of "Run" button
                        Tab1RunBT.Text = "Stop";
                        Tab1_Enable_setting(false);
                        
                        

                        // Reset run time
                        ResetTimeCheck();

                        // Enable timer for check can not read
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
                        Update_Status_bar(1, false);
                    }
                    // Stop Click
                    else
                    {
                        // Change status of "Run" button
                        Tab1RunBT.Text = "Run";
                        Tab1_Enable_setting(true);
                        promptMes = "Ready for Test";
                        Display_prompt(promptMes, LogMsgType.Normal);

                        // Update Time Check
                        tab1_stop = DateTime.Now;

                        // Report Run Time Check
                        Time_Report();
                        Update_Status_bar(1, false);
                        // Stop timer for check can not read
                        if (Tab1_CheckNotRead.Checked == true)
                        {
                            try
                            {
                                Tab1_WaitNextLbl_Timer.Stop();
                            }
                            catch
                            {
                                MessageBox.Show("Can not stop Check Not Read.", "Error");
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Name: Tab1SendBT_Click
        /// Event: Click on "Send" button
        /// Function: Send data through Comport
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab1SendBT_Click(object sender, EventArgs e)
        {
            string Logmes;
            string senddata;
            byte[] data_converted;
            int len;
            

            if (Tab1InfStatus == Tab1Interface.Ser)
            {
                senddata = Tab1SendData.Text;
                len = senddata.Length;
                data_converted = new byte[len + 5];
                len = Change_Text2Bytes(senddata, ref data_converted);

                // Send data
                //Tab1serialPort.(Tab1SendData.Text);
                Tab1serialPort.Write(data_converted, 0, len);

                // Report Time Check (For last Transaction)
                tab1_last_send = tab1_curr_send;
                tab1_curr_send = DateTime.Now;
                if (tab1_first_send == tab1_start) 
                {
                    tab1_first_send = tab1_curr_send;
                }

                // Add to log

                senddata = Change_HexString2String(senddata);
                Logmes = FormatData(senddata, DataType.Send, tab1_curr_send, TabNum.Tab1, false);
                Tab1_wait_receive = true;
                Add_logs(Logmes, LogMsgType.Outgoing, TabNum.Tab1);
            }
            else
            {
                Display_prompt("Can not write data in KDB mode", LogMsgType.Error);
            }
        }

        /// <summary>
        /// Name: Tab1LogBT_Click
        /// Function: Save log data into file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab1LogBT_Click(object sender, EventArgs e)
        {
            string promptMes;
            string fileName;
            saveFileDialog1.Filter = "RichTextFile |*.rtf|Text file (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save Log Files To";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fileName = saveFileDialog1.FileName;
                promptMes = "Saved to: " + fileName;
                Display_prompt(promptMes, LogMsgType.Normal);
                if (saveFileDialog1.FilterIndex == 2)
                {
                    Tab1DataReceive.SaveFile(fileName, RichTextBoxStreamType.UnicodePlainText);
                }
                else
                {
                    Tab1DataReceive.SaveFile(fileName, RichTextBoxStreamType.RichText);
                }
            }
        }

        private void Tab1_Load_DataCheck_BT_Click(object sender, EventArgs e)
        {
            string file_name;
            OpenFileDialog dialog = new OpenFileDialog();
            System.IO.StreamReader readfile;
            string line;
            dialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                file_name = dialog.FileName;
                readfile = new System.IO.StreamReader(file_name);
                while ((line = readfile.ReadLine()) != null)
                {
                    if (line != "") line = line.Trim();
                    if (line != "")
                    {
                        Tab1_Expect_Data_List.Items.Add(line);
                    }
                }
                readfile.Close();
            }
        }

        private void Tab1_StatisticBT_Click(object sender, EventArgs e)
        {
            string statistic_mess;
            DateTime currTime = DateTime.Now;
            TimeSpan duration = currTime - tab1_start;
            string TimeStamp = currTime.ToString("HH:mm:ss.fff");
            bool last_com_isOpen = true;
            int last_baud;
           

            statistic_mess =  "|==================================================|\n";
            statistic_mess += "  Report Statistic at: \t" + TimeStamp + "\n";
            statistic_mess += "  Run Time   \t" + duration.ToString() + "\n";
            statistic_mess += "  Good read  \t" + Right_num.ToString() + "\n";
            statistic_mess += "  Misread    \t" + Wrong_num.ToString() + "\n";
            statistic_mess += "  Not read   \t" + NotRead_num.ToString() + "\n";
            statistic_mess += "  Total Read \t" + Total_read.ToString() + "\n";
            statistic_mess += "|--------------------------------------------------|\n";

            // Add Good Read label
            statistic_mess += "  Good read Label \n";
            foreach (DataRow row in Goodread_table.Rows)
            {
                statistic_mess += "     " + row["Data"] + ":\t" + row["Cnt"] + "\n";
            }
            statistic_mess += "|--------------------------------------------------|\n";

            // Add Misread Label 
            statistic_mess += "  Misread Label: \n";
            foreach (DataRow row in Misread_table.Rows)
            {
                statistic_mess += "     " + row["Data"] + ": \t" + row["Cnt"] + "\n";
            }
            statistic_mess += "|==================================================|\n";

            Add_logs(statistic_mess, LogMsgType.Normal, TabNum.Tab1);

            //@NOTE (Kien): get Statistic in Scanner:

            if (Tab1InfStatus != Tab1Interface.Ser)
            {
                return;
            }
            last_baud = Tab1serialPort.BaudRate;
            last_com_isOpen = Tab1serialPort.IsOpen;
            if (Tab1serialPort.IsOpen == false)
            {
                Tab1serialPort.Open();
            }
            

            Receive_State = TAB1_STATE.ENTER_SERVICE;
            Tab1serialPort.Write("$S\r");
            Thread.Sleep(500);
            Application.DoEvents();

            Tab1serialPort.Close();
            Tab1serialPort.BaudRate = 115200;
            Tab1serialPort.Open();
            Receive_State = TAB1_STATE.SCAN_INFO;
            Tab1serialPort.Write("$!\r");
            Thread.Sleep(300);
            Application.DoEvents();
            Tab1serialPort.Write("$L00,L01,L02,L03,L04,L05,L06,L07,L08,L09\r");
            Thread.Sleep(300);
            Application.DoEvents();
            Tab1serialPort.Write("$t00,t01,t02,t03\r");
            Thread.Sleep(300);
            Application.DoEvents();
            Receive_State = TAB1_STATE.EXIT_SERVICE;

            statistic_mess = "Scanner information: \n" + Scan_info_Buf;
            Add_logs(statistic_mess, LogMsgType.Normal, TabNum.Tab1);
            Tab1serialPort.Write("$s\r");
            Thread.Sleep(300);
            Application.DoEvents();
            Receive_State = TAB1_STATE.NORMAL;

            Tab1serialPort.Close();
            Tab1serialPort.BaudRate = last_baud;
            if (last_com_isOpen == true)
            {
                Tab1serialPort.Open();
            }

        }

        private void Send_Click(object sender, EventArgs e)
        {
            int num_bell = Convert.ToInt32(Tab1_BellNum.Text.ToString().Trim());
            int delay_bell = Convert.ToInt32(Tab1_BellDelay.Text.ToString().Trim());
            int i;
            byte[] data_converted = new byte[1];
            if (Tab1serialPort.IsOpen == true)
            {
                for (i = 0; i < num_bell; i++)
                {
                    if (BellBeep_Test1_Ena.Checked == true)
                    {
                        data_converted[0] = 1;
                    }
                    else if (BellBeep_TestB_Ena.Checked == true)
                    {
                        data_converted[0] = 0x42;   // 'B'
                    }
                    Tab1serialPort.Write(data_converted, 0, 1);
                    Thread.Sleep(delay_bell);
                }
            }
        }

        /*******************************************************************
         *         ########     ###     #######    #    #######         
                   ##     ##    # ##    ##     ##  #   ##     ##        
                   ## #####    ##  ##   ##      #  #  ##       ##       
                   ## #####   ##    #   ##      #  #  ##       ##       
                   ##     #   ########  ##     ##  #   ##     ##        
                   ##     ## ##      ## #######    ##   #######         
                                                                        
                                                                        
                                                                        
                                                                        
            #######   #      # ###################   #######   ###     #
            #     ##  #      #      #        #      #      ##  ####    #
            #######   #      #     ##        #     ##       #  ## ##   #
            #     ##  #      #     ##        #     #        #  ##  ##  #
            #     ##  #      #     ##        #     ##      ##  ##   ## #
            #######    ######      ##        #       ######    ##     ##
         *******************************************************************/
        /// <summary>
        /// Name: Tab1KeyOption_CheckedChanged
        /// Function: set tab1 interface
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab1KeyOption_CheckedChanged(object sender, EventArgs e)
        {
            if (Tab1SerOption.Checked == true)
            {
                Tab1InfStatus = Tab1Interface.Ser;
            }
            else
            {
                Tab1InfStatus = Tab1Interface.Key;
            }
        }

        private void Tab1HexView_CheckedChanged(object sender, EventArgs e)
        {
            if (Tab1HexView.Checked == true)
                Tab1DataViewMode = DataMode.Hex;
            else
                Tab1DataViewMode = DataMode.Text;
        }

        private void Tab1ReportMode_CheckedChanged(object sender, EventArgs e)
        {
            if (Tab1ReportRun.Checked == true) Tab1CurrMode = Tab1ReportMode.RunTime;
            else if (Tab1ReportFirstRsp.Checked == true) Tab1CurrMode = Tab1ReportMode.FirstRSP;
            else if (Tab1ReportTransaction.Checked == true) Tab1CurrMode = Tab1ReportMode.Transaction;
            else Tab1CurrMode = Tab1ReportMode.Receive;
        }

        private void Tab1_Add_BT_Click(object sender, EventArgs e)
        {
            string item = Tab1Data4Check.Text.Trim();
            if (Is_new_Item(item, Tab1_Expect_Data_List) == true)
            {
                Tab1_Expect_Data_List.Items.Add(item);
            }
        }

        private void Tab1_Remove_BT_Click(object sender, EventArgs e)
        {
            if (Tab1_Expect_Data_List.Items.Count != 0)
            {
                if (Tab1_Expect_Data_List.SelectedIndex != -1)
                {
                    Tab1_Expect_Data_List.Items.RemoveAt(Tab1_Expect_Data_List.SelectedIndex);
                }
            }
        }


        private void Tab1_WaitNextLbl_Timer_Tick(object sender, EventArgs e)
        {
            // Enable timer for check can not read
            Reset_NotRead_Timer();

            Add_logs("Not read\n", LogMsgType.Error, TabNum.Tab1);
            NotRead_num ++;
            Total_read++;

            Update_Statistic();
            if (Tab1InfStatus == Tab1Interface.Key)
            {
                Tab1DataReceiveLine.Focus();
            }
        }
    }
}