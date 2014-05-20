using System;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;
using System.Collections.Generic;

namespace WindowsFormsApplication1
{
    public class Demo
    {
        public bool RunDemo()
        {
            System.Console.WriteLine("Hello");
            return true;
        }
    }
}
public class Tab2ComPort
{
    public SerialPort ComPort;
    public Timer ComTimer;
    public Timer RecTimer;
    public CheckBox ComCheckBox;
    public TextBox DeviceNameText;
    public TextBox DelayValueText;
    // public CheckBox FixTimeCheck;
    public Button SelectPathBT;
    public Label DataforSendLabel;
    public ListBox Data4Send;
    
    public void Class_Init(int x, int y, int index)
    {
        // Init Component
        ComPort = new SerialPort();
        ComTimer = new Timer();
        Data4Send = new ListBox();
        ComCheckBox = new CheckBox();
        DeviceNameText = new TextBox();
        DelayValueText = new TextBox();
        // FixTimeCheck = new CheckBox();
        SelectPathBT = new Button();
        DataforSendLabel = new Label();
        

        /************************ Setting *****************************/
        // ComPort
        /***********************************************/
        ComPort.ReadTimeout = 100;      // Timeout for read is 100ms

        // Timer
        /***********************************************/
        ComTimer.Tag = index;
        // ComTimer.Interval = 6000000;

        // Data
        /***********************************************/
        Data4Send.TabIndex = index;
        Data4Send.Visible = false;

        // ComCheckBox
        /***********************************************/
        ComCheckBox.AutoSize = true;
        ComCheckBox.Location = new System.Drawing.Point(6 + x, 59 + y);
        ComCheckBox.Name = "ComCheckBox";
        ComCheckBox.Size = new System.Drawing.Size(60, 17);
        ComCheckBox.Text = "COM";
        ComCheckBox.TextAlign = System.Drawing.ContentAlignment.TopRight;
        ComCheckBox.UseVisualStyleBackColor = true;
        ComCheckBox.TabIndex = index;

        // Device Name Text Box
        /***********************************************/
        DeviceNameText.Location = new System.Drawing.Point(72 + x, 57 + y);
        DeviceNameText.Name = "DeviceNameText";
        DeviceNameText.Size = new System.Drawing.Size(83, 20);
        DeviceNameText.TabIndex = index;

        // Delay Value
        /***********************************************/
        DelayValueText.Location = new System.Drawing.Point(171 + x, 57 + y);
        DelayValueText.Name = "DelayValueText";
        DelayValueText.Size = new System.Drawing.Size(86, 20);
        DelayValueText.TabIndex = index;

        
        // Check Box for use Fix Delay
        /***********************************************/
        /*
        FixTimeCheck.Location = new System.Drawing.Point(289 + x, 60 + y);
        FixTimeCheck.Name = "FixTimeCheck";
        FixTimeCheck.Size = new System.Drawing.Size(15, 14);
        FixTimeCheck.UseVisualStyleBackColor = true;
        FixTimeCheck.TabIndex = index;
        */

        // Select Path Button
        /***********************************************/
        SelectPathBT.Location = new System.Drawing.Point(336 + x, 55 + y);
        SelectPathBT.Name = "SelectPathBT";
        SelectPathBT.Size = new System.Drawing.Size(24, 23);
        SelectPathBT.Text = "...";
        SelectPathBT.UseVisualStyleBackColor = true;
        SelectPathBT.TabIndex = index;

        // Data for Send Label
        /***********************************************/
        DataforSendLabel.AutoSize = true;
        DataforSendLabel.Location = new System.Drawing.Point(366 + x, 60 + y);
        DataforSendLabel.Name = "DataforSendLabel";
        DataforSendLabel.Size = new System.Drawing.Size(87, 13);
        DataforSendLabel.TabIndex = index;
        DataforSendLabel.Text = "Select Data for Send";
    }

    /************************ Control ComPort *****************************/
    /// <summary>
    /// Name: Open Port
    /// </summary>
    /// <param name="portName"></param>
    public bool OpenPort()
    {
        try{
            ComPort.Open();
            return true;
        }
        catch{
            MessageBox.Show(("Can not Open" + ComPort.PortName),"Error");
            return false;
        }
    }

    /// <summary>
    /// Name: Close Port
    /// </summary>
    public void ClosePort()
    {
        try
        {
            ComPort.Close();
        }
        catch
        {
            MessageBox.Show(("Can not Close" + ComPort.PortName), "Error");
        }
    }

    /// <summary>
    /// Name: Write Data
    /// </summary>
    /// <param name="data"></param>
    public void Write_data(byte[] data, int len)
    {
        // ComPort.WriteLine(data);
        ComPort.Write(data, 0, len);
    }

    /************************ Timer Control *****************************/
    /// <summary>
    /// Name: Timer_setDelay
    /// </summary>
    /// <param name="delayTime"></param>
    public void Timer_setDelay(int delayTime)
    {
        ComTimer.Stop();
        ComTimer.Interval = delayTime;
    }

    /// <summary>
    /// Name: Timer_Stop
    /// </summary>
    public void Timer_Stop()
    {
        ComTimer.Stop();
    }

    /// <summary>
    /// Name: Timer_Start
    /// </summary>
    public void Timer_Start()
    {
        try
        {
            ComTimer.Start();
        }
        catch
        {
            MessageBox.Show("Can not Start Timer", "Error");
        }
    }


    /************************ Form Control *****************************/
    /// <summary>
    /// Disable
    /// </summary>
    public void Disable()
    {
        ComCheckBox.Enabled = false;
        DeviceNameText.Enabled = false;
        DelayValueText.Enabled = false;
        // FixTimeCheck.Enabled = false;
        SelectPathBT.Enabled = false;
        DataforSendLabel.Enabled = false;
    }

    /// <summary>
    /// Name: Enabel
    /// </summary>
    public void Enable()
    {
        ComCheckBox.Enabled = true;
        DeviceNameText.Enabled = true;
        DelayValueText.Enabled = true;
        // FixTimeCheck.Enabled = true;
        SelectPathBT.Enabled = true;
        DataforSendLabel.Enabled = true;
    }
}