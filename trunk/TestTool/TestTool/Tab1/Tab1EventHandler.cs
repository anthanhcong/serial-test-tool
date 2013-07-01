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
        #region Public Enumerations
        private int count_data;
        private int Right_num;
        private int Wrong_num;
        private int NotRead_num;
        private int ReadSpeed;

        private string TAB1_RECEIVE_BUFFER;
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

            TAB1_RECEIVE_BUFFER += InData;
            if ((TAB1_RECEIVE_BUFFER != "") && (TAB1_RECEIVE_BUFFER.Length >= 2))
            {
                if ((TAB1_RECEIVE_BUFFER.Substring(TAB1_RECEIVE_BUFFER.Length - 2, 2) == "\r\n") || (TAB1_RECEIVE_BUFFER.Substring(TAB1_RECEIVE_BUFFER.Length - 1, 1) == "\r"))
                {
                    // Change to Hex string for check correct data
                    OutData = FormatData(TAB1_RECEIVE_BUFFER, DataType.Receive, tab1_curr_receive, TabNum.Tab1, false);
                    Tab1DataReceiveLine.Invoke(new EventHandler(delegate
                    {
                        // Enable timer for check can not read
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
                        Tab1DataReceiveLine.Text = OutData.Trim();
                        count_data++;
                        // if (Tab1Data4Check.Text == OutData.Trim())
                        if (Is_new_Item(OutData.Trim(), Tab1_Expect_Data_List) == false)
                        {
                            Right_num++;
                            
                            // Tab1NumCorrect.Text = Right_num.ToString();
                            type = LogMsgType.Incoming;
                        }
                        else
                        {
                            Wrong_num++;
                            // Tab1NumWrong.Text = Wrong_num.ToString();
                            type = LogMsgType.Error;
                        }
                        Update_Statistic();
                    }));

                    // Add to logs
                    OutData = FormatData(TAB1_RECEIVE_BUFFER, DataType.Receive, tab1_curr_receive, TabNum.Tab1, true);
                    if (type == LogMsgType.Error)
                    {
                        Add_logs("Miss read\n", LogMsgType.Error, TabNum.Tab1);
                    }
                    // OutData += "\n";
                    Add_logs(OutData, type, TabNum.Tab1);
                    TAB1_RECEIVE_BUFFER = "";
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
                        promptMes = "Runnning in KDB Interface";
                        Display_prompt(promptMes, LogMsgType.Normal);
                    }
                    // Stop Click
                    else
                    {
                        // Change status of "Run" button
                        Tab1RunBT.Text = "Run";
                        Tab1_Enable_setting(true);
                        promptMes = "Ready for Test";
                        Display_prompt(promptMes, LogMsgType.Normal);
                    }
                    break;
                default:
                    break;
            }
            Right_num = 0;
            Wrong_num = 0;
            NotRead_num = 0;
            count_data = 0;
            Update_Statistic();
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


        private void Tab1_StatisticBT_Click(object sender, EventArgs e)
        {
            string statistic_mess;
            DateTime currTime = DateTime.Now;
            string TimeStamp = currTime.ToString("HH:mm:ss.fff");

            statistic_mess = "Report Statistic at: " + TimeStamp + "\n";
            statistic_mess += "Good Read  : " + Right_num.ToString() + "\n";
            statistic_mess += "Miss Read  : " + Wrong_num.ToString() + "\n";
            statistic_mess += "Not Read   : " + NotRead_num.ToString() + "\n";
            statistic_mess += "Total Read : " + count_data.ToString() + "\n";

            Add_logs(statistic_mess, LogMsgType.Normal, TabNum.Tab1);

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

            Add_logs("Not read\n", LogMsgType.Error, TabNum.Tab1);
            NotRead_num ++;
            count_data++;

            Update_Statistic();
        }
    }
}