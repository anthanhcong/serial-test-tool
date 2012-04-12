using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsApplication1
{
    partial class Test_Form
    {
        /*************************************************************************
         * 
         *  ########   ######   ########   ##     ##     ###   #########
            #        ##     ##  ##     #  ###    ###    ## #       ##   
            ######  ##       ## ## ## ##  ## #   # #    #  ##      ##   
            #       ##       ## ## ## ##  ## #  ## #   ##   ##     ##   
            #        ##     ##  ##     #  ## ## #  #  ########     ##   
            #         #######   ##     ## ##  ###  #  #      ##    ##   
                                                                        
                       #######      ###   ##########   ###              
                       #     ##    ## #       #       ## #              
                       #      ##   #  ##      ##      ## ##             
                       #      ##  #    ##     ##     ##   ##            
                       #     ##  ########     ##     #######            
                       #######   #      ##    ##    #       #    
         * 
         **************************************************************************/

        /// <summary>
        /// Name: FormatData
        /// 
        /// </summary>
        /// <param name="InData"></param>
        /// <param name="type"></0: Send Data></1: Receive>
        /// <returns></returns>
        private string FormatData(String InData, DataType type, DateTime CurrTime, TabNum tabNum, bool first_rec)
        {
            String OutData;
            String TimeStamp;
            DataMode currMode;
            bool LR_Mode;
            bool show_timeStamp;


            // Add Time Stamp
            TimeStamp = CurrTime.ToString("HH:mm:ss.fff");

            // Get Current data view mode
            switch (tabNum)
            {
                case TabNum.Tab1:
                    currMode = Tab1DataViewMode;
                    LR_Mode = Tab1LR.Checked;
                    show_timeStamp = true;
                    break;
                case TabNum.Tab2:
                    LR_Mode = Tab2LR.Checked;
                    currMode = Tab2DataViewMode;
                    show_timeStamp = Tab2_TimeStamp.Checked;
                    break;
                case TabNum.Tab3:
                    LR_Mode = Tab3LR.Checked;
                    currMode = t3_dataView;
                    show_timeStamp = true;
                    break;
                default:
                    LR_Mode = false;
                    currMode = Tab1DataViewMode;
                    show_timeStamp = true;
                    break;
            }

            if ((first_rec == false) && (type == DataType.Receive))
            {
                // in receive string
                // Add to rich text
                if (currMode == DataMode.Text)
                {
                    OutData = InData;
                    if (LR_Mode == true)
                    {
                        OutData = Change_CRLF(OutData);
                    }
                }
                else
                {
                    OutData = StringToHexString(InData);
                }
            }
            else
            {
                // Format data for receive data
                // For marciano test. we donot use time stamp for run automation test and check error
                if (show_timeStamp == false)
                {
                    TimeStamp = "";
                }

                switch (type)
                {
                    case DataType.Receive:
                        OutData = "R <" + TimeStamp + ">: \n";
                        break;
                    case DataType.Send:
                        OutData = "T <" + TimeStamp + ">: \n";
                        break;
                    default:
                        OutData = "Error: Can not format Message";
                        break;
                }

                // Add to rich text
                if (currMode == DataMode.Text)
                {
                    OutData += InData;
                    if (LR_Mode == true)
                    {
                        OutData = Change_CRLF(OutData);
                    }
                }
                else
                {
                    OutData += StringToHexString(InData);
                }
            }
            // Add new line
            if (type == DataType.Send)
            {
                OutData += "\n";
            }
            return OutData;
        }

        /// <summary> Converts an array of bytes into a formatted string of hex digits (ex: E4 CA B2)</summary>
        /// <param name="data"> The array of bytes to be translated into a string of hex digits. </param>
        /// <returns> Returns a well formatted string of hex digits with spacing. </returns>
        private string StringToHexString(String data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (char b in data)
            {
                sb.Append(Convert.ToString(Convert.ToByte(b), 16));
                sb.Append(" ");
            }
            return sb.ToString().ToUpper();
        }

        /// <summary>
        /// Name: Change_CRLF
        /// Function: Put CR, LF into {}
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string Change_CRLF(string data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (char b in data)
            {
                if (b == 5)
                {
                    sb.Append("{ENQ}");
                }
                else if (b == 6)
                {
                    sb.Append("{ACK}");
                }
                else if ((b >= 0x20) && (b <= 0x7E))
                {
                    sb.Append(b);
                }
                else if (b == '\n')
                {
                    sb.Append("{LF}\n");
                }
                else if (b == '\r')
                {
                    sb.Append("{CR}\r");
                }
                else
                {
                    sb.Append("{" + Convert.ToString(Convert.ToByte(b), 16) + "}");
                }
            }
            return sb.ToString();
        }

        private string Change_HexString2String(string indata) 
        {
            int i, in_len;
            Int32 value;

            // check correct data
            if (indata == "") return "";
            in_len = indata.Length;
            StringBuilder sb = new StringBuilder(in_len);
            for (i = 0; i < in_len; i++)
            {
                if (i < in_len - 3)
                {
                    if (((indata[i] == '\\') && ((indata[i + 1] == 'x') || (indata[i + 1] == 'x'))) &&
                        (((indata[i + 2] >= '0') && (indata[i + 2] <= '9')) ||
                         ((indata[i + 2] >= 'a') && (indata[i + 2] <= 'f')) ||
                         ((indata[i + 2] >= 'A') && (indata[i + 2] <= 'F'))) &&
                        (((indata[i + 3] >= '0') && (indata[i + 3] <= '9')) ||
                         ((indata[i + 3] >= 'a') && (indata[i + 3] <= 'f')) ||
                         ((indata[i + 3] >= 'A') && (indata[i + 3] <= 'F'))))
                    {
                        value = Int32.Parse(indata.Substring(i + 2, 2), System.Globalization.NumberStyles.HexNumber);
                        if ((value > 127) || (value == 0))
                        {
                            sb.Append("{");
                            sb.Append(Convert.ToString(value, 16));
                            sb.Append("}");
                        }
                        else
                        {
                            sb.Append(Convert.ToString(Convert.ToChar(Int32.Parse(indata.Substring(i + 2, 2), System.Globalization.NumberStyles.HexNumber))));
                        }
                        i += 3;
                    }
                    else
                    {
                        sb.Append(indata[i]);
                    }
                }
                else
                {
                    sb.Append(indata[i]);
                }
            }
            return sb.ToString();
        }


        private int Change_Text2Bytes(string indata, ref byte[] outdata_ptr)
        {
            int i, in_len, out_len = 0;

            // check correct data
            if (indata == "") return 0;
            in_len = indata.Length;
            for (i = 0; i < in_len; i++)
            {
                if (i < in_len - 3)
                {
                    if (((indata[i] == '\\') && ((indata[i + 1] == 'x') || (indata[i + 1] == 'x'))) &&
                        (((indata[i + 2] >= '0') && (indata[i + 2] <= '9')) ||
                         ((indata[i + 2] >= 'a') && (indata[i + 2] <= 'f')) ||
                         ((indata[i + 2] >= 'A') && (indata[i + 2] <= 'F'))) &&
                        (((indata[i + 3] >= '0') && (indata[i + 3] <= '9')) ||
                         ((indata[i + 3] >= 'a') && (indata[i + 3] <= 'f')) ||
                         ((indata[i + 3] >= 'A') && (indata[i + 3] <= 'F'))))
                    {
                        outdata_ptr[out_len] = byte.Parse(indata.Substring(i + 2, 2), System.Globalization.NumberStyles.HexNumber);
                        i += 3;
                    }
                    else
                    {
                        outdata_ptr[out_len] = (byte)indata[i];
                    }
                }
                else
                {
                    outdata_ptr[out_len] = (byte)indata[i];
                }
                out_len++;
            }
            return out_len;
        }

        private string Convert_Bytes_to_String(byte []input,int start, int len)
        {
            StringBuilder sb = new StringBuilder(len * 4);
            int i;

            for (i = start; i < start + len; i++)
            {
                if ((input[i] > 127) || (input[i] == 0))
                {
                    sb.Append("{" + Convert.ToString(input[i], 16) + "}");
                }
                else
                {
                    sb.Append((char) input[i]);
                }
            }
            return sb.ToString();
        }

        private string Convert_Bytes_to_HexString(byte[] input, int start, int len)
        {
            StringBuilder sb = new StringBuilder(len * 4);
            int i;

            for (i = start; i < start + len; i++)
            {
                sb.Append("{" + Convert.ToString(input[i], 16) + "}");
            }
            return sb.ToString();
        }
    }
}
