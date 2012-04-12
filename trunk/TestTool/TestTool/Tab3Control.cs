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
        #region Public Enumerations
        public enum Tab3_device {Magallen, Cradle };
        public enum Tab3_protocol {CTS, NoneCTS };
        public enum Tab3_mode {OneTime, MulTime };
        public enum Tab3_state {Idle, Start,Wait_Trans, In_Trans, Send_PC};
        public enum Tab3_event {Run_click, Send_click, Receive, Inter_char_Timeout};
        #endregion

        #region Public Enumerations
        public DataMode t3_dataView;
        public Tab3_mode t3_mode;
        public Tab3_device t3_device;
        public Tab3_protocol t3_protocol;
        public bool t3_run;
        #endregion

        #region Public Trasaction_parameter
        public int total_item;
        public int item_count;
        public string item_data;
        public int basket_ID;
        public int basket_Str;
        public Tab3_state state;
        public bool pause;
        public bool Tab3_wait_receive;
        #endregion

        #region Public Tab3_Time_check
        public DateTime tab3_start;
        public DateTime tab3_first_send;
        public DateTime tab3_last_send;
        public DateTime tab3_curr_send;
        public DateTime tab3_first_receive;
        public DateTime tab3_last_receive;
        public DateTime tab3_curr_receive;
        public DateTime tab3_stop;
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
        private bool Tab3_Init()
        {
            //Set Check Box
            Tab3_Magllan_RBt.Checked = true;
            Tab3_Protocol1_RBt.Checked = true;
            Tab3_View_Text.Checked = true;
            Tab3_Onetime_RBt.Checked = true;
            Tab3_Send_BT.Enabled = false;
            Tab3_Pause_BT.Enabled = false;   // Need to modify here
            pause = false;

            // Update status variable and display
            Tab3_Update_Status();
            Tab3_Enable_Setting(true);
            Tab3_wait_receive = false;
            return true;
        }

        private bool Tab3_Update_Status()
        {
            // View Mode
            if (Tab3_View_Hex.Checked == true) t3_dataView = DataMode.Hex;
            else t3_dataView = DataMode.Text;

            // Protocol
            if (Tab3_Protocol2_RBt.Checked == true) t3_protocol = Tab3_protocol.NoneCTS;
            else t3_protocol = Tab3_protocol.CTS;

            // Device
            if (Tab3_Cradle_RBt.Checked == true) t3_device = Tab3_device.Cradle;
            else t3_device = Tab3_device.Magallen;

            // Update status
            Tab3_Mode_update();
            

            return true;
        }

        private bool Tab3_Mode_update()
        {
            if (Tab3_MTL_RBt.Checked == true)
            {
                t3_mode = Tab3_mode.MulTime;
                Tab3_Data_text.Enabled = false;
                Tab3_Data_List.Enabled = true;
            }
            else
            {
                t3_mode = Tab3_mode.OneTime;
                Tab3_Data_text.Enabled = true;
                Tab3_Data_List.Enabled = false;
            }
            return true;
        }

        private bool Tab3_Enable_Setting(bool enable)
        {
            // port setting
            Tab3_Set_Baud.Enabled = enable;
            Tab3_Set_Data.Enabled = enable;
            Tab3_Set_Parity.Enabled = enable;
            Tab3_Set_Port.Enabled = enable;
            Tab3_Set_StopBit.Enabled = enable;
            Tab3_Set_Threshold.Enabled = enable;

            // Time out
            Tab3_set_TransTout.Enabled = enable;
            Tab3_Set_PackTout.Enabled = enable;
            Tab3_Set_InterCharDelay.Enabled = enable;

            // protocol
            Tab3_Protocol1_RBt.Enabled = enable;
            Tab3_Protocol2_RBt.Enabled = enable;

            // View mode
            // Tab3_View_Hex.Enabled = enable;
            // Tab3_View_Text.Enabled = enable;

            // Device
            Tab3_Magllan_RBt.Enabled = enable;
            Tab3_Cradle_RBt.Enabled = enable;

            return true;
        }
        

        /***************************************************************************/
        /*   #####   #####  ##   ##    ######  #####  # # # ######
            #       #     # ###  ##    #    # #     # #   #    #  
            #       #     # # # # #    #####  #     # # # #    #  
             #####   #####  #  ## #    #       #####  #   #    #                   */
        /***************************************************************************/
        /// <summary>
        /// Name: Tab3_open_port
        /// Function: Open COMPORT
        /// </summary>
        /// <returns></returns>
        private bool Tab3_open_port()
        {
            GetTab3SerialConfig();
            try
            {
                Tab3_serialPort.Open();
            }
            catch
            {
                MessageBox.Show(("Can not Open" + Tab3_serialPort.PortName), "Error");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Name: Tab3_close_port
        /// Function: Close COMPORT
        /// </summary>
        /// <returns></returns>
        private bool Tab3_close_port()
        {
            try
            {
                Tab3_serialPort.Close();
            }
            catch
            {
                MessageBox.Show(("Can not Open" + Tab3_serialPort.PortName), "Error");
            }
            return true;
        }

        /// <summary>
        /// Name: GetTab3SerialConfig()
        /// Get configuration for COMPORT
        /// </summary>
        /// <returns></returns>
        private bool GetTab3SerialConfig()
        {
            try
            {
                Tab3_serialPort.PortName = Tab3_Set_Port.Text;
                Tab3_serialPort.BaudRate = int.Parse(Tab3_Set_Baud.Text);
                Tab3_serialPort.DataBits = int.Parse(Tab3_Set_Data.Text);
                Tab3_serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), Tab3_Set_Parity.Text);
                Tab3_serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), Tab3_Set_StopBit.Text);
                Tab3_serialPort.ReceivedBytesThreshold = int.Parse(Tab3_Set_Threshold.Text);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Name: Tab3_SendData
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        private bool Tab3_SendData(string Data)
        {
            if (Tab3_serialPort.IsOpen == true)
            {
                // Update Time check
                tab3_last_send = tab3_curr_send;
                tab3_curr_send = DateTime.Now;
                if (tab3_first_send == tab3_start)
                {
                    tab3_first_send = tab3_curr_send;
                    tab3_last_send = tab3_curr_send;
                }
                
                // Send data & add log
                Tab3_serialPort.Write(Data);
                Add_tab3_log(Data, LogMsgType.Outgoing, tab3_curr_send);
                return true;
            }
            return false;
        }

        /***************************************************************************
                   ###### #######   ##   ###### #######       
                   ##        #     # ##     #   ##            
                     ####    #    ##  #    ##   ## ###        
                   #    #    #   #######   ##   ##            
                    #####    #   #     #    #   #######       
                                                              
                                                              
            #     #    ##     ####   #    #  #  #    ## ######
            ##   ##   ####   ##   ## #    #  #  ###  ## #     
            # #  ##   #  #  ##       ######  #  # ## ## ######
            # # # #  ###### ##     # #    #  #  #  #### #     
            # ### #  #    ## ######  #    #  #  #   ### ######  *
        /***************************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eevent"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private Tab3_state Magellan_FSM(Tab3_event eevent)
        {
            Tab3_state next_state;
            string request;
            string keyData;

            next_state = state;

            if (t3_protocol == Tab3_protocol.NoneCTS)
            {
                switch (state)
                {
                    case Tab3_state.Idle:
                        // Write code 
                        if (eevent == Tab3_event.Run_click)
                        {
                            if (Tab3_open_port() == true) next_state = Tab3_state.Start;
                            else
                            {
                                // @Todo
                                MessageBox.Show("ComPort Can not Open", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            }
                        }
                        break;

                    case Tab3_state.Start:
                        // Write code 
                        if (eevent == Tab3_event.Send_click)
                        {
                            if (t3_mode == Tab3_mode.OneTime)
                            {
                                request = Tab3_Data_text.Text;
                                request = request.Replace('\n', '\r');
                                Add_tab3_log("\nStart Transaction \n", LogMsgType.Normal, DateTime.Now);
                                if (Tab3_SendData(request) == true)
                                {
                                    start_tab3_timer(Pack_Timer);
                                    next_state = Tab3_state.Wait_Trans;
                                }
                            }
                            else
                            {
                                // @Todo: Implement Multiple time state
                            }
                        }
                        else if (eevent == Tab3_event.Receive)
                        {
                            Add_tab3_log(Tab3_serial_buf.Text, LogMsgType.Incoming, DateTime.Now);
                            Tab3_serial_buf.Clear();
                        }
                        break;

                    case Tab3_state.Wait_Trans:
                        // Write code 
                        if (eevent == Tab3_event.Receive)
                        {
                            if (Tab3_serial_buf.Text == null) break;
                            else if (Tab3_serial_buf.Text.Length >= 4) keyData = Tab3_serial_buf.Text.Substring(0, 4);
                            else break;

                            if (keyData == "#+BS")
                            {
                                // get total_item & item_count
                                Add_tab3_log(Tab3_serial_buf.Text, LogMsgType.Incoming, DateTime.Now);
                                Tab3_SendData("#+NL001\r");
                                total_item = Convert.ToInt16(Tab3_serial_buf.Text.Substring(4, 3));
                                item_count = 0;

                                // Start timer
                                start_tab3_timer(Trans_Timer);
                                start_tab3_timer(Pack_Timer);

                                // Next state
                                next_state = Tab3_state.In_Trans;
                            }
                            else
                            {
                                Add_tab3_log(Tab3_serial_buf.Text, LogMsgType.Incoming, DateTime.Now);
                                reset_trans();
                                state = Tab3_state.Start;
                                MessageBox.Show("Transaction Error", "Error");
                            }
                            Tab3_serial_buf.Clear();
                        }
                        break;

                    case Tab3_state.In_Trans:
                        // Write code 
                        if (eevent == Tab3_event.Receive)
                        {
                            if (Tab3_serial_buf.Text != null)
                            {
                                item_data = Tab3_Parser();
                                if (item_data != "")
                                {
                                    stop_tab3_timer(Pack_Timer, false);
                                    item_count++;
                                    start_tab3_timer(Pack_Timer);
                                    start_tab3_timer(InterChar_Timer);
                                    next_state = Tab3_state.Send_PC;
                                }
                            }
                        }
                        break;
                            
                    case Tab3_state.Send_PC:
                        // Write code 
                        if (eevent == Tab3_event.Inter_char_Timeout)
                        {
                            Send_data2PC(item_data);
                            if (item_count < total_item)
                            {
                                next_state = Tab3_state.In_Trans;
                            }
                            else
                            {
                                // Add Log
                                Add_tab3_log("Item Received = " + item_count + "\n", LogMsgType.Warning, DateTime.Now);
                                Add_tab3_log("Total items = " + total_item + "\n", LogMsgType.Warning, DateTime.Now);
                                Add_tab3_log("Stop Transaction \n", LogMsgType.Normal, DateTime.Now);

                                // Reset transaction
                                reset_trans();
                                next_state = Tab3_state.Start;
                            }
                        }
                        break;
                    default:
                        reset_trans();
                        break;
                }
            }
            else if (t3_protocol == Tab3_protocol.CTS)
            {
                switch (state)
                {
                    case Tab3_state.Idle:
                        // Write code 
                        if (eevent == Tab3_event.Run_click)
                        {
                            if (Tab3_open_port() == true)
                            {
                                next_state = Tab3_state.Start;
                                Tab3_serialPort.RtsEnable = true;
                            }
                            else
                            {
                                // @Todo
                                MessageBox.Show("ComPort Can not Open", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            }
                        }
                        break;

                    case Tab3_state.Start:
                        // Write code 
                        if (eevent == Tab3_event.Send_click)
                        {
                            if (t3_mode == Tab3_mode.OneTime)
                            {
                                request = Tab3_Data_text.Text;
                                request = request.Replace('\n', '\r');
                                Add_tab3_log("\n Start Transaction \n", LogMsgType.Normal, DateTime.Now);
                                if (Tab3_SendData(request) == true)
                                {
                                    start_tab3_timer(Pack_Timer);
                                    next_state = Tab3_state.Wait_Trans;
                                }
                            }
                            else
                            {
                                // @Todo: Implement Multiple time state
                            }
                        }
                        else if (eevent == Tab3_event.Receive)
                        {
                            Add_tab3_log(Tab3_serial_buf.Text, LogMsgType.Incoming, DateTime.Now);
                            if (t3_protocol == Tab3_protocol.CTS)
                            {
                                Tab3_serialPort.RtsEnable = false;
                                Tab3_serialPort.RtsEnable = true;
                            }
                            Tab3_serial_buf.Clear();
                        }
                        break;

                    case Tab3_state.Wait_Trans:
                    case Tab3_state.In_Trans:
                        // Write code 
                        if (eevent == Tab3_event.Receive)
                        {
                            if (Tab3_serial_buf.Text != null)
                            {
                                item_data = Tab3_Parser();
                                if (item_data != "")
                                {
                                    stop_tab3_timer(Pack_Timer, false);
                                    item_count++;
                                    start_tab3_timer(Pack_Timer);
                                    start_tab3_timer(InterChar_Timer);
                                    next_state = Tab3_state.Send_PC;
                                }
                            }
                        }
                        break;

                    case Tab3_state.Send_PC:
                        // Write code 
                        if (eevent == Tab3_event.Inter_char_Timeout)
                        {
                            Send_data2PC(item_data);
                            next_state = Tab3_state.In_Trans;
                        }
                        break;
                    default:
                        reset_trans();
                        break;
                }
            }
            else {
                MessageBox.Show("Can not define protocol is: Bmode or New Protocol", "Error");
            }

            return next_state;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private Tab3_state Cradle_FSM(Tab3_event e, string data)
        {
            Tab3_state next_state;

            next_state = state;
            switch (state)
            {
                case Tab3_state.Idle:
                    // Write code 

                    break;
                case Tab3_state.Start:
                    // Write code 

                    break;
                case Tab3_state.Wait_Trans:
                    // Write code 

                    break;
                case Tab3_state.In_Trans:
                case Tab3_state.Send_PC:
                default:
                    next_state = Tab3_state.Idle;
                    break;
            }
            return Tab3_state.Idle;
        }


        private bool reset_trans()
        {
            stop_tab3_timer(null,true);
            total_item = 0;
            item_count = 0;

            tab3_start = tab3_first_receive 
                = tab3_first_send
                = tab3_curr_receive
                = tab3_curr_send
                = tab3_last_receive
                = tab3_last_send = DateTime.Now;
            pause = false;
            Tab3_Pause_BT.Enabled = false;
            return true;
        }

        /***************************************************************************/
        /*  ######     ##    #######  ######  #######  ###### 
            #    ##   # ##   #     #  #       ##       #     #
            ######   #   #   #######   #####  #######  ###### 
            #        ######  #     # #      # ##       #    ##
            #       #     #  #     #  ####### #######  #     #*/
        /***************************************************************************/
        private string Tab3_Parser()
        {
            string ret_string = "";
            string key;
            int len;

            if (ret_string == "\r")
            {
                Tab3_serial_buf.Clear();
                return "";
            }

            len = Tab3_serial_buf.Text.Length;
            // Check error code
            if (len >= 4)
            {
                key = Tab3_serial_buf.Text.Substring(0, 4);
                if (key == "#+BE")
                {
                    ret_string = Tab3_serial_buf.Text;
                    Tab3_serial_buf.Clear();
                    reset_trans();
                }
                else if (Tab3_serial_buf.Text == "#+EC")
                {
                    ret_string = Tab3_serial_buf.Text;
                    Tab3_serial_buf.Clear();
                    MessageBox.Show("Transaction Error: \n" + ret_string, "Error");
                    reset_trans();
                }
            }
            // Item data
            // Search keyword
            if (Tab3_serial_buf.Text != "")
            {
                ret_string = Tab3_serial_buf.Text;
                Add_tab3_log(ret_string, LogMsgType.Incoming, DateTime.Now);
                Tab3_serial_buf.Clear();
            }
            return ret_string;
        }

        /****************************************************************************/
        /*                   #####  # #### ##    #  ##### 
                            ##      #      # #   #  #    #
                              ####  # ###  #  ## # ##    #
                            #    ## #      #   ### ##   ##
                             #####  ###### #    ## ###### 
                                                          
                                                          
                                     ######               
                                     # ## #               
                                       ##   #####         
                                       ##  ##   #         
                                       ##   #   #         
                                       #     ###          
                                                          
                                                          
                                    ######   ####         
                                    #    #  #    ##       
                                    ###### #              
                                    #      ##    ##       
                                    #       ######                                  */
         /***************************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"><do not use>
        /// <returns></returns>
        private bool Send_data2PC(string item)
        {
            string s;
            int len, i;

            // Request next Label
            if (t3_protocol == Tab3_protocol.NoneCTS)
            {
                // New Protocal
                if (item_count < total_item)
                {
                    s = Convert.ToString((item_count + 1));
                    len = s.Length;
                    for (i = len; i < 3; i++)
                    {
                        s = "0" + s;
                    }
                    Tab3_SendData("#+NL" + s + "\r");
                }
                else if (item_count == total_item)
                {
                    Tab3_SendData("#+NL000\r");
                }
            }
            else {
                // BMode
                Tab3_serialPort.RtsEnable = false;
                Tab3_serialPort.RtsEnable = true;
            }
            return true;
        }

        /***************************************************************************/
        /*      ##########  #   ##      ###   #########  ######## 
                    #       #   ###     ###   #          #      ##
                    ##      #   # #    ####   #          #      # 
                    ##      #   # ##   #  #   ########   ######## 
                    ##      #   #  ## ## ##   #          #      # 
                    ##      #   #  ## #  ##   #          #      ##
                    ##      #   #   ###  ##   #########  #      ##                 */
        /***************************************************************************/

        private bool start_tab3_timer( Timer CurrTime)
        {
            int delay;
            string timer_name;
            timer_name = CurrTime.Tag.ToString();

            switch (timer_name)
            {
                case "InterChar_Timer":
                    delay = Convert.ToInt16(Tab3_Set_InterCharDelay.Text);
                    break;
                case "Pack_Timer":
                    delay = Convert.ToInt16(Tab3_Set_PackTout.Text);
                    break;
                case "Trans_Timer":
                    delay = Convert.ToInt16(Tab3_set_TransTout.Text);
                    break;
                default:
                    delay = 0;
                    MessageBox.Show(timer_name + " did not set delay", "Error");
                    break;
            }

            if (delay != 0)try
            {
                
                CurrTime.Stop();
                CurrTime.Interval = delay;
                CurrTime.Start();
            }
            catch
            {
                MessageBox.Show("Can not start package timer","Error");
                return false;
            }
            return true;
        }

        private bool stop_tab3_timer(Timer CurrTime, bool All)
        {
            if (All != true)
            {
                CurrTime.Stop();
            }
            else {
                InterChar_Timer.Stop();
                Pack_Timer.Stop();
                Trans_Timer.Stop();
            }
            return true;
        }



        public bool Add_tab3_log(String text, LogMsgType type, DateTime time)
        {
            string log_mes;
            log_mes = text;
            switch (type)
            {
                case LogMsgType.Error:
                    log_mes = text;
                    break;
                case LogMsgType.Incoming:
                    log_mes = FormatData(text, DataType.Receive, time, TabNum.Tab3, Tab3_wait_receive);
                    Tab3_wait_receive = false;
                    break;
                case LogMsgType.Normal:
                    log_mes = text;
                    break;
                case LogMsgType.Outgoing:
                    log_mes = FormatData(text, DataType.Send, time, TabNum.Tab3, Tab3_wait_receive);
                    Tab3_wait_receive = true;
                    break;
                case LogMsgType.Warning:
                    log_mes = text;
                    break;
                default:
                    break;
            }

            Add_logs(log_mes, type, TabNum.Tab3);
            return true;
        }
    }
}
