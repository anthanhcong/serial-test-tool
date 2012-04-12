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
        private void Tab3_Mode_CheckedChanged(object sender, EventArgs e)
        {
            Tab3_Mode_update();
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
        private void Tab3_Start_BT_Click(object sender, EventArgs e)
        {
            // Update status
            if (t3_run == false)
            {
                // Run FSM
                if (t3_device == Tab3_device.Cradle)
                {
                    state = Cradle_FSM(Tab3_event.Run_click, null);
                }
                else if (t3_device == Tab3_device.Magallen)
                {
                    state = Magellan_FSM(Tab3_event.Run_click);
                }
                else
                {
                    state = Tab3_state.Idle;
                }

                //update status of 
                if (state == Tab3_state.Start)
                {
                    Tab3_Start_BT.Text = "Stop";

                    //Disable Setting
                    Tab3_Send_BT.Enabled = true;
                    Tab3_Enable_Setting(false);

                    // Add log
                    Add_tab3_log("Start Application \n", LogMsgType.Normal, DateTime.Now);
                    t3_run = true;
                    Tab3_wait_receive = false;
                }
            }
            else
            {
                //update status of 
                Tab3_Start_BT.Text = "Start";
                Tab3_close_port();

                //enable Setting
                Tab3_Send_BT.Enabled = true;
                Tab3_Enable_Setting(true);

                Tab3_Pause_BT.Enabled = false;   // Need to modify here

                // Add log & update state
                Add_tab3_log("Stop Application \n", LogMsgType.Normal, DateTime.Now);
                reset_trans();
                state = Tab3_state.Idle;
                t3_run = false;
            }
        }

        private void Tab3_Send_BT_Click(object sender, EventArgs e)
        {
            // Enable Pause BT
            Tab3_Pause_BT.Enabled = true;
            state = Magellan_FSM(Tab3_event.Send_click);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab3_Clear_BT_Click(object sender, EventArgs e)
        {
            Tab3_richText.Clear();
        }

        private void Tab3_Save_BT_Click(object sender, EventArgs e)
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
                    Tab3_richText.SaveFile(fileName, RichTextBoxStreamType.UnicodePlainText);
                }
                else
                {
                    Tab3_richText.SaveFile(fileName, RichTextBoxStreamType.RichText);
                }
            }
        }

        /// <summary>
        /// Name: Tab3_Load_BT_Click
        /// Function: Load data for Multi_Time Mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab3_Load_BT_Click(object sender, EventArgs e)
        {
            string path;
            string buffer;
            StreamReader myfile;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog1.FileName;
                myfile = File.OpenText(path);

                // Clear buffer and copy the new one
                Tab3_Data_List.Items.Clear();
                while ((buffer = myfile.ReadLine()) != null)
                {
                    Tab3_Data_List.Items.Add(buffer);
                }
            }
            else
            {
                Add_tab3_log("Can not Open File \n", LogMsgType.Error, DateTime.Now);
                MessageBox.Show("Can not Open file !", "Error");
            }
        }

        private void Tab3_Pause_BT_Click(object sender, EventArgs e)
        {
            if ((state == Tab3_state.In_Trans) || (state == Tab3_state.Send_PC))
            {
                if (pause == false)
                {
                    pause = true;
                    InterChar_Timer.Enabled = false;
                    Pack_Timer.Enabled = false;
                    Trans_Timer.Enabled = false;
                    Tab3_Pause_BT.Text = "Play";
                }
                else
                {
                    pause = false;
                    InterChar_Timer.Enabled = true;
                    Pack_Timer.Enabled = true;
                    Trans_Timer.Enabled = true;
                    Tab3_Pause_BT.Text = "Pause";
                }
            }
        }
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

        private void Tab3_serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string InData;

            SerialPort thisCom = (SerialPort)sender;
            InData = thisCom.ReadExisting();
            
            Tab3_serial_buf.Invoke(new EventHandler(delegate
            {
                Tab3_serial_buf.Text = Tab3_serial_buf.Text + InData;
            }));

            // Update Time check
            tab3_last_receive = tab3_curr_receive;
            tab3_curr_receive = DateTime.Now;
            if (tab3_first_receive == tab3_start)
            {
                tab3_first_receive = tab3_curr_receive;
                tab3_last_receive = tab3_curr_receive;
            }
        }

        private void Tab3_serial_buf_TextChanged(object sender, EventArgs e)
        {
            if (Tab3_serial_buf.Text != "")
            {
                Receive_timer.Stop();
                Receive_timer.Start();
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
        private void Tab3_Timer_Tick(object sender, EventArgs e)
        {
            string Time_name;
            Timer currTimer = (Timer)sender;
            Time_name = currTimer.Tag.ToString();
            switch (Time_name)
            {
                case "InterChar_Timer":
                    state = Magellan_FSM(Tab3_event.Inter_char_Timeout);
                    break;
                case "Pack_Timer":
                    // add log
                    if (t3_protocol == Tab3_protocol.NoneCTS)
                    {
                        Add_tab3_log("Package Timeout expire \n", LogMsgType.Warning, DateTime.Now);
                    }
                    else
                    {
                        Add_tab3_log("Item Received = " + item_count + "\n", LogMsgType.Warning, DateTime.Now);
                        Add_tab3_log("Stop Transaction" + "\n", LogMsgType.Normal, DateTime.Now);
                    }

                    //reset Transaction
                    reset_trans();
                    state = Tab3_state.Start;

                    // Show Message
                    if (t3_protocol == Tab3_protocol.NoneCTS)
                    {
                        MessageBox.Show("Package Timeout expire \n", "Time Out");
                    }

                    break;
                case "Trans_Timer":
                    reset_trans();
                    state = Tab3_state.Start;
                    MessageBox.Show("Trans Timeout expire", "Time Out");
                    break;
                case "Receive_timer":
                    try
                    {
                        Receive_timer.Stop();
                    }
                    catch
                    {
                        MessageBox.Show("Error with receive timer in tab3", "Error");
                    }
                    // Tab3_Parser();
                    if (Tab3_serial_buf.Text != "")
                    {
                        state = Magellan_FSM(Tab3_event.Receive);
                    }
                    break;
                default:
                    break;
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

        private void Tab3_change_protocol(object sender, EventArgs e)
        {
            if (Tab3_Protocol1_RBt.Checked == true)
            {
                t3_protocol = Tab3_protocol.CTS;
            }
            else
            {
                t3_protocol = Tab3_protocol.NoneCTS;
            }
        }

        private void Tab3_Change_View_Mode(object sender, EventArgs e)
        {
            if (Tab3_View_Text.Checked == true)
            {
                t3_dataView = DataMode.Text;
            }
            else {
                t3_dataView = DataMode.Hex;
            }
        }

        private void Tab3_Change_Device(object sender, EventArgs e)
        {
            if (Tab3_Magllan_RBt.Checked == true)
            {
                t3_device = Tab3_device.Magallen;
            }
            else {
                t3_device = Tab3_device.Cradle;
            }
        }
    }
}
