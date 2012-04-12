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
        public int count_data;
        public int right_num;
        public int wrong_num;
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
            string InData = Tab1serialPort.ReadExisting();
            string OutData;

            // Update Time Check
            tab1_last_receive = tab1_curr_receive;
            tab1_curr_receive = DateTime.Now;
            if (tab1_first_receive == tab1_start)
            {
                tab1_last_receive = tab1_curr_receive;
                tab1_first_receive = tab1_curr_receive;
            }

            // Change to Hex string
            OutData = FormatData(InData, DataType.Receive, tab1_curr_receive, TabNum.Tab1, Tab1_wait_receive);

            if ((InData != "") && (InData.Length >= 2))
            {
                if (InData.Substring(InData.Length-2, 2) != "\r\n")
                {
                    Tab1_wait_receive = false;
                }
                else
                {
                    Tab1_wait_receive = true;
                    OutData += "\n";
                }
            }

            // Write data to an object of an other thread
            Tab1DataReceiveLine.Invoke(new EventHandler(delegate
            {
                Tab1DataReceiveLine.SelectedText = string.Empty;
                Tab1DataReceiveLine.Text = InData;
                count_data++;
                if (Tab1Data4Check.Text == InData)
                {
                    right_num++;
                    Tab1NumCorrect.Text = right_num.ToString();
                }
                else
                {
                    wrong_num++;
                    Tab1NumWrong.Text = wrong_num.ToString();
                }
            }));

            // Add to logs
            Add_logs(OutData, LogMsgType.Incoming, TabNum.Tab1);
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
            Tab1ComPortSelect.SelectedIndex = 0;
            Tab1Data4Check.Text = "";
            Tab1NumCorrect.Text = "";
            Tab1NumWrong.Text = "";
            Tab1SetBaudrate.SelectedIndex = 0;
            Tab1SetDatabit.SelectedIndex = 0;
            Tab1SetParity.SelectedIndex = 0;
            Tab1SetStopbit.SelectedIndex = 0;
            Tab1SetThreshold.Text = "1";
            Tab1DataReceive.Text = "";
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

            
            right_num = 0;
            wrong_num = 0;
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
            if (Tab1InfStatus == Tab1Interface.Ser)
            {
                // Send data
                Tab1serialPort.Write(Tab1SendData.Text);

                // Report Time Check (For last Transaction)
                tab1_last_send = tab1_curr_send;
                tab1_curr_send = DateTime.Now;
                if (tab1_first_send == tab1_start) 
                {
                    tab1_first_send = tab1_curr_send;
                }

                // Add to log
                Logmes = FormatData(Tab1SendData.Text, DataType.Send, tab1_curr_send, TabNum.Tab1, false);
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
    }
}