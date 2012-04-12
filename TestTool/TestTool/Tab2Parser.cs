using System;
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
    partial class Test_Form
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="command_str"></param>
        /// <returns></returns>
        private bool Parser_command(int index, string command_str)
        {
            string command = "";
            bool ret_var = false;
            int len = command_str.Length;

            if (len < 3) return false;
            else command = command_str.Substring(0, 3);

            switch (command)
            {
                case "EXP":
                    ret_var = Set_Expect_Receive(index, command_str.Substring(3, len - 3));
                    break;
                case "WAT":
                    ret_var = Set_Wait_Receive(index, command_str.Substring(3, len - 3));
                    break;
                case "DLY":
                    ret_var = Set_Delay_Value(index, command_str.Substring(3, len - 3));
                    break;
                case "COM":
                    ret_var = Config_Comport(index, command_str.Substring(3, len - 3));
                    break;
                case "STR":
                    ret_var = Store_Time_Value(index, command_str.Substring(3, len - 3));
                    break;
                case "SST":
                    ret_var = Store_String(index, command_str.Substring(3, len - 3));
                    break;
                case "CAT":
                    ret_var = Concat_String(index, command_str.Substring(3, len - 3));
                    break;
                case "DTA":
                    ret_var = Compute_Delta_Time(index, command_str.Substring(3, len - 3));
                    break;
                case "PRN":
                    ret_var = Print_String(index, command_str.Substring(3, len - 3));
                    break;
                case "RPT":
                    ret_var = Report_Pass_Fail(index);
                    break;
                case "PIC":
                    ret_var = Display_Pic(index, command_str.Substring(3, len - 3));
                    break;

                // @Note (Kien ##): Check Frame Command
                case "ECF":
                    Check_Frame_Ena[index] = true;
                    ret_var = true;
                    break;
                case "DCF":
                    Check_Frame_Ena[index] = false;
                    ret_var = true;
                    break;
                case "RCN":
                    Frame_Expect_Cnt[index] = 0;
                    ret_var = true;
                    break;
                //End @Note

                // @Note (Kien ##): For GoTo Function
                case "LBL":
                    // Ignore this command
                    ret_var = true;
                    break;
                case "GTO":
                    if ((command_str[3] >= '1') && (command_str[3] <= '6')){
                        Data_index[index] = Tab2_LBL[index, command_str[3] - '1'];
                    }
                    ret_var = true;
                    break;
                //End @Note

                default:
                    ret_var = false;
                    break;
            }
            return ret_var;
        }

        private bool Set_Expect_Receive(int index, string command_str)
        {
            int len;
            string cmd;
            string[] tag;
            len = command_str.Length;
            if (command_str == "") return false;
            if (command_str[0] != ':') return false;

            cmd = command_str.Substring(1, len - 1).Trim();
            if (cmd == "")
            {
                //@NOTE (Kien ##): Expect No Respond
                Expect_Cnt[index]++;
            }
            else
            {
                len = cmd.Length;
                tag = cmd.Split(':');

                // Get expect respond
                if (tag.Length == 1)
                {
                    if (Expect_Cnt[index] < MAX_RESOURCE)
                    {
                        Tab2_Expect_Respond[index, Expect_Cnt[index]] = tag[0].Trim();
                        Expect_Cnt[index]++;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (tag.Length == 2)
                {
                    Tab2_Expect_Respond[index, Expect_Cnt[index]] = tag[0].Trim();
                    Tab2_Expect_Send[index, Expect_Cnt[index]] = tag[1];
                    Expect_Cnt[index]++;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private bool Set_Wait_Receive(int index, string command_str)
        {
            int len;
            string cmd;
            string[] tag;
            len = command_str.Length;
            if (command_str == "") return false;
            if (command_str[0] != ':') return false;

            cmd = command_str.Substring(1, len - 1).Trim();
            if (cmd == "")
            {
                //@NOTE (Kien ##): Expect No Respond
                Wait_Cnt[index]++;
            }
            else
            {
                len = cmd.Length;
                tag = cmd.Split(':');

                // Get expect respond
                if (tag.Length == 1)
                {
                    if (Wait_Cnt[index] < MAX_RESOURCE)
                    {
                        Tab2_Wait_Respond[index, Expect_Cnt[index]] = tag[0].Trim();
                        Wait_Cnt[index]++;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (tag.Length == 2)
                {
                    Tab2_Wait_Respond[index, Expect_Cnt[index]] = tag[0].Trim();
                    Tab2_Wait_Send[index, Expect_Cnt[index]] = tag[1].Trim();
                    Wait_Cnt[index]++;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private bool Set_Delay_Value(int index, string command_str)
        {
            int len;
            int cut_index;
            int min_delay, max_delay, delay_var;
            string cmd;
            
            string []tag;
            string delay_str;
            string option = "";
            bool ret_var;

            // parser command
            len = command_str.Length;
            if (command_str == "")
            {
                delay_str = ComControlArray[index].DelayValueText.Text;
                option = "ms";
            }
            else if (command_str[0] != ':') return false;
            else
            {
                cmd = command_str.Substring(1, len - 1).Trim();
                len = cmd.Length;
                tag = cmd.Split(':');
                cut_index = cmd.IndexOf(':');

                // Get Delay
                if (tag.Length == 1) {
                    delay_str = tag[0];
                    option = "ms";
                }
                else if (tag.Length >= 2)
                {
                    delay_str = tag[0];
                    option = tag[1];
                }
                else
                {
                    delay_str = ComControlArray[index].DelayValueText.Text;
                    option = "ms";
                }
                
            }
            delay_str = delay_str.Trim();
            option = option.Trim();
            tag = delay_str.Split('-');
            
            // set delay value
            if (tag.Length == 1)
            {
                delay_var = Convert.ToInt32(tag[0]);
            }
            else { 
                min_delay = Convert.ToInt32(tag[0]);
                max_delay = Convert.ToInt32(tag[1]);
                if (min_delay < max_delay)
                {
                    delay_var = random.Next(min_delay, max_delay);
                }
                else
                {
                    delay_var = min_delay;
                }

            }

            ret_var = true;

            switch (option) { 
                case "":
                case "ms":
                    Delay_Value[index] = delay_var;
                    break;
                case "s":
                    Delay_Value[index] = delay_var * 1000;
                    break;
                case "m":
                    Delay_Value[index] = delay_var * 1000 * 60;
                    break;
                default:
                    delay_str = ComControlArray[index].DelayValueText.Text.Trim();
                    Delay_Value[index] = delay_var;
                    ret_var = false;
                    break;
            }
            return ret_var;
        }

        private bool Config_Comport(int index, string command_str)
        {
            int len;
            string cmd;
            string []tag;   //, 
            string value;
            string log_mess = "";
            bool ret_var = false;

            len = command_str.Length;
            if (command_str == "") return false;
            if (command_str[0] != ':') return false;

            cmd = command_str.Substring(1, len - 1).Trim();
            len = cmd.Length;
            tag = cmd.Split(':');
            // cut_index = cmd.IndexOf(':');

            // Parser Command
            value = "";
            if (tag.Length == 1)
            {
                switch (tag[0])
                {
                    case "ONP":
                        if (ComControlArray[index].ComPort.IsOpen == false)
                        {
                            ComControlArray[index].OpenPort();
                        }
                        ret_var = true;
                        break;
                    case "OFF":
                        if (ComControlArray[index].ComPort.IsOpen == true)
                        {
                            ComControlArray[index].ClosePort();
                        }
                        ret_var = true;
                        break;
                    default:
                        break;
                }
            }
            else if (tag.Length == 2)
            {
                value = tag[1];
                // Configure COMPORT
                switch (tag[0])
                {
                    case "BAD":
                        if ((value == "9600") || (value == "19200") ||
                            (value == "38400") || (value == "57600") ||
                            (value == "115200"))
                        {
                            if (ComControlArray[index].ComPort.IsOpen == true)
                            {
                                ComControlArray[index].ClosePort();
                                ComControlArray[index].ComPort.BaudRate = int.Parse(value);
                                ComControlArray[index].OpenPort();
                            }
                            else
                            {
                                ComControlArray[index].ComPort.BaudRate = int.Parse(value);
                            }
                            ret_var = true;
                        }
                        break;

                    case "DAT":
                        if ((value == "8") || (value == "7") ||
                            (value == "6") || (value == "5"))
                        {
                            if (ComControlArray[index].ComPort.IsOpen == true)
                            {
                                ComControlArray[index].ClosePort();
                                ComControlArray[index].ComPort.DataBits = int.Parse(value);
                                ComControlArray[index].OpenPort();
                            }
                            else
                            {
                                ComControlArray[index].ComPort.DataBits = int.Parse(value);
                            }
                            ret_var = true;
                        }
                        break;

                    case "STP":
                        if ((value == "One") || (value == "Two") ||
                            (value == "OnePointFive"))
                        {
                            if (ComControlArray[index].ComPort.IsOpen == true)
                            {
                                ComControlArray[index].ClosePort();
                                ComControlArray[index].ComPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), value);
                                ComControlArray[index].OpenPort();
                            }
                            else
                            {
                                ComControlArray[index].ComPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), value);
                            }
                            ret_var = true;
                        }
                        break;

                    case "PAR":
                        if ((value == "None") || (value == "Even") ||
                            (value == "Odd") || (value == "Mark") ||
                            (value == "Space"))
                        {
                            if (ComControlArray[index].ComPort.IsOpen == true)
                            {
                                ComControlArray[index].ClosePort();
                                ComControlArray[index].ComPort.Parity = (Parity)Enum.Parse(typeof(Parity), value);
                                ComControlArray[index].OpenPort();
                            }
                            else
                            {
                                ComControlArray[index].ComPort.Parity = (Parity)Enum.Parse(typeof(Parity), value);
                            }
                            ret_var = true;
                        }
                        break;
                    default:
                        break;
                }
            }

            // Check error & Write to Log File
            if (ret_var == false)
            {
                log_mess += "Error Command at line" + (Data_index[index] + 1) + "\n Failed \n";
                Tab2_add_log(index, log_mess, LogMsgType.Error);
            }
            else
            {
                log_mess = "\nChange configure of ";
                log_mess += ComControlArray[index].ComPort.PortName.ToString();
                log_mess += ": " + tag[0] + " with new value " + value + "\n";
                log_mess += "Change configure success \n";
                Tab2_add_log(index, log_mess, LogMsgType.Warning);
            }

            return ret_var;
        }

        private bool Store_Time_Value(int index, string command_str)
        {
            int len;
            int cut_index;
            int tsv_index, i;
            string cmd;
            string log_mess = "";
            bool ret_var = false;

            len = command_str.Length;
            if (command_str == "") return false;
            if (command_str[0] != ':') return false;

            cmd = command_str.Substring(1, len - 1).Trim();
            len = cmd.Length;
            cut_index = cmd.IndexOf(':');

            // Get cur_TSV
            if ((cmd[0] == '$') && (cmd[1] == 'T') && (cmd[2] >= '1') && (cmd[2] <= '1' + MAX_RESOURCE))
            {
                tsv_index = cmd[2] - '1';

                // Get Time Value
                if ((cmd[3] != ':') || (cmd[4] != '$')) return false;
                if ((cmd[5] == 'T') && (cmd[6] >= '1') && (cmd[6] <= '1' + MAX_RESOURCE))
                {
                    i = cmd[6] - '1';
                    Tab2_TSV[index, tsv_index] = Tab2_TSV[index, i];
                    log_mess = "Copy TSV[" + i + "] to TSV[" + (tsv_index+1) + "]\n";   // Copy TSV[x] to TSV[y]
                    ret_var = true;
                }
                else if (cmd.Substring(5, 3) == "CST")
                {
                    Tab2_TSV[index, tsv_index] = DateTime.Now;
                    log_mess = "Store Current Time to: TSV[" + (tsv_index + 1) + "]\n";   //Store Current Time to: TSV[x]
                    ret_var = true;
                }
                else if (cmd.Substring(5, 3) == "RCT")
                {
                    Tab2_TSV[index, tsv_index] = Tab2_RCT[index];
                    log_mess = "Store Last Receive Time to: TSV[" + (tsv_index + 1) + "]\n";   //Store Last Receive Time to: TSV[x]
                    ret_var = true;
                }
                else if (cmd.Substring(5, 3) == "TCT")
                {
                    Tab2_TSV[index, tsv_index] = Tab2_TCT[index];
                    log_mess = "Store Last Transmit Time to: TSV[" + (tsv_index + 1) + "]\n";   //Store Last Transmit Time to: TSV[x]
                    ret_var = true;
                }
                else
                {
                    return false;
                }

                if (Tab2_HideResourceFunction.Checked == false)
                {
                    Tab2_add_log(index, log_mess, LogMsgType.Normal);
                }
            }
            return ret_var;
        }

        private bool Store_String(int index, string command_str)
        {
            int len;
            int ssv_index,i;
            string cmd;
            string []tag;
            string log_mess;

            len = command_str.Length;
            if (command_str == "") return false;
            if (command_str[0] != ':') return false;

            cmd = command_str.Substring(1, len - 1).Trim();
            len = cmd.Length;
            tag = cmd.Split(':');
            if (tag.Length != 2) return false;
            else
            {
                tag[0].Trim();
                tag[1].Trim();
                if ((tag[0][0] == '$') && (tag[0][1] == 'S') && (tag[0][2] >= '1') && (tag[0][2] <= '1' + MAX_RESOURCE))
                {
                    ssv_index = tag[0][2] - '1';
                }
                else return false;

                if ((tag[1][0] == '$') && (tag[1][1] == 'S') && (tag[1][2] >= '1') && (tag[1][2] <= '1' + MAX_RESOURCE))
                {
                    i = tag[1][2] - '1';
                    Tab2_SSV[index, ssv_index] = Tab2_SSV[index, i];
                    log_mess = "Copy SSV[" + i + "] to SSV[" + (ssv_index+1) + "]\n";   // Copy SSV[x] to SSV[y]
                }
                else if (tag[1] == "$RCV")
                {
                    Tab2_SSV[index, ssv_index] = Convert_Bytes_to_String(Tab2_Receive_Buf[index], 0, Tab2_Receive_index[index]);
                    log_mess = "Store Received String to: SSV[" + (ssv_index + 1) + "]\n";   //Store Received String to: TSV[x]
                }
                else
                {
                    Tab2_SSV[index, ssv_index] = tag[1];
                    log_mess = "Store string: " + tag[1] + " to: SSV[" + (ssv_index + 1) + "]\n";   //Store  String to: TSV[x]
                }

                // Log Message
                if (Tab2_HideResourceFunction.Checked != true)
                {
                    Tab2_add_log(index, log_mess, LogMsgType.Normal);
                }
            }
            return true;

        }

        private bool Concat_String(int index, string command_str)
        {
            int len;
            int ssv_index, i;
            string cmd;
            string[] tag;
            string log_mess;

            len = command_str.Length;
            if (command_str == "") return false;
            if (command_str[0] != ':') return false;

            cmd = command_str.Substring(1, len - 1).Trim();
            len = cmd.Length;
            tag = cmd.Split(':');
            if (tag.Length != 2) return false;
            else
            {
                tag[0].Trim();
                tag[1].Trim();
                if ((tag[0][0] == '$') && (tag[0][1] == 'S') && (tag[0][2] >= '1') && (tag[0][2] <= '1' + MAX_RESOURCE))
                {
                    ssv_index = tag[0][2] - '1';
                }
                else return false;

                if ((tag[1][0] == '$') && (tag[1][1] == 'S') && (tag[1][2] >= '1') && (tag[1][2] <= '1' + MAX_RESOURCE))
                {
                    i = tag[1][2] - '1';
                    Tab2_SSV[index, ssv_index] += Tab2_SSV[index, i];
                    log_mess = "Concatanate SSV[" + i + "] with SSV[" + ssv_index + "]\n";   // Concatanate SSV[x] with SSV[y]
                }
                else if (tag[1] == "$RVC")
                {
                    Tab2_SSV[index, ssv_index] += Tab2_Receive_Buf[index];
                    log_mess = "Concatanate Received String with: SSV[" + ssv_index + "]\n";   //Concatanate Received String with: SSV[x]
                }
                else
                {
                    Tab2_SSV[index, ssv_index] += tag[1];
                    log_mess = "Concatanate string: " + tag[1] + " with: SSV[" + ssv_index + "]\n";   //Concatanate  String with: SSV[x]
                }

                // Log Message
                if (Tab2_HideResourceFunction.Checked != true)
                {
                    Tab2_add_log(index, log_mess, LogMsgType.Normal);
                }
            }
            return true;
        }

        private bool Compute_Delta_Time(int index, string command_str)
        {
            int len, i;
            int []t_index = new int[3];
            string cmd;
            string[] tag;
            string log_mess = "";

            len = command_str.Length;
            if (command_str == "") return false;
            if (command_str[0] != ':') return false;

            cmd = command_str.Substring(1, len - 1).Trim();
            len = cmd.Length;

            tag = cmd.Split(':');
            if (tag.Length != 3) return false;
            for (i = 0; i < 3; i++ )
            {
                tag[i].Trim();
                if ((i == 0) && (tag[i].Substring(0, 2) == "$P"))
                {
                    t_index[i] = tag[i][2] - '1';
                }
                else if ((i != 0) && (tag[i].Substring(0, 2) == "$T"))
                {
                    t_index[i] = tag[i][2] - '1';
                }
                else return false;
            }

            Tab2_TimeSpend[index, t_index[0]] = Tab2_TSV[index, t_index[1]] - Tab2_TSV[index, t_index[2]];
            
            // Log Message
            if (Tab2_HideResourceFunction.Checked != true)
            {
                log_mess = "Time Span[" + (t_index[0]+ 1) + "] = TSV[" + (t_index[1]+1) + "] - TSV[" + (t_index[2]+1) + "]= ";
                log_mess += Tab2_TimeSpend[index, t_index[0]];
                Tab2_add_log(index, log_mess + "\n", LogMsgType.Normal);
            }
            return true;
        }

        private bool Print_String(int index, string command_str)
        {
            int len, res_index;
            int[] t_index = new int[3];
            string cmd;
            string[] tag;
            string option = "";
            string log_mess = "";
            bool ret_var = false;

            len = command_str.Length;
            if (command_str == "") return false;
            if (command_str[0] != ':') return false;

            cmd = command_str.Substring(1, len - 1).Trim();
            len = cmd.Length;
            tag = cmd.Split(':');

            switch (tag[0])
            {
                case "$RCV":
                    log_mess = Convert_Bytes_to_String(Tab2_Receive_Buf[index],0,Tab2_Receive_index[index]);
                    ret_var = true;
                    break;
                default:
                    if (tag[0][0] == '$')
                    {
                        if (len != 3) return false;
                        if ((tag[0][2] >= '1') && (tag[0][2] <= '1' + MAX_RESOURCE))
                        {
                            res_index = tag[0][2] - '1';
                        }
                        else return false;

                        switch (tag[0][1])
                        {
                            case 'S':
                                log_mess = Tab2_SSV[index, res_index].ToString();
                                ret_var = true;
                                break;
                            case 'T':
                                if (tag.Length == 1) option = "T";
                                if (tag.Length == 2)
                                {
                                    option = tag[1];
                                }
                                switch (option)
                                {
                                    case "F":
                                        log_mess = Tab2_TSV[index, res_index].ToString("dd/MM/yyyy-HH:mm:ss.fff");
                                        log_mess += Convert.ToString(Tab2_TSV[index, res_index].Millisecond);
                                        ret_var = true;
                                        break;
                                    case "D":
                                        log_mess = Tab2_TSV[index, res_index].ToString("dd/MM/yyyy");
                                        ret_var = true;
                                        break;
                                    case "T":
                                        log_mess = Tab2_TSV[index, res_index].ToString("HH:mm:ss.fff");
                                        log_mess += Convert.ToString(Tab2_TSV[index, res_index].Millisecond);
                                        ret_var = true;
                                        break;
                                    default:
                                        return false;
                                }
                                break;
                            case 'P':
                                log_mess = Tab2_TimeSpend[index, res_index].ToString();
                                ret_var = true;
                                break;
                            default:
                                return false;
                        }
                    }
                    else
                    {
                        log_mess = Change_HexString2String(cmd);
                        ret_var = true;
                    }
                    break;
            }
            if (log_mess != "")
            {
                Tab2_add_log(index, log_mess, LogMsgType.Normal);
            }
            return ret_var;
        }

        private bool Report_Pass_Fail(int index)
        {
            DateTime cur_time = DateTime.Now;
            TimeSpan time_span = cur_time - Tab2_Start[index];
            string log_mess;

            log_mess = "/*****************************************************************************************************************\n";
            log_mess += " * $Port Name   : " + ComControlArray[index].ComPort.PortName.ToString() + "\n";
            log_mess += " * $Device      : " + ComControlArray[index].DeviceNameText.Text + "\n";
            log_mess += " * $Report at   : " + cur_time.ToString("dd/MM/yyyy-HH:mm:ss") + "\n";
            log_mess += " * $Start time  : " + Tab2_Start[index].ToString("dd/MM/yyyy-HH:mm:ss") + "\n";
            log_mess += " * $Duration    : " + time_span.ToString() + "\n";
            log_mess += " * $Total Pass  : " + Pass_Cnt[index].ToString() + "\n";
            log_mess += " * $Total Fail  : " + Fail_Cnt[index].ToString() + "\n";
            log_mess += " *****************************************************************************************************************/\n";
            Tab2_add_log(index, log_mess, LogMsgType.Outgoing);
            return true;
        }

        private bool Display_Pic(int index, string command_str)
        {
            return true;
        }
    }
}
