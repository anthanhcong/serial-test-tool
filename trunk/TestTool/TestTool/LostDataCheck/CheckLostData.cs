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
using System.Diagnostics;

namespace WindowsFormsApplication1
{
    public partial class Test_Form
    {
        public Process aspvnProcess;
        public ProcessStartInfo aspvnProcessStartInfo;
        static Int32 tp_expc_cnt, appl_expc_cnt;

        private void Lost_Frame_Check_Load()
        {
         
        }
        private void Lost_Frame_Check_Close()
        {
        }

        private void Lost_OpenFile_Click(object sender, EventArgs e)
        {
            StreamReader myfile;
            string path;
            string buffer;
            int line_index;
            string []split_data;
            string address;
            string[] addr_array;
            int split_len;
            string tp_cnt_str;
            string appl_cnt_str;
            Int32 appl_cnt, tp_cnt;
            Int32 total_label;
            Int32 appl_cnt_err, tp_cnt_err;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                
                address = Gun_Addr_Stamping_txt.Text.Trim();
                if (address == "")
                {
                    MessageBox.Show("Please check address stamping", "Missing input");
                }
                addr_array = address.Split('|');
                for (int i = 0; i < addr_array.Length; i++)
                {
                    address = addr_array[i].Trim();
                    tp_expc_cnt = 0;
                    appl_expc_cnt = 0;
                    line_index = 0;
                    appl_cnt_err = 0;
                    tp_cnt_err = 0;
                    total_label = 0;
                    path = openFileDialog1.FileName;
                    myfile = File.OpenText(path);
                    while ((buffer = myfile.ReadLine()) != null)
                    {
                        buffer = buffer.Trim();
                        split_data = buffer.Split('<');
                        split_len = split_data.Length;
                        if (split_data[0] == address)
                        {
                            if (split_len == 2)
                            {
                                // Only one counter in data
                                // check appl_cnt
                                if (Lost_Check_Appl_Cnt.Checked == true)
                                {
                                    appl_cnt_str = split_data[1].Substring(0, 6);
                                    appl_cnt = Convert.ToInt32(appl_cnt_str);
                                    if (appl_cnt != appl_expc_cnt)
                                    {
                                        Check_Lost_Frame_log.AppendText("Line: " + (line_index + 1).ToString() + "AP_CNT: " + appl_cnt + "Exp: " + appl_expc_cnt + "\n");
                                        appl_cnt_err++;
                                    }
                                    appl_expc_cnt = appl_cnt + 1;
                                }
                                // Check TP_counter
                                if (Lost_Check_Tp_Cnt.Checked == true)
                                {
                                    tp_cnt_str = split_data[1].Substring(0, 4);
                                    tp_cnt = Convert.ToInt32(tp_cnt_str);
                                    if (tp_cnt != tp_expc_cnt)
                                    {
                                        Check_Lost_Frame_log.AppendText("Line: " + (line_index + 1).ToString() + " TP_CNT: " + tp_cnt + "Exp: " + tp_expc_cnt + "\n");
                                        tp_cnt_err++;
                                    }
                                    tp_expc_cnt = tp_cnt + 1;
                                }
                            }
                            else if (split_len > 2)
                            {
                                // check appl_cnt
                                if (Lost_Check_Appl_Cnt.Checked == true)
                                {
                                    appl_cnt_str = split_data[1].Substring(0, 6);
                                    appl_cnt = Convert.ToInt32(appl_cnt_str);
                                    if (appl_cnt != appl_expc_cnt)
                                    {
                                        Check_Lost_Frame_log.AppendText("Line: " + (line_index + 1).ToString() + " AP_CNT: " + appl_cnt + "Exp: " + appl_expc_cnt + "\n");
                                        appl_cnt_err++;
                                    }
                                    appl_expc_cnt = appl_cnt + 1;
                                }
                                // Check TP_counter
                                if (Lost_Check_Tp_Cnt.Checked == true)
                                {
                                    tp_cnt_str = split_data[2].Substring(0, 4);
                                    tp_cnt = Convert.ToInt32(tp_cnt_str);
                                    if (tp_cnt != tp_expc_cnt)
                                    {
                                        Check_Lost_Frame_log.AppendText("Line: " + (line_index + 1).ToString() + " TP_CNT: " + tp_cnt + "Exp: " + tp_expc_cnt + "\n");
                                        tp_cnt_err++;
                                    }
                                    tp_expc_cnt = tp_cnt + 1;
                                }
                            }
                            total_label++;
                        }
                        line_index++;
                    }
                    Check_Lost_Frame_log.AppendText("+-------------------------------------------------------------------------+\n");
                    Check_Lost_Frame_log.AppendText("Test for Gun             : " + address.Trim() + "\n");
                    Check_Lost_Frame_log.AppendText("Total Appl Counter Error : " + appl_cnt_err.ToString() + "\n");
                    Check_Lost_Frame_log.AppendText("Total Tp Counter Error   : " + tp_expc_cnt.ToString() + "\n");
                    Check_Lost_Frame_log.AppendText("Total Data               : " + total_label.ToString() + "\n");
                    Check_Lost_Frame_log.AppendText("+-------------------------------------------------------------------------+\n");
                    myfile.Close();
                }
            }
            else
            {
                MessageBox.Show("Can not Open file !", "Error");
            }
        }

        private void Lost_Enter_BT_Click(object sender, EventArgs e)
        {
            /*
            try
            {
                Process aspvnProcess = new Process();
                ProcessStartInfo aspvnProcessStartInfo = new ProcessStartInfo("cmd.exe");
                aspvnProcessStartInfo.UseShellExecute = false;
                aspvnProcessStartInfo.RedirectStandardOutput = true;
                aspvnProcess.StartInfo = aspvnProcessStartInfo;
                aspvnProcessStartInfo.Arguments = "/C" + Lost_CmdTxt.Text;
                aspvnProcess.Start();
                StreamReader myStreamReader = aspvnProcess.StandardOutput;
                string myString = myStreamReader.ReadToEnd();
                aspvnProcess.Close();
                // Lost_Display.AppendText( Lost_CmdTxt.Text + System.Environment.NewLine + "<pre>" + myString + "</pre>");
                Lost_Display.AppendText(Lost_CmdTxt.Text + System.Environment.NewLine + myString);
                Lost_CmdTxt.Text = "";
            }
            catch
            {
                Lost_Display.AppendText("Unknow Command");
            }
            finally { }
             * */
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = "calc";
            proc.Start();
        }
    }
}