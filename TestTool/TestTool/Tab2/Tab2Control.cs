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
    partial class Test_Form
    {
        #region Public Enumerations for Tab2
        public int Auto_Save;
        public byte totalPort;
        public bool Tab2Running = false;
        public bool[] First_Receive;
        public bool[] Has_data;
        public int[] Delay_index;
        public int[] Data_index;
        public DateTime[] Tab2_Start;
        public DateTime[] Tab2_First_Send;
        public DateTime[] Tab2_Last_Send;
        public DateTime[] Tab2_Curr_Send;
        public DateTime[] Tab2_First_Receive;
        public DateTime[] Tab2_Last_Receive;
        public DateTime[] Tab2_Curr_Receive;
        public DateTime[] Tab2_Stop;
        const int DELAY_TIME = 5000;
        public string[,] Tab2_Expect_Respond;
        public string[,] Tab2_Wait_Respond;
        public string[,] Tab2_Expect_Send;
        public string[,] Tab2_Wait_Send;
        
        public int[] Expect_Cnt;
        public int[] Wait_Cnt;
        Random random = new Random();
        #endregion

        #region Check Frame
        private bool[] Collect_Frame;
        public byte[,] Tab2_Frame_Receive;
        public int[] Tab2_FReceive_Len;
        public byte[][] Tab2_Receive_Buf;
        public int[] Tab2_Receive_index;
        private int[] Pream_Cnt;
        public int[] Frame_Expect_Cnt;
        public bool[] Check_Frame_Ena;
        public UInt16 CrcAccumulator = 0;
        public UInt16[] crcTable1021 = new UInt16[256] {
                0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7,
                0x8108, 0x9129, 0xA14A, 0xB16B, 0xC18C, 0xD1AD, 0xE1CE, 0xF1EF,
                0x1231, 0x0210, 0x3273, 0x2252, 0x52B5, 0x4294, 0x72F7, 0x62D6,
                0x9339, 0x8318, 0xB37B, 0xA35A, 0xD3BD, 0xC39C, 0xF3FF, 0xE3DE,
                0x2462, 0x3443, 0x0420, 0x1401, 0x64E6, 0x74C7, 0x44A4, 0x5485,
                0xA56A, 0xB54B, 0x8528, 0x9509, 0xE5EE, 0xF5CF, 0xC5AC, 0xD58D,
                0x3653, 0x2672, 0x1611, 0x0630, 0x76D7, 0x66F6, 0x5695, 0x46B4,
                0xB75B, 0xA77A, 0x9719, 0x8738, 0xF7DF, 0xE7FE, 0xD79D, 0xC7BC,
                0x48C4, 0x58E5, 0x6886, 0x78A7, 0x0840, 0x1861, 0x2802, 0x3823,
                0xC9CC, 0xD9ED, 0xE98E, 0xF9AF, 0x8948, 0x9969, 0xA90A, 0xB92B,
                0x5AF5, 0x4AD4, 0x7AB7, 0x6A96, 0x1A71, 0x0A50, 0x3A33, 0x2A12,
                0xDBFD, 0xCBDC, 0xFBBF, 0xEB9E, 0x9B79, 0x8B58, 0xBB3B, 0xAB1A,
                0x6CA6, 0x7C87, 0x4CE4, 0x5CC5, 0x2C22, 0x3C03, 0x0C60, 0x1C41,
                0xEDAE, 0xFD8F, 0xCDEC, 0xDDCD, 0xAD2A, 0xBD0B, 0x8D68, 0x9D49,
                0x7E97, 0x6EB6, 0x5ED5, 0x4EF4, 0x3E13, 0x2E32, 0x1E51, 0x0E70,
                0xFF9F, 0xEFBE, 0xDFDD, 0xCFFC, 0xBF1B, 0xAF3A, 0x9F59, 0x8F78,
                0x9188, 0x81A9, 0xB1CA, 0xA1EB, 0xD10C, 0xC12D, 0xF14E, 0xE16F,
                0x1080, 0x00A1, 0x30C2, 0x20E3, 0x5004, 0x4025, 0x7046, 0x6067,
                0x83B9, 0x9398, 0xA3FB, 0xB3DA, 0xC33D, 0xD31C, 0xE37F, 0xF35E,
                0x02B1, 0x1290, 0x22F3, 0x32D2, 0x4235, 0x5214, 0x6277, 0x7256,
                0xB5EA, 0xA5CB, 0x95A8, 0x8589, 0xF56E, 0xE54F, 0xD52C, 0xC50D,
                0x34E2, 0x24C3, 0x14A0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405,
                0xA7DB, 0xB7FA, 0x8799, 0x97B8, 0xE75F, 0xF77E, 0xC71D, 0xD73C,
                0x26D3, 0x36F2, 0x0691, 0x16B0, 0x6657, 0x7676, 0x4615, 0x5634,
                0xD94C, 0xC96D, 0xF90E, 0xE92F, 0x99C8, 0x89E9, 0xB98A, 0xA9AB,
                0x5844, 0x4865, 0x7806, 0x6827, 0x18C0, 0x08E1, 0x3882, 0x28A3,
                0xCB7D, 0xDB5C, 0xEB3F, 0xFB1E, 0x8BF9, 0x9BD8, 0xABBB, 0xBB9A,
                0x4A75, 0x5A54, 0x6A37, 0x7A16, 0x0AF1, 0x1AD0, 0x2AB3, 0x3A92,
                0xFD2E, 0xED0F, 0xDD6C, 0xCD4D, 0xBDAA, 0xAD8B, 0x9DE8, 0x8DC9,
                0x7C26, 0x6C07, 0x5C64, 0x4C45, 0x3CA2, 0x2C83, 0x1CE0, 0x0CC1,
                0xEF1F, 0xFF3E, 0xCF5D, 0xDF7C, 0xAF9B, 0xBFBA, 0x8FD9, 0x9FF8,
                0x6E17, 0x7E36, 0x4E55, 0x5E74, 0x2E93, 0x3EB2, 0x0ED1, 0x1EF0,
            };
        #endregion

        #region Public Resouce for Serial Test Tool
        const int MAX_RESOURCE = 6;
        const int MAX_BUF_LEN = 2000;
        const int BUF_LEN = 500;
        public DateTime[,] Tab2_TSV;
        public TimeSpan[,] Tab2_TimeSpend;
        public string[,] Tab2_SSV;
        public DateTime[] Tab2_RCT;
        public DateTime[] Tab2_TCT;
        public long[] Tab2_SendCnt;
        public long[] Pass_Cnt;
        public long[] Fail_Cnt;
        public int [] Delay_Value;
        #endregion

        #region COMPORT Status
        public Tab2Stauts []Tab2_Status;
        public int []Tab2_Run_Step;
        #endregion

        #region Resource for Goto function (%GTO & $LBL)
        public int[,] Tab2_LBL;

        #endregion

        private void Tab2Init()
        {
            int i, j;
            Tab2LogPathText.Text = "C:";
            Tab2DataViewMode = DataMode.Text;
            Tab2ViewTextOp.Checked = true;
            Tab2RunOneTimeMode.Checked = true;
            Auto_Save = 0;

            // init time check value
            Tab2_Start = new DateTime[totalPort];
            Tab2_First_Send = new DateTime[totalPort];
            Tab2_Last_Send = new DateTime[totalPort];
            Tab2_Curr_Send = new DateTime[totalPort];
            Tab2_First_Receive = new DateTime[totalPort];
            Tab2_Last_Receive = new DateTime[totalPort];
            Tab2_Curr_Receive = new DateTime[totalPort];
            Tab2_Stop = new DateTime[totalPort];
            First_Receive = new bool[totalPort];
            Has_data = new bool[totalPort];
            FileOut = new StreamWriter[totalPort];
            
            Tab2_RCT = new DateTime[totalPort];
            Tab2_TCT = new DateTime[totalPort];

            Tab2_Expect_Respond = new string[totalPort, MAX_RESOURCE];
            Tab2_Wait_Respond = new string[totalPort, MAX_RESOURCE];
            Tab2_Expect_Send = new string[totalPort, MAX_RESOURCE];
            Tab2_Wait_Send = new string[totalPort, MAX_RESOURCE];
            Expect_Cnt = new int[totalPort];
            Wait_Cnt = new int[totalPort];

            // Check frame
            Tab2_Frame_Receive = new byte[totalPort, MAX_BUF_LEN];
            Tab2_FReceive_Len = new int[totalPort];
            Tab2_Receive_Buf = new byte[totalPort][];
            Tab2_Receive_index = new int[totalPort];
            Frame_Expect_Cnt = new int[totalPort];
            Check_Frame_Ena = new bool[totalPort];
            Collect_Frame = new bool[totalPort];
            Pream_Cnt = new int[totalPort];


            // Goto Function
            Tab2_LBL = new int[totalPort, MAX_RESOURCE];

            // Init for Serial Test Tool
            Tab2_TSV = new DateTime[totalPort, MAX_RESOURCE];
            Tab2_TimeSpend = new TimeSpan[totalPort, MAX_RESOURCE];
            Tab2_SSV = new string[totalPort, MAX_RESOURCE];
            Tab2_SendCnt = new long [totalPort];
            Pass_Cnt = new long[totalPort];
            Fail_Cnt = new long[totalPort];
            Tab2_Status = new Tab2Stauts [totalPort];
            Tab2_Run_Step = new int[totalPort];
            Delay_Value = new int[totalPort];

            // init global value 
            for (i = 0; i < totalPort; i++)
            {
                First_Receive[i] = true;
                Has_data[i] = false;
                Tab2_FReceive_Len[i] = 0;
                Tab2_Receive_Buf[i] = new byte[MAX_BUF_LEN];
                Tab2_Receive_index[i] = 0;
                Tab2_Status[i] = Tab2Stauts.Init;
                FileOut[i] = null;

                // Check Frame
                Check_Frame_Ena[i] = false;
                Collect_Frame[i] = false;
                Pream_Cnt[i] = 0;

                Tab2_Run_Step[i] = 0;
                Wait_Cnt[i] = 0;
                Expect_Cnt[i] = 0;
                for (j = 0; j < MAX_RESOURCE; j++)
                {
                    Tab2_Expect_Respond[i, j] = "";
                    Tab2_Wait_Respond[i, j] = "";
                    Tab2_Expect_Send[i, j] = "";
                    Tab2_Wait_Send[i, j] = "";
                    Tab2_LBL[i, j] = 0;
                }
            }
            Tab2_Update_Status(Tab2Stauts.Init);
        }

        /********************************************************************************
         * 
         *    #####    ######   ##     ##      ######    ######   ######  ########
            ##        ##    ##  ###   ###      #     #  #     ##  #     #     #   
            #        ##      ## # #   # #      #    ## #       #  # ## ##     #   
            #        ##      ## #  # ## #      ######  #       #  #######     #   
            ##        #      #  ## # #  #      #        #     ##  #     #     #   
              #####    ######   ## ###  #      #         ######   #     #     #   
         * 
         ********************************************************************************/

        private void Tab2ComPortInit(Tab2ComPort[] ComControlArray, int index, string PortName)
        {
            int x, y, i;

            i = index;
            x = 0;
            y = 23 * i;
            ComControlArray[i] = new Tab2ComPort();
            ComControlArray[i].Class_Init(x, y, index);

            //Config ComPort
            ComControlArray[i].ComCheckBox.Text = PortName;
            ComControlArray[i].ComPort.PortName = PortName;
            ComControlArray[i].DelayValueText.Text = "500";

            // Event Handle
            // ComControlArray[i].ComCheckBox.CheckStateChanged += new System.EventHandler(ComCheck_eventhandler);
            // ComControlArray[i].DeviceNameText.TextChanged += new System.EventHandler(DeviceName_evenhandler);
            ComControlArray[i].SelectPathBT.Click += new EventHandler(SelectPathBT_Click);
            ComControlArray[i].ComPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(Tab2SerialPort_ISR);
            ComControlArray[i].ComTimer.Tick += new System.EventHandler(Timer_ISR);
            ComControlArray[i].DelayValueText.TextChanged += new EventHandler(DelayValueText_TextChanged);

            // Add to Form
            Tab2groupComSet.Controls.Add(ComControlArray[i].ComCheckBox);
            Tab2groupComSet.Controls.Add(ComControlArray[i].DeviceNameText);
            Tab2groupComSet.Controls.Add(ComControlArray[i].DelayValueText);
            // Tab2groupComSet.Controls.Add(ComControlArray[i].FixTimeCheck);
            Tab2groupComSet.Controls.Add(ComControlArray[i].SelectPathBT);
            Tab2groupComSet.Controls.Add(ComControlArray[i].DataforSendLabel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool WriteCom(int index, byte[] data_ptr, int len)
        {
            string mess;
            string dats_str;
            DateTime current_time = DateTime.Now;

            dats_str = Convert_Bytes_to_String(data_ptr, 0, len);
            // Check correct respond
            if (ComControlArray[index].ComPort.IsOpen == true)
            {
                // Update Time Check
                ComControlArray[index].Write_data(data_ptr, len);
                Tab2_Last_Send[index] = Tab2_Curr_Send[index];
                Tab2_Curr_Send[index] = current_time;
                Tab2_RCT[index] = current_time;
                Tab2_SendCnt[index]++;

                if (Tab2_First_Send[index] == Tab2_Start[index])
                {
                    Tab2_First_Send[index] = Tab2_Curr_Send[index];
                    Tab2_Last_Send[index] = Tab2_Curr_Send[index];
                }

                // Add to log
                mess = FormatData(dats_str, DataType.Send, Tab2_Curr_Send[index], TabNum.Tab2, false);
                First_Receive[index] = true;
                mess = "\n" + ComControlArray[index].ComPort.PortName.ToString() + ":" + mess + "\n";
                Tab2_add_log(index, mess, LogMsgType.Outgoing);

                return true;
            }
            else
            {
                mess = ComControlArray[index].ComPort.PortName.ToString() + " is Closed" + "\n";
                mess = FormatData("Comport", DataType.Send, DateTime.Now, TabNum.Tab2, false); // type: 0 is output
                Tab2_add_log(index, mess, LogMsgType.Error);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        private void GetTab2SerialConfig(int index)
        {
            ComControlArray[index].ComPort.BaudRate = int.Parse(Tab2SetBaudrate.Text);
            ComControlArray[index].ComPort.DataBits = int.Parse(Tab2SetDataBit.Text);
            ComControlArray[index].ComPort.Parity = (Parity)Enum.Parse(typeof(Parity), Tab2SetParity.Text);
            ComControlArray[index].ComPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), Tab2SetStopBit.Text);
            ComControlArray[index].ComPort.ReceivedBytesThreshold = int.Parse(Tab2setThreshold.Text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enable"></param>
        public bool Tab2_Update_Status(Tab2Stauts status)
        {
            bool enable;

            switch (status)
            {
                case Tab2Stauts.Stop:
                case Tab2Stauts.Init:
                    Tab2Stop.Enabled = false;
                    Tab2Pause.Enabled = false;
                    Tab2RunBT.Enabled = true;
                    Tab2SigStep.Enabled = true;
                    Tab2MulStep.Enabled = true;
                    Tab2_Step_Num.Enabled = true;
                    enable = true;
                    break;
                case Tab2Stauts.Run:
                    Tab2Stop.Enabled = true;
                    Tab2Pause.Enabled = true;
                    Tab2RunBT.Enabled = false;
                    Tab2SigStep.Enabled = false;
                    Tab2MulStep.Enabled = false;
                    Tab2_Step_Num.Enabled = false;
                    enable = false;
                    break;
                case Tab2Stauts.Pause:
                    Tab2Stop.Enabled = true;
                    Tab2Pause.Enabled = false;
                    Tab2RunBT.Enabled = true;
                    Tab2SigStep.Enabled = true;
                    Tab2MulStep.Enabled = true;
                    Tab2_Step_Num.Enabled = true;
                    enable = false;
                    break;
                default:
                    return false;
            }
            Tab2SetBaudrate.Enabled = enable;
            Tab2SetDataBit.Enabled = enable;
            Tab2SetParity.Enabled = enable;
            Tab2SetStopBit.Enabled = enable;
            Tab2setThreshold.Enabled = enable;
            Tab2ViewHexOp.Enabled = enable;
            Tab2ViewTextOp.Enabled = enable;
            Tab2LogPathText.Enabled = enable;
            Tab2RefeshBT.Enabled = enable;
            Tab2RestoreDefaultBT.Enabled = enable;
            Tab2LogBT.Enabled = enable;
            return true;
        }

        //
        /***************************************************************************
         * 
         *    ######   ######## ########     #######      ###   #########   ###   
            ##     ##  #           #         ##    ##    ## #       #       # ##  
            #          #######     ##        ##     ##  ##  ##      #      ##  #  
            #    ####  ##          ##        ##     ##  #    #      #      #   ## 
            ##      #  ##          ##        ##     #  ########     #     ####### 
              ##### #  ########    ##        #######   #      ##    #    ##      #
         * 
         ***************************************************************************/
        public int Get_Data(int index, ref byte[] data_ptr)
        {
            string data = "";
            // string data_send = "";
            string log_mess;
            int Totalitem;
            int currItemIndex;
            int len = 0;

            // Get data from list
            Totalitem = ComControlArray[index].Data4Send.Items.Count;
            currItemIndex = Data_index[index];
            Delay_Value[index] = 0;
            Reset_Buffer(index);
            while (Delay_Value[index] == 0)
            {
                if (ComControlArray[index].Data4Send.Items.Count == 0)
                {
                    log_mess = ComControlArray[index].ComPort.PortName.ToString();
                    log_mess += ": Not load script file.\n";
                    Tab2_add_log(index, log_mess, LogMsgType.Coment);
                    Data_index[index] = 0;
                    Delay_Value[index] = 1000;
                }
                else
                {
                    data = ComControlArray[index].Data4Send.Items[Data_index[index]].ToString();
                    data.Trim();
                    if (data != "")
                    {
                        len += Advance_format_data(index, data, ref data_ptr);
                    }

                    // increase index for next time
                    Data_index[index]++;
                    if (Data_index[index] >= Totalitem)
                    {
                        if (Tab2ForeverMode.Checked == true)
                        {
                            Data_index[index] = 0;
                        }
                        else
                        {
                            Data_index[index] = 0;
                            Delay_Value[index] = 1000;
                            // Tab2_Status[index] = Tab2Stauts.Pause;
                            Tab2Pause_Click(null, null);
                            log_mess = ComControlArray[index].ComPort.PortName.ToString();
                            log_mess += ": Complete Run Scrip File. Pause for all other Port.\n";
                            Tab2_add_log(index, log_mess, LogMsgType.Coment);
                        }
                    }
                }
            }
            Update_Pass_Fail();
            return len;
        }

        private void Write_log_file(StreamWriter file_ptr, String text, LogMsgType type)
        {
            // Write the alphabet to the file.
            try
            {
                file_ptr.Write(text);
            }
            catch (IOException)
            {
                MessageBox.Show("Can not write file" + file_ptr);
            }
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
        private void Tab2_ResetTimeCheck(int index, DateTime current_time)
        {
            current_time = DateTime.Now;
            Tab2_Start[index] = current_time;
            Tab2_First_Send[index] = current_time;
            Tab2_Last_Send[index] = current_time;
            Tab2_Curr_Send[index] = current_time;
            Tab2_First_Receive[index] = current_time;
            Tab2_Last_Receive[index] = current_time;
            Tab2_Curr_Receive[index] = current_time;
            Tab2_Stop[index] = current_time;
            Tab2_RCT[index] = current_time;
            Tab2_TCT[index] = current_time;

            Tab2_SendCnt[index] = 0;
            Pass_Cnt[index] = 0;
            Fail_Cnt[index] = 0;
        }
        /// <summary>
        /// Name: 
        /// </summary>
        /// <param name="mode"></param>1: transaction, 0: Run Time
        private void Reset_Buffer(int index)
        {
            int i;
            for (i = 0; i < MAX_RESOURCE; i++)
            {
                Tab2_Expect_Respond[index, i] = "";
                Tab2_Wait_Respond[index, i] = "";
                Tab2_Expect_Send[index, i] = "";
                Tab2_Wait_Send[index, i] = "";
            }
            Wait_Cnt[index] = 0;
            Expect_Cnt[index] = 0;
            Tab2_Receive_index[index] = 0;
            Delay_Value[index] = 0;
        }
        

        /*****************************************************************************************************
         #######  ########### #             ########      ###     ########    #######   #########  ######## 
         #     ##      #      #             #      ##    ## #     #      ##  ##     ##  ##         #      ##
         ##            #      #             #      ##   ##  ##    #      ##  ##         ##     #   #      # 
          #######      #      #             ########    #    ##   ########    #######   ########   ######## 
                ##     #      #             #          ##    ##   #      ##         ##  ##         #      # 
        ##      ##     #      #             #         ##########  ##     ## ##      ##  ##         ##     ##
         ########      #      ########      #         ##       #  ##     ##   #######   #########  ##     ## 

        *****************************************************************************************************/
        /// <summary>
        /// If the first charapter: 
        ///     + ';'       : Comment : Ignore this line
        ///     + '$'       : Command : 
        ///     + No Prefix : Data for send. check Expect receive
        ///     + "\x"      : Hex value
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private int Advance_format_data(int index, string input_data, ref byte[] outdata_ptr)
        {
            string log_mess;
            int in_len, out_len = 0;

            in_len = input_data.Length;
            // check correct data
            if (input_data == "") return 0;

            switch (input_data[0])
            {
                case ';':   // Comment in Script File: Ignore this line
                    break;
                case '%':   // change configure comport
                    if (Parser_command(index, input_data.Substring(1, in_len - 1)) == false)
                    {
                        log_mess = "Command: [" + input_data + "] at line: " + (Data_index[index] + 1) + " ==> Error\n";
                        Tab2_add_log(index, log_mess, LogMsgType.Error);
                    }
                    break;
                default:
                    out_len = Change_Text2Bytes(input_data, ref outdata_ptr);
                    break;
            }

            return out_len;
        }

        /********************************************************************************************************
               #####  #    #  # ####  ###### #   ##     ######  # ####  #####  ###### ## #    ## # ####     
              #     # #    #  #      #     # # ##       #    #  #      #     # #      ## ##   #  #          
             ##       # ## #  # ###  #       ####       ######  # ###  #       # ###  ##  #  ##  # ###      
              ##   ## #    #  #      ##    # #   ##     #    #  #      ##   ## #      ##   ###   #          
               ####   #    #  ######   ####  #    ##    #    #  ######   ####  ###### ##   ##    ######     
                                                                                                                    
                                                                                                                    
           ##     ####  ###### ##   ####   #     #     #####  ######  ####   #####    ####   ##    #  ####  
          ###   ##   ##   ##   ## ##   ##  ###  ##    ##   ## #      ##   #  #    # ##   ##  ###   # ##   ##
         ## ##  #         ##   ## #     ## # ##  #    ######  ######  ####   # # ## #      # # ##  # ##    #
         #####  #     #   ##   ## #     ## #  ## #    ##   #  #           ## # ##   #     ## #  ## #  #    #
        #     #  ######   ##   ##  ######  #    ##    ##   ## ###### ######  #       ######  #    ##  ##### 

         ********************************************************************************************************/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool Check_Wait_Receive_Data(int index)
        {
            int i;
            string log_mess;
            string wait_string;
            DateTime curr;
            byte []bufer_ptr = Tab2_Receive_Buf[index];


            if (Wait_Cnt[index] != 0)
            {
                for (i = 0; i < Wait_Cnt[index] + 1 ; i++)
                {
                    if (Check_Data_Match_Patten(Tab2_Receive_Buf[index], Tab2_Receive_index[index], Tab2_Wait_Respond[index, i]) == true)
                    {
                        if (Tab2_Wait_Respond[index, i] == "")
                        {
                            //@Note (Kien ##): Not support for wait NULL data
                            return false;
                        }
                        else
                        {
                            // Write Log Message: PASS
                            log_mess = "\n" + ComControlArray[index].ComPort.PortName.ToString();
                            curr = DateTime.Now;
                            log_mess += " <" + curr.ToString("hh:mm:ss,");
                            log_mess += curr.Millisecond.ToString() + "> ";
                            wait_string = Change_CRLF(Tab2_Wait_Respond[index, i]);
                            log_mess += "Wait Data: [" + wait_string + "] :PASS \n";
                            Tab2_add_log(index, log_mess, LogMsgType.Coment);
                            Wait_Cnt[index] = 0;
                            Pass_Cnt[index]++;

                            Respond_Action(index, Tab2_Wait_Send[index, i]);
                            break;
                        }
                    }
                }
            }
            return true;
        }  
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool Check_Expect_Receive_Data(int index)
        {
            int i;
            string log_mess;
            string receive_str = Convert_Bytes_to_String(Tab2_Receive_Buf[index], 0, Tab2_Receive_index[index]);
            string wait_string;

            // Check for Wait data
            if (Wait_Cnt[index] != 0)
            {
                // Write Log Message: FAIL
                log_mess = "Wait Data: \n"; // +Tab2_Receive_Buf[index] + " :PASS \n";
                for (i = 0; i < Wait_Cnt[index]; i++) {
                    wait_string = Change_CRLF(Tab2_Wait_Respond[index, i]);
                    log_mess += "    " + i + ". [" + wait_string + "]\n";
                }
                log_mess += "    ==> FAIL\n";
                Fail_Cnt[index]++;
                Tab2_add_log(index, log_mess, LogMsgType.Error);
            }

            // Check Expect Receive:
            if (Expect_Cnt[index] != 0)
            {
                for (i = 0; i < Expect_Cnt[index]; i++)
                {
                    if (Check_Data_Match_Patten(Tab2_Receive_Buf[index], Tab2_Receive_index[index], Tab2_Expect_Respond[index, i]) == true)
                    {
                        if (Tab2_Expect_Respond[index, i] == "")
                        {
                            // Write Log Message
                            log_mess = "\nExpected: No Receive\n";
                            log_mess += "Meet with Expect Data: PASS";
                            Tab2_add_log(index, log_mess, LogMsgType.Coment);


                            // Action for match with expect
                            // WriteCom(index, Tab2_Expect_Send[index, i]);
                            Respond_Action(index, Tab2_Expect_Send[index, i]);
                            Pass_Cnt[index]++;
                            break;
                        }
                        else
                        {
                            // Write Log Message
                            log_mess = "\nReceive Data: " + receive_str + "\n";
                            log_mess += "Meet with Expect Data: PASS\n";
                            Tab2_add_log(index, log_mess, LogMsgType.Coment);


                            // Action for match with expect
                            // WriteCom(index, Tab2_Expect_Send[index, i]);
                            Respond_Action(index, Tab2_Expect_Send[index, i]);
                            Pass_Cnt[index]++;
                            break;
                        }
                    }
                }

                // Report Fail
                if (i == Expect_Cnt[index])
                {
                    // Write Log Message: FAIL
                    log_mess = "Expect Data: \n";
                    for (i = 0; i < Expect_Cnt[index]; i++)
                    {
                        log_mess += "    " + i + ". [" + Tab2_Expect_Respond[index, i] + "]\n";
                    }
                    log_mess += "    ==> FAIL\n";
                    Tab2_add_log(index, log_mess, LogMsgType.Error);
                    Fail_Cnt[index]++;

                }
            }
            Expect_Cnt[index] = 0;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="patten"></param>
        /// <returns></returns>
        private bool Check_Data_Match_Patten(byte []input_data, int len, string patten)
        {
            bool ret_var = false;
            string cur_patten;
            string input_str;
            byte[] patten_bytes = new byte [BUF_LEN];
            int patten_len, i;

            patten = patten.Trim();
            
            if (patten == "")
            {
                // @TODO (Kien ##): Check for expect no respond
                if (len == 0)
                {
                    return true;
                }

            }else{
                cur_patten = patten;
                switch (cur_patten[0])
                {
                    case 'R':
                        input_str = Convert_Bytes_to_String(input_data, 0, len);
                        cur_patten = cur_patten.Substring(1,cur_patten.Length -1);
                        // cur_patten = Change_HexString2String(cur_patten);
                        if (Regex.IsMatch(input_str, cur_patten))
                        {
                            ret_var = true;
                        }
                        break;
                    case 'T':                       
                        cur_patten = cur_patten.Substring(1,cur_patten.Length -1);
                        //@FIXME (Kien #1#): Need to compare in string
                        patten_len = Change_Text2Bytes(cur_patten, ref patten_bytes);
                        if (patten_len == len)
                        {
                            ret_var = true;
                            for (i = 0; i < len; i++)
                            {
                                if (input_data[i] != patten_bytes[i])
                                {
                                    ret_var = false;
                                    break;
                                }
                            }
                        }
                        break;
                    default:
                            ret_var = false;
                        break;
                }
            }
            return ret_var;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private bool Respond_Action(int index, string action)
        {
            bool ret_var = false;
            byte[] write_buf = new byte[BUF_LEN];
            int write_len;
            string send_data;
            string log_mess;
            string a = "";
            send_data = a.Trim();

            action = action.Trim();
            if (action == "") return false;

            if (action[0] == '$') 
            {
                log_mess = "Action Respond: " + action + "\n";
                Tab2_add_log(index, log_mess, LogMsgType.Normal);
                // @TODO: Tool Action
                ret_var = true;
                switch (action)
                {
                    case "RTS:ON":
                        ComControlArray[index].ComPort.RtsEnable = true;
                        break;
                    case "RTS:OFF":
                        ComControlArray[index].ComPort.RtsEnable = false;
                        break;
                    case "$DTR:ON":
                        ComControlArray[index].ComPort.DtrEnable = true;
                        break;
                    case "$DTR:OFF":
                        ComControlArray[index].ComPort.DtrEnable = false;
                        break;
                    case "$ONP":
                        if (ComControlArray[index].ComPort.IsOpen == false)
                        {
                            ComControlArray[index].OpenPort();
                        }
                        break;
                    case "OFF":
                        if (ComControlArray[index].ComPort.IsOpen == true)
                        {
                            ComControlArray[index].ClosePort();
                        }
                        break;
                    case "$PAU":
                        Tab2Pause_Click(null, null);
                        break;
                    case "$STP":
                        Tab2Stop_Click(null, null);
                        break;
                    default:
                        ret_var = false;
                        break;
                }
            }
            // For "%GTO" function
            else if ((action.Length == 5) && (action.Substring(0, 4) == "%GTO") &&
                        ((action[4] >= '1') && (action[4] <= '6'))) 
            {
                Data_index[index] = Tab2_LBL[index, action[4] - '1'];
                ret_var = true;
            }
            else
            {
                write_len = Change_Text2Bytes(action, ref write_buf);
                WriteCom(index, write_buf, write_len);
                ret_var = true;
            }

            return ret_var;
        }

        /**************************************************************************
                  ######   #      #   ########   ######   #    ## 
                ##      ## #      #   #        ##      ## #  ##   
                #          ########   #######  #          ###     
                #          #      #   #        #          #  ##   
                ##      ## #      #   #        ##      ## #   ##  
                 #######   ##     #  #########   ######  ##     ##
                                                                  
                ######## ########     ###     ##     ###  ########
                #         #     #     # ##    ###    ###  #       
                #######   # ## ##    ##  #    # #   ####  ####### 
                ##        # #####   ##   ##   # ##  # ##  #       
                ##        #     #   ########  #  # ## ##  #       
                ##        #     ## ##      #  #  ###  ##  ########
        **************************************************************************/

        /// <summary>
        /// Name:       Tab2_RxFrame
        /// Function:   Collect receive data into frame. 
        ///             Look for EOF: <CR>
        ///             
        /// </summary>
        /// <param name="index"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool Tab2_RxFrame(int index, byte[] input, int input_len)
        {
            int i;
            bool retvar = false;
            int len;
            int x;

            // check for end of frame: look for <CR>
            for (i = 0; i < input_len; i++)
            {
                if (Collect_Frame[index] == false)
                {
                    // Check Preamble
                    if (Pream_Cnt[index] < 4) {
                        if (input[i] == '\r'){
                            Pream_Cnt[index]++;
                        }
                        else 
                        {
                            Pream_Cnt[index] = 0;
                        }
                    }
                    else {
                        if (input[i] != '\r')
                        {
                            // @NOTE (Kien ##): Start of Frame also is Byte High of Length
                            Pream_Cnt[index] = 0;
                            Tab2_FReceive_Len[index] = 0;
                            Collect_Frame[index] = true;
                            Tab2_Frame_Receive[index, Tab2_FReceive_Len[index]] = input[i];
                            Tab2_FReceive_Len[index]++;
                        }
                    }
                }
                else {
                    Tab2_Frame_Receive[index, Tab2_FReceive_Len[index]] = input[i];
                    Tab2_FReceive_Len[index]++;
                }
            }

            //Check Len to get End Of Frame
            if (Tab2_FReceive_Len[index] > 2)
            {
                x = (int)Tab2_Frame_Receive[index,0] << 8;
                x = (int)Tab2_Frame_Receive[index,1];
                len = ((int)Tab2_Frame_Receive[index,0] << 8) + (int)Tab2_Frame_Receive[index,1]; ;
                if (len <= Tab2_FReceive_Len[index])
                {
                    Tab2_FReceive_Len[index] = len;
                    retvar = Tab2_Check_Frame(index);        // Call to L2 Check Frame
                    Collect_Frame[index] = false;
                    Pream_Cnt[index] = 0;
                }
            }

            return retvar;
        }

        /// <summary>
        /// Name:       Tab2_Check_Frame
        /// Function:   Check correct frame 
        /// 
        /// +------------------+-----------+-----------+----------+-----------+-----------+-----------+-----------+
        /// |  PREAMBLE      |           |           |                                  |           |           |
        /// | <CR><CR><CR><CR> | LEN       | COUNTER   |             PAY LOAD             | CRC       | EOF       |
        /// | 4 bytes          | 2 bytes   | 1 byte    |                                  | 2 bytes   | <CR>      |
        /// +------------------+-----------+-----------+----------+-----------+-----------+-----------+-----------+
        /// 
        /// Note: Preaimble and EOF not count in len
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        ///
        private bool Tab2_Check_Frame(int index)
        {
            byte [] cur_frame = new byte[MAX_BUF_LEN];
            int len = Tab2_FReceive_Len[index];
            int frame_len;
            int frame_cnt;
            int frame_crc;
            int i;
            string frame_str, frame_payload;
            string frame_type;
            string error_mess;
            string log_mess;
            bool retvar = false;


            // Get Frame information
            for (i = 0; i < len; i++)
            {
                cur_frame[i] = Tab2_Frame_Receive[index, i];
            }
            frame_str = Convert_Bytes_to_String(cur_frame, 0, len);
            // frame_str = Tab2_Frame_Receive[index].Substring(0, len - 2);
            frame_len = ((int)cur_frame[0] << 8) + (int)cur_frame[1];
            frame_cnt = (int)cur_frame[2];
            frame_payload = Convert_Bytes_to_String(cur_frame, 3, len);
            frame_crc = ((int)cur_frame[len - 2] << 8) + (int)cur_frame[len - 1];
            Tab2_FReceive_Len[index] = 0;       // Reset Buffer

            // Check Frame
            if ((frame_payload.Length >6 ) && ((frame_payload.Substring(1,6) == "$+RSTG") ||(frame_payload.Substring(1,6) == "$+RSTB")))
            {
                frame_type = "Reset Frame";
                // Frame Reset from Base or Gun
                if ((frame_cnt == 0) && (frame_crc == Get_CRC(cur_frame, len-2)) && (frame_len == len))
                {
                    Frame_Expect_Cnt[index] = 0;
                    error_mess = "Syntax Good";
                    retvar = false;
                }
                else
                {
                    error_mess = "Syntax Fail";
                    retvar = false;
                }
            }
            else
            {
                frame_type = "Data Frame";
                if (frame_len != len) {
                    error_mess = "Fail in check Length";
                    retvar = false;
                }
                else if (frame_cnt != Frame_Expect_Cnt[index])
                {
                    error_mess = "Expect Counter: " + Frame_Expect_Cnt[index] + "; Frame Counter: " + frame_cnt;
                    Frame_Expect_Cnt[index] = frame_cnt;    // update new frame counter
                    retvar = false;
                }
                else if (frame_crc != Get_CRC(cur_frame, len-2))
                {
                    error_mess = "Error Check CRC";
                    retvar = false;
                }
                else if (Check_Payload(index, frame_payload) == false)
                {
                    error_mess = "Error Payload";
                    retvar = false;
                }
                else
                {
                    error_mess = "Good Frame";
                    retvar = true;
                }
            }
            
            // Add Log Message
            log_mess = "Frame Length: " + frame_len;
            log_mess += "; Frame Counter: " + frame_cnt;
            // log_mess += "; Frame Payload:[" + Change_CRLF(frame_payload) + "]";
            log_mess += "; Frame CRC: " + Convert_Bytes_to_HexString(cur_frame, len - 2, 2);
            log_mess += "\nFrame Type: " + frame_type + "; Check Frame: " + error_mess + ";";
            if (retvar == true)
            {
                Pass_Cnt[index]++;
                log_mess += " ==> PASS\n";
                Tab2_add_log(index, log_mess, LogMsgType.Normal);
            }
            else
            {
                Fail_Cnt[index]++;
                log_mess += " ==> FAIL\n";
                Tab2_add_log(index, log_mess, LogMsgType.Error);
            }
            First_Receive[index] = true;       // Set Wait Receive 
            Adjust_frame_cnt(index);
            return retvar;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private int Get_CRC(byte [] input, int len)
        {
            int i;
            CrcAccumulator = 0;
            for (i = 0; i < len; i++)
            {
                CrcAccumulator = (UInt16)((CrcAccumulator << 8) ^ crcTable1021[(CrcAccumulator >> 8) ^ input[i]]);
            }
            return CrcAccumulator;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="flag"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        private bool Check_Payload(int index, string payload)
        {
            return true;

        }

        private bool Adjust_frame_cnt(int index)
        {
            bool ret_var = false;

            Frame_Expect_Cnt[index]++;
            if (Frame_Expect_Cnt[index] == 256)
            {
                Frame_Expect_Cnt[index] = 0;
            }
            ret_var = true;

            return ret_var;

        }

        /**********************************************************
             ######  ######  ######  ##    ##    ##   ####### 
             #      ##     # ##   ## ###   ##   # ##     #    
             ###### #      # ######  # #  # #  ##  #     #    
             #      ##    ## ##   ## #  # # #  ######    #    
             ##      ######  ##   ## ## ##  # ##    ##   #    
                                                              
                                                              
                                                              
            #      ######   ######      ###### #  #     ######
            #     ##    ## ##    ##     #      #  #     #     
            #     #      # #   ###      #####  #  #     ######
            #     #     ## #     ##     #      #  #     #     
            #####  ######   #######     #      #  ##### ######

         **********************************************************/
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file_index"></param>
        /// <param name="logmess"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool Tab2_add_log(int file_index, string logmess, LogMsgType type)
        {
            Add_logs(logmess, type, TabNum.Tab2);
            if (FileOut[file_index] != null)
            {
                Write_log_file(FileOut[file_index], logmess, LogMsgType.Outgoing);
            }
            else
            {
                Add_logs("Log file not exist! \n", type, TabNum.Tab2);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="file_name"></param>
        /// <returns></returns>
        private bool Write_Header_File(int index, string file_name) {
            bool ret_var = false;
            DateTime time_stamp = DateTime.Now;
            string log_mess;

            log_mess = "/*****************************************************************************************************************\n";
            log_mess += " * $File Name        : " + file_name + "\n";
            log_mess += " * $Device           : " + ComControlArray[index].DeviceNameText.Text + "\n";
            log_mess += " * $Default setting  : " + Tab2SetBaudrate.Text + ":" + Tab2SetDataBit.Text + ":" + Tab2SetStopBit.Text + ":" + Tab2SetParity.Text + "\n";
            log_mess += " * $Date             : " + time_stamp.ToString("dd/MM/yyyy-HH:mm:ss") + "\n";
            log_mess += " *****************************************************************************************************************/\n";
            Tab2_add_log(index, log_mess, LogMsgType.Outgoing); 

            return ret_var;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="file_name"></param>
        /// <returns></returns>
        private bool Write_Report(int index)
        {
            bool ret_var = false;
            DateTime time_stamp = DateTime.Now;
            TimeSpan time_span = Tab2_Stop[index] - Tab2_Start[index];
            string log_mess;

            log_mess = "/*****************************************************************************************************************\n";
            log_mess += " * $Report      : \n";
            log_mess += " * $Start time  : " + Tab2_Start[index].ToString("dd/MM/yyyy-HH:mm:ss") + "\n";
            log_mess += " * $Stop time   : " + Tab2_Stop[index].ToString("dd/MM/yyyy-HH:mm:ss") + "\n";
            log_mess += " * $Duration    : " + time_span.ToString() + "\n";
            log_mess += " * $Total Pass  : " + Pass_Cnt[index].ToString() + "\n";
            log_mess += " * $Total Fail  : " + Fail_Cnt[index].ToString() + "\n";                   
            log_mess += " *****************************************************************************************************************/\n";
            Tab2_add_log(index, log_mess, LogMsgType.Outgoing);

            return ret_var;
        }

        private bool Update_Pass_Fail()
        {
            int i;
            long pass = 0, fail = 0;
            for (i = 0; i < totalPort; i++)
            {
                pass += Pass_Cnt[i];
                fail += Fail_Cnt[i];
            }
            Tab2_Total_Pass_Lbl.Text = pass.ToString();
            Tab2_Total_Fail_Lbl.Text = fail.ToString();
            return true;
        }
    }
}