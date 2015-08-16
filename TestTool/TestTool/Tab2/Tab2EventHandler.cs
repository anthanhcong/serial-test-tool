using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    partial class Test_Form
    {
        public StreamWriter[] FileOut;
        public string[] Comlist;

        /*******************************************************
         * 
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
         * 
         *********************************************************/
        private void Tab2SerialPort_ISR(object sender, SerialDataReceivedEventArgs e)
        {
            string portName;
            string Indatastring, OutData;
            byte[] indata; // = new byte[BUF_LEN];
            int len;
            int i,index;
            bool end_frame = false;

            SerialPort thisCom = (SerialPort)sender;
            portName = thisCom.PortName.ToString();
           
            // find log file & Comport index
            index = 0;
            for (i = 0; i < totalPort; i++)
            {
                if (Comlist[i] == portName)
                {
                    index = i;
                    break;
                }
            }

            // @TODO (Kien #2#DONE): Add receive data to buffer and check for "Wait Respond"
            if (i < totalPort){
                // Read Data and store to buffer
                len = thisCom.BytesToRead; // >= BUF_LEN ? BUF_LEN : thisCom.BytesToRead;
                if (len >= BUF_LEN) len = BUF_LEN;
                indata = new byte[len+1];
                thisCom.Read(indata, 0, len);       // Read Data from COMPORT

                //@NOTE (Kien##): Implement feature for Support receive data in X_MODEM 1K protocol. 
                if (Tab2_XMODEM[index].Enable == true)
                {
                    Process_XMODEL_Data(index, indata, len);
                    return;
                }

                // Add to buffer
                for (i = 0; i < len; i++)
                {
                    Tab2_Receive_Buf[index][Tab2_Receive_index[index]] = indata[i];
                    Tab2_Receive_index[index]++;
                    if (Tab2_Receive_index[index] >= MAX_BUF_LEN)
                    {
                        Tab2_Receive_index[index] = 0;
                    }

                    ///@TODO (Kien ##): in case don not have check frame. End of lable by <CR> <LF>
                    if (i < len)
                    {
                        if ((indata[i] == 0x0D) && (indata[i + 1] == 0x0A))
                        {
                            end_frame = true;
                        }
                    }
                }

                // Update Time Check
                Tab2_RCT[index] = DateTime.Now;
                Tab2_Curr_Receive[index] = DateTime.Now;
                if (Tab2_First_Receive[index] == Tab2_Start[index]){
                    Tab2_First_Receive[index] = Tab2_Curr_Receive[index];
                }
                Check_Wait_Receive_Data(index);

                // Change to Hex string Format Data & Add log to window and file
                Indatastring = Convert_Bytes_to_String(indata, 0, len);
                OutData = FormatData(Indatastring, DataType.Receive, Tab2_Curr_Receive[index], TabNum.Tab2, First_Receive[index]);
                if (First_Receive[index] == true){
                    OutData = portName + ":" + OutData;
                }
                Tab2_add_log(index, OutData, LogMsgType.Incoming);
                First_Receive[index] = false;       // Set Wait Receive 

                //Chek Receive Frame
                if (Check_Frame_Ena[index] == true)
                {
                    Tab2_RxFrame(index, indata, len);
                }
                else
                {
                    ///@TODO (Kien ##): in case don not have check frame. End of lable by <CR> <LF>
                    if (end_frame == true)
                    {
                        First_Receive[index] = true;
                        Reset_Buffer(index);
                    }
                }
                
            }else{
                MessageBox.Show("Can not find log file for " + portName, "Error");
            }
        }


        /******************************************************
         * 
         *  ##########  #   ##      ###   #########  ######## 
                ##      #   ###     ###   #          #      ##
                ##      #   # #    ####   #          #      # 
                ##      #   # ##   #  #   ########   ######## 
                ##      #   #  ## ## ##   #          #      # 
                ##      #   #  ## #  ##   #          #      ##
                ##      #   #   ###  ##   #########  #      ##

         *******************************************************/
        private void Timer_ISR(object sender, EventArgs e)
        {
            int index, write_len;
            byte [] data_write = new byte[BUF_LEN];
            string log_mess = "";

            

            // get index
            Timer CurrTimer = (Timer)sender;
            index = Convert.ToByte(CurrTimer.Tag);
            if (Has_data[index] == true)
            {
                if (Tab2_XMODEM[index].Enable == true)
                {
                    ComControlArray[index].Timer_Stop();
                    ComControlArray[index].Timer_Start();
                    return;
                }

                ComControlArray[index].Timer_Stop();
                switch (Tab2_Status[index])
                {
                    case Tab2Stauts.Init:
                        break;
                    case Tab2Stauts.Run:
                        /************************   Check expect result    **************************/
                        // @TODO (Kien #1#DONE): Check Expect Result
                        /******************************************************************************/
                        Check_Expect_Receive_Data(index);

                        // Get data for Write COM Port & Get Delay_value for Timer
                        write_len = Get_Data(index, ref data_write);

                        // Write COM & Start Timer
                        if (write_len != 0)
                        {
                            WriteCom(index, data_write, write_len);
                        }
                        
                        ComControlArray[index].Timer_setDelay(Delay_Value[index]);
                        ComControlArray[index].Timer_Start();
                        log_mess = ComControlArray[index].ComPort.PortName.ToString();
                        log_mess += " Delay:" + Delay_Value[index].ToString() + "ms\n";
                        // log_mess += "-----------------------------------------------\n\n";
                        Tab2_add_log(index, log_mess, LogMsgType.Normal);
                        break;
                    case Tab2Stauts.Pause:
                    case Tab2Stauts.Stop:

                        // Check & Clear Receive Buffer
                        Check_Expect_Receive_Data(index);

                        // Send out Next data
                        if (Tab2_Run_Step[index] != 0) {

                            // Get data for Write COM Port & Get Delay_value for Timer
                            write_len = Get_Data(index, ref data_write);

                            // Write COM & Start Timer
                            if (write_len != 0)
                            {
                                WriteCom(index, data_write, write_len);
                            }


                            ComControlArray[index].Timer_setDelay(Delay_Value[index]);
                            ComControlArray[index].Timer_Start();
                            log_mess = ComControlArray[index].ComPort.PortName.ToString();
                            log_mess += " Delay:" + Delay_Value[index].ToString() + "ms\n";
                            Tab2_add_log(index, log_mess, LogMsgType.Normal);
                            Tab2_Run_Step[index]--;
                        }
                        break;
                    default:
                        return;
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
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab2RunBT_Click(object sender, EventArgs e)
        {
            byte i;
            string folder_name;
            string time_stamp;
            string file_name;
            // string log_mes;
            DateTime current_time;
            byte[] data_write = new byte[BUF_LEN];
            int write_len;

            
            if (Tab2Running == false)
            {
                
                Comlist = new string[totalPort];
                for (i = 0; i < totalPort; i++)
                {
                    if (ComControlArray[i].ComCheckBox.Checked == true)
                    {
                        // Get Default Configure for comport
                        GetTab2SerialConfig(i);

                        // Try to Open COM Port
                        if (ComControlArray[i].OpenPort() == true)
                        {
                            // Set delay for timer
                            ComControlArray[i].Timer_Stop();

                            // Set Time check & Create log file
                            current_time = DateTime.Now;
                            Tab2_ResetTimeCheck(i, current_time);

                            folder_name = Tab2LogPathText.Text;
                            time_stamp = current_time.ToString("MMMdd_HH-mm");
                            file_name = folder_name + "\\" + ComControlArray[i].DeviceNameText.Text + "_" + Convert.ToString(ComControlArray[i].ComPort.PortName) + "_" + time_stamp + ".txt";
                            if (File.Exists(file_name) != true)
                            {
                                try
                                {
                                    FileOut[i] = File.CreateText(file_name);
                                    FileOut[i].Close();
                                }
                                catch (IOException)
                                {
                                    MessageBox.Show("Can not create file: " + file_name);
                                    return;
                                }
                            }

                            FileOut[i] = new StreamWriter(file_name, true);

                            // Write the Header Information to the file.
                            Write_Header_File(i, file_name);

                            // Get data for Write COM Port & Get Delay_value for Timer
                            write_len = Get_Data(i, ref data_write);

                            // Write COM & Start Timer
                            if (write_len != 0)
                            {
                                WriteCom(i, data_write, write_len);
                            }
                            ComControlArray[i].Timer_setDelay(Delay_Value[i]);
                            ComControlArray[i].Timer_Start();
                            Tab2_Status[i] = Tab2Stauts.Run;
                        }
                        else
                        {
                            // @Note (Kien ##): Can not open COM Port:
                            //                  + Do not create lod file
                            //                  + Do not run Timer
                            //                  + Uncheck for select COM Port
                            FileOut[i] = null;
                            ComControlArray[i].ComCheckBox.Checked = false;
                        }
                    }
                    else
                    {
                        FileOut[i] = null;
                    }

                    // Add comlist
                    Comlist[i] = ComControlArray[i].ComPort.PortName;
                    ComControlArray[i].Disable();
                    First_Receive[i] = true;
                }
                Tab2_Update_Status(Tab2Stauts.Run);
                Tab2Running = true;
            }else {
                for (i = 0; i < totalPort; i++)
                {
                    if (ComControlArray[i].ComCheckBox.Checked == true)
                    {
                        // Send Data and Start Delay Timer
                        write_len = Get_Data(i, ref data_write);

                        // Write COM & Start Timer
                        if (write_len != 0)
                        {
                            WriteCom(i, data_write, write_len);
                        }

                        ComControlArray[i].Timer_setDelay(Delay_Value[i]);
                        ComControlArray[i].Timer_Start();
                        Tab2_Status[i] = Tab2Stauts.Run;
                    }
                }
                Tab2_Update_Status(Tab2Stauts.Run);
            }

        }

        /// <summary>
        /// Name: Tab2Stop_Click
        /// Function: Stop run scripting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab2Stop_Click(object sender, EventArgs e)
        {
            byte i;
            string time_stamp;
            string log_mes;
            DateTime curr_time = DateTime.Now;

            for (i = 0; i < totalPort; i++)
            {
                if ((ComControlArray[i].ComCheckBox.Checked == true) && (Tab2Running == true))
                {
                    ComControlArray[i].ClosePort();
                    ComControlArray[i].Timer_Stop();
                    Tab2_Stop[i] = curr_time;

                    // Write log to the file.
                    time_stamp = curr_time.ToString("<HH:mm:ss>: ");
                    log_mes = time_stamp + ComControlArray[i].ComPort.PortName + ": Closed\n";
                    Tab2_add_log(i, log_mes, LogMsgType.Outgoing);
                    Write_Report(i);
                    Auto_save_RichText();
                    Tab2ReceiveData.Clear();
                    FileOut[i].Close();
                    Tab2_ResetTimeCheck(i,curr_time);

                    // Reset check frame
                    Frame_Expect_Cnt[i] = 0;
                    First_Receive[i] = true;       // Set Wait Receive 
                    Tab2_FReceive_Len[i] = 0;
                    
                }
                Data_index[i] = 0;
                ComControlArray[i].Enable();
                Tab2_Status[i] = Tab2Stauts.Stop;
            }
            Tab2_Update_Status(Tab2Stauts.Stop);
            Tab2Running = false;
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab2Pause_Click(object sender, EventArgs e)
        {
            int i;
            string log_mess;
            if (Tab2Running == false) return;

            for (i = 0; i < totalPort; i++)
            {
                if (ComControlArray[i].ComCheckBox.Checked == true)
                {
                    if (Tab2_Status[i] != Tab2Stauts.Pause)
                    {
                        Tab2_Status[i] = Tab2Stauts.Pause;
                        log_mess = ComControlArray[i].ComPort.PortName.ToString();
                        log_mess  += ": Pause Click.\nKeep delay for check Wait & Expect Recevie.\n";
                        Tab2_add_log(i, log_mess, LogMsgType.Coment);
                    }
                }
            }

            Tab2_Update_Status(Tab2Stauts.Pause);
            return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab2SigStep_Click(object sender, EventArgs e)
        {
            int i;
            byte[] data_write = new byte[BUF_LEN];
            int write_len;

            // Tab2 in Stop or Init Mode: Run for fisrt Time
            if (Tab2Running == false)
            {
                Tab2RunBT_Click(null, null);
            }
            // Tab2 in Pause Mode: Run for not fisrt Time
            else
            {
                for (i = 0; i < totalPort; i++)
                {
                    if (ComControlArray[i].ComCheckBox.Checked == true)
                    {
                        // Send Data and Start Delay Timer
                        write_len = Get_Data(i, ref data_write);

                        // Write COM & Start Timer
                        if (write_len != 0)
                        {
                            WriteCom(i, data_write, write_len);
                        }
                        ComControlArray[i].Timer_setDelay(Delay_Value[i]);
                        ComControlArray[i].Timer_Start();
                    }
                }
            }
            Tab2Running = true;
            for (i = 0; i < totalPort; i++)
            {
                Tab2_Run_Step[i] = 0;
                Tab2_Status[i] = Tab2Stauts.Pause;
            }
            Tab2_Update_Status(Tab2Stauts.Pause);
            return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab2MulStep_Click(object sender, EventArgs e)
        {
            int i;
            string num_step_str;
            byte[] data_write = new byte[BUF_LEN];
            int write_len;

            // Tab2 in Stop or Init Mode: Run for fisrt Time
            if (Tab2Running == false)
            {
                Tab2RunBT_Click(null, null);
            }
            // Tab2 in Pause Mode: Run for not fisrt Time
            else
            {
                for (i = 0; i < totalPort; i++)
                {
                    if (ComControlArray[i].ComCheckBox.Checked == true)
                    {
                        // Send Data and Start Delay Timer
                        write_len = Get_Data(i, ref data_write);

                        // Write COM & Start Timer
                        if (write_len != 0)
                        {
                            WriteCom(i, data_write, write_len);
                        }
                        ComControlArray[i].Timer_setDelay(Delay_Value[i]);
                        ComControlArray[i].Timer_Start();
                    }
                }
            }

            Tab2Running = true;
            for (i = 0; i < totalPort; i++)
            {
                num_step_str = Tab2_Step_Num.Text;
                if (num_step_str == "") num_step_str = "1";
                Tab2_Run_Step[i] = Convert.ToInt32(Tab2_Step_Num.Text) - 1;
                Tab2_Status[i] = Tab2Stauts.Pause;
            }
            Tab2_Update_Status(Tab2Stauts.Pause);

            return;
        }

        /// <summary>
        /// Name: Tab2RestoreDefaultBT_Click
        /// Function: Restore default COMPORT Setting 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab2RestoreDefaultBT_Click(object sender, EventArgs e)
        {
            Tab2SetBaudrate.SelectedIndex = 0;
            Tab2SetDataBit.SelectedIndex = 0;
            Tab2SetParity.SelectedIndex = 0;
            Tab2SetStopBit.SelectedIndex = 0;
            Tab2setThreshold.SelectedIndex = 0;
        }

        /// <summary>
        /// Name: SelectPathBT_Click
        /// Function: Open file & Load data 4 send
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectPathBT_Click(object sender, EventArgs e)
        {
            int index;
            string path;
            string buffer;
            StreamReader myfile;

            Button item = (Button)sender;
            index = item.TabIndex;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog1.FileName;
                myfile = File.OpenText(path);
                
                // Clear & Coppy new data 4 send
                while ((buffer = myfile.ReadLine()) != null)
                {
                    buffer = buffer.Trim();
                    ComControlArray[index].Data4Send.Items.Add(buffer);
                    if (buffer.Length == 6)
                    {
                        if ((buffer.Substring(0, 4) == "%LBL") && (buffer[5] >= '1') && (buffer[5] <= '6'))
                        {
                            Tab2_LBL[index, buffer[5] - '1'] = ComControlArray[index].Data4Send.Items.Count - 1;
                        }
                    }
                }
                ComControlArray[index].DataforSendLabel.Text = path;
                Has_data[index] = true;
            }
            else
            {
                MessageBox.Show("Can not Open file !", "Error");
            }
        }

        /// <summary>
        /// Name: Tab2LogBT_Click
        /// Function: Select Log folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab2LogBT_Click(object sender, EventArgs e)
        {
            string folder_name;

            // Set Initial Directory
            folder_name = "C:\\";
            if (folder_name != null)
            {
                folderBrowserDialog1.SelectedPath = folder_name;
            }

            // Open Dialog
            if ( folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                folder_name = folderBrowserDialog1.SelectedPath.ToString();
                Tab2LogPathText.Text = folder_name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab2ClearBT_Click(object sender, EventArgs e)
        {
            int i;

            Tab2ReceiveData.Clear();
            for (i = 0; i < totalPort; i++)
            {

                ComControlArray[i].Data4Send.Items.Clear();
                ComControlArray[i].DataforSendLabel.Text = "Select Data for Send";
                Data_index[i] = 0;
                Has_data[i] = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab2_save_bt_Click(object sender, EventArgs e)
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
                    Tab2ReceiveData.SaveFile(fileName, RichTextBoxStreamType.UnicodePlainText);
                }
                else
                {
                    Tab2ReceiveData.SaveFile(fileName, RichTextBoxStreamType.RichText);
                }
            }
        }

        /******************************************************************
         * 
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
         * 
         ******************************************************************/

        /// <summary>
        /// Name: Tab2ViewMode_CheckedChanged
        /// Function: change Mode View
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab2ViewMode_CheckedChanged(object sender, EventArgs e)
        {
            if (Tab2ViewHexOp.Checked == true)
                Tab2DataViewMode = DataMode.Hex;
            else
                Tab2DataViewMode = DataMode.Text;
        }


        /******************************************************************
         * 
         *                ####### ####### #    # #######          
                             #    #        ####     #             
                             #    ######    ##      #             
                             #    #        #  #     #             
                             #    #########    ##   #             
                                                                  
                                                                  
                                                                  
                 ######  #    ##   ###   ###   ##   #####   ######
                ##    ## #    ##   # #    ###   # ##     #  #     
                #        #######  #   #   # ##  # #   ####  ######
                #     ## #    ##  ######  #  ## # ##     #  #     
                 ######  #    ## #     #  #    ##  #######  ######
                             * 
         ******************************************************************/
        private void DelayValueText_TextChanged(object sender, EventArgs e)
        {   
            //@TODO (Kien #3#DONE): Regex for Default delay
            string data;
            int index, i;
            int len;
            StringBuilder sb;

            TextBox item = (TextBox)sender;
            index = item.TabIndex;

            data = item.Text;
            len = data.Length;
            sb = new StringBuilder();

            for (i = 0; i < len; i++)
            {
                if ((data[i] >= '0') && (data[i] <= '9'))
                {
                    sb.Append(data[i]);
                }
            }
            data = sb.ToString();
            if (data.Length > 4)
            {
                data = data.Substring(0, 4);
            }
            item.Text = data;
        }

        private void Tab2ReceiveData_TextChanged(object sender, EventArgs e)
        {
            if (Tab2ReceiveData.Lines.Count() >= 1000)
            {
                Auto_save_RichText();
            }
        }
        private bool Auto_save_RichText()
        {
            string folder_name;
            string time_stamp;
            string fileName;
            folder_name = Tab2LogPathText.Text;
            time_stamp = Tab2_Start[0].ToString("MMMdd");
            fileName = folder_name + "\\" + "Temp" + "-" + Auto_Save + "-" + ".rtf";
            Auto_Save++;
            Tab2ReceiveData.SaveFile(fileName, RichTextBoxStreamType.RichText);
            Tab2ReceiveData.Clear();
            return true;
        }
    }
}