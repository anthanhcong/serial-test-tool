using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
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
    public enum XMODEM_MODE
    {
        XMODEM_128 = 0,
        XMODEM_1K,
    };
    public class XMODEM
    {
        public bool Enable;
        public XMODEM_MODE Mode;    // = new XMODEM_MODE();
        public Timer X_Timer;   // = new Timer();
        public int Timeout;
        public int XMODEM_Retry; 
        public string Log_Folder;
        public byte[] Buffer = new byte[1100];
        public int Received_index;

        public XMODEM()
        {
            Enable = false;
            Timeout = 0;
            Log_Folder = "";
            X_Timer = new Timer();
        }
    }

    partial class Test_Form
    {
        /// <summary>
        /// XMODEM command syntax: %XMODEM:[Mode]:[TimeOut]:[Retry]:[Folder]
        /// </summary>
        /// <param name="index"></param>
        /// <param name="command_str"></param>
        /// <returns></returns>
        private bool XMODEL_Function(int index, string command_str)
        {
            int len, i;
            string cmd;
            string[] tag;
            string mode = "";
            string timeout_str, retry_str;
            int timeout, retry;
            string log_folder, log_mess;
            byte [] data = {0x43};

            len = command_str.Length;
            if (command_str == "") return false;
            if (command_str[0] != ':') return false;

            cmd = command_str.Substring(1, len - 1).Trim();
            len = cmd.Length;
            tag = cmd.Split(':');
            if (tag.Length < 4) return false;
            else
            {
                try
                {
                    mode = tag[0];
                    timeout_str = tag[1];
                    retry_str = tag[2];
                    timeout = Convert.ToInt32(timeout_str);
                    retry = Convert.ToInt32(retry_str);
                    log_folder = tag[3];

                    Tab2_XMODEM[index].Enable = true;
                    switch (mode)
                    {
                        case "1k":
                        case "1K":
                            Tab2_XMODEM[index].Mode = XMODEM_MODE.XMODEM_1K;
                            data[0] = 0x43;
                            WriteCom(index, data, 1);
                            log_mess = "Start Receive XMODEM_1K\n";
                            Tab2_add_log(index, log_mess, LogMsgType.Coment);
                            break;
                        case "128":
                            Tab2_XMODEM[index].Mode = XMODEM_MODE.XMODEM_128;
                            data[0] = 0x15;
                            WriteCom(index, data, 1);
                            log_mess = "Start Receive XMODEM_128\n";
                            Tab2_add_log(index, log_mess, LogMsgType.Coment);
                            break;
                        default:
                            return false;
                    }

                    Tab2_XMODEM[index].Timeout = timeout;
                    Tab2_XMODEM[index].Log_Folder = log_folder;
                    Tab2_XMODEM[index].Received_index = 0;
                    Tab2_XMODEM[index].XMODEM_Retry = retry;

                    
                    
                    Tab2_XMODEM[index].X_Timer.Stop();
                    Tab2_XMODEM[index].X_Timer.Interval = timeout;
                    Tab2_XMODEM[index].X_Timer.Start();
                    for (i = 0; i < 1100; i++)
                    {
                        Tab2_XMODEM[index].Buffer[i] = 0;
                    }
                    
                }
                catch (Exception ex)
                {
                    // MessageBox.Show(ex.ToString(), "Error");
                    return false;
                }

            }
            return true;
        }

        private void XMODEM_Timer_ISR(object sender, EventArgs e)
        {
            int index;
            string log_mess;
            byte[] send_data = { 0x06 };

            // get index
            Timer CurrTimer = (Timer)sender;
            index = Convert.ToByte(CurrTimer.Tag);
            if (Has_data[index] == true)
            {
                // Check End of Receive 
                if (Tab2_XMODEM[index].Received_index != 0)
                {
                    if (Tab2_XMODEM[index].Buffer[Tab2_XMODEM[index].Received_index - 1] == 0x04)
                    {
                        XMODEM_Complete_Receive(index);
                        return;
                    }
                }
 
                // Retry
                if (Tab2_XMODEM[index].XMODEM_Retry != 0)
                {
                    Tab2_XMODEM[index].XMODEM_Retry--;
                    WriteCom(index, send_data, 1);
                    log_mess = "XMODEM: retry -" + Tab2_XMODEM[index].XMODEM_Retry + "\n";
                    Tab2_add_log(index, log_mess, LogMsgType.Coment);
                    return;
                }

                XMODEM_TimeOut(index);
            }
        }

        private bool Process_XMODEL_Data(int index, byte[] data, int len)
        {
            string log_mess;
            int i;
            int cur_r_index = Tab2_XMODEM[index].Received_index;
            byte [] send_data = {0x06};

            if (len == 0) return false;
            for (i = 0; i < len; i++)
            {
                Tab2_XMODEM[index].Buffer[cur_r_index + i] = data[i];
            }
            Tab2_XMODEM[index].Received_index += len;

            switch (Tab2_XMODEM[index].Mode)
            {
                case XMODEM_MODE.XMODEM_128:
                    if (Tab2_XMODEM[index].Received_index >= 131)
                    {
                        for (i = 0; i < 132; i++)
                        {
                            Tab2_XMODEM[index].Buffer[cur_r_index + i] = 0;
                        }
                        // Complete one Frame
                        Tab2_XMODEM[index].Received_index = 0;
                        Tab1DataReceiveLine.Invoke(new EventHandler(delegate
                        {
                            WriteCom(index, send_data, 1);
                            Tab2_XMODEM[index].X_Timer.Stop();
                            //Tab2_XMODEM[index].X_Timer.Interval = Tab2_XMODEM[index].Timeout;
                            Tab2_XMODEM[index].X_Timer.Start();
                            log_mess = "Complete Frame\n";
                            Tab2_add_log(index, log_mess, LogMsgType.Coment);
                        }));
                    }
                    break;
                case XMODEM_MODE.XMODEM_1K:
                    if (Tab2_XMODEM[index].Received_index >= 1028)
                    {
                        // Complete one Frame
                        for (i = 0; i < 1100; i++)
                        {
                            Tab2_XMODEM[index].Buffer[cur_r_index + i] = 0;
                        }

                        Tab1DataReceiveLine.Invoke(new EventHandler(delegate
                        {
                            WriteCom(index, send_data, 1);
                            Tab2_XMODEM[index].X_Timer.Stop();
                            //Tab2_XMODEM[index].X_Timer.Interval = Tab2_XMODEM[index].Timeout;
                            Tab2_XMODEM[index].X_Timer.Start();
                            log_mess = "Complete Frame\n";
                            Tab2_add_log(index, log_mess, LogMsgType.Coment);
                        }));
                    }
                    break;
                default:
                    return false;
            }

            // Check Complete file
            if ((len == 1)&&(data[0] == 0x04))
            {
                XMODEM_Complete_Receive(index);
            }
            return true;
        }

        public void XMODEM_Complete_Receive(int index)
        {
            string log_mess;
            byte[] send_data = { 0x06 };

            Tab1DataReceiveLine.Invoke(new EventHandler(delegate
            {
                Tab2_XMODEM[index].X_Timer.Stop();
                Tab2_XMODEM[index].Enable = false;
                log_mess = "Complete Receive X_MODEM Data\n";
                Tab2_add_log(index, log_mess, LogMsgType.Coment);
                WriteCom(index, send_data, 1);

                Goto_Next_Code_Line(index);
            }));
        }

        public void XMODEM_TimeOut(int index)
        {
            string log_mess;
            byte[] send_data = { 0x06 };

            Tab1DataReceiveLine.Invoke(new EventHandler(delegate
            {
                Tab2_XMODEM[index].X_Timer.Stop();
                Tab2_XMODEM[index].Enable = false;
                log_mess = "X_MODEM Time Out\n";
                Tab2_add_log(index, log_mess, LogMsgType.Coment);

                Goto_Next_Code_Line(index);
            }));
        }
    }
}