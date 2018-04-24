using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp7
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //Listing audio devices in comboBox1
            NAudio.CoreAudioApi.MMDeviceEnumerator enumerator = new NAudio.CoreAudioApi.MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(NAudio.CoreAudioApi.DataFlow.All, NAudio.CoreAudioApi.DeviceState.Active);
            comboBox1.Items.AddRange(devices.ToArray());
            globalVariable.oldPeak = 0;
            //Trying to open a serial port. Program won't fail if Arduino is not connected.
            try { serialPort1.Open(); }
            catch { }
        }
        
        private void timer1_Tick(object sender, EventArgs e)
        {
            //Pulling peak value from WASAPI device. And setting it to between 0-100
            //https://www.youtube.com/watch?v=OVkd6xIWHDc I actually did this part with this video. (Lines between 19-32, rest of them is mine) 
            if (comboBox1.SelectedItem != null)
            {               var device = (NAudio.CoreAudioApi.MMDevice) comboBox1.SelectedItem;
                double smoothingMultiplier;
                double amplifierMultiplier;
                amplifierMultiplier = (trackBar2.Value / 100.00) + 1.00;
                label10.Text = trackBar1.Value.ToString();
                labelAmp.Text = (trackBar2.Value + 100).ToString() + "%";
                smoothingMultiplier = (double)trackBar1.Value / 100.00;
                NewPeakMultiplier.Text = (1 - smoothingMultiplier).ToString();
                OldPeakMultiplier.Text = smoothingMultiplier.ToString();
                int peakVal = (int)Math.Round((device.AudioMeterInformation.MasterPeakValue * 100));
                peakVal = (int)Math.Round(peakVal * amplifierMultiplier);
                if (peakVal > 100)
                {
                    peakVal = 100;
                }
                notSmooth.Text = peakVal.ToString();
                //Smoothing basically works with weighted average. 
                peakVal = (int)Math.Round(globalVariable.oldPeak * smoothingMultiplier + peakVal * (1 - smoothingMultiplier));
                label1.Text = peakVal.ToString();
                // I divided spectrum between blue and red to three parts. Blue-magenta-red. 
                // At peak 0-33 will be shades of blue. (Higher the peak, higher the brightness)
                // At peak 33-66 blue at full level. Red will be added to color depending on the peak. On 66 it will be true magenta. 
                // At peak 66-100 red at full level. Blue will be substracted from color depending on the peak. On 100 it will be pure red.
                if (peakVal < 33)
                {
                    int blueVal = peakVal * 3;
                    int redVal = 0;
                    //Initializing SDK and sending color to all Logitech RGB devices connected.
                    LedCSharp.LogitechGSDK.LogiLedInit();
                    LedCSharp.LogitechGSDK.LogiLedSetLighting(redVal, 0, blueVal);
                    //Sending color values to Arduino or similar device through serial port.
                    writeToSerialPort(redVal, blueVal);
                    label3.Text = redVal.ToString();
                    label4.Text = blueVal.ToString();
                    globalVariable.oldPeak = peakVal;
                }
                if (peakVal > 33 & peakVal < 66)
                {
                    int redVal = (peakVal - 33) * 3;
                    int blueVal = 100;
                    LedCSharp.LogitechGSDK.LogiLedInit();
                    LedCSharp.LogitechGSDK.LogiLedSetLighting(redVal, 0, blueVal);
                    writeToSerialPort(redVal, blueVal);
                    label3.Text = redVal.ToString();
                    label4.Text = blueVal.ToString();
                    globalVariable.oldPeak = peakVal;
                }
                if (peakVal > 66)
                {
                    int redVal = 100;
                    //Using this variable for decreasing blue when peak is higher than 66. Also peak value of 100 will give pure red.
                    int differenceWith66 = peakVal - 66;
                    int blueVal = 100 - differenceWith66 * 3;
                    LedCSharp.LogitechGSDK.LogiLedInit();
                    LedCSharp.LogitechGSDK.LogiLedSetLighting(redVal, 0, blueVal);
                    writeToSerialPort(redVal, blueVal);
                    label3.Text = redVal.ToString();
                    label4.Text = blueVal.ToString();
                    globalVariable.oldPeak = peakVal;
                }


            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                //For some odd reason serial port gets stuck after about 1 minute. 
                //My solution was reseting serial port. It doesn't get stuck and not making a noticeable delay.
                serialPort1.Close();
                serialPort1.Open();
            }
            catch { }
        }

        void writeToSerialPort(int red, int blue)
        {
            //Don't use strings because it will cause delays. So we convert integers to  bytes
            //then send them to Arduino.
            //Sending integer 101 before sending red color value, 102 for green, 103 for blue.
            try
            {
                byte[] b = BitConverter.GetBytes(101);
                serialPort1.Write(b, 0, 1);
            }
            catch { }
            try
            {
                byte[] b = BitConverter.GetBytes(red);
                serialPort1.Write(b, 0, 1);
            }
            catch { }
            try
            {
                byte[] b = BitConverter.GetBytes(103);
                serialPort1.Write(b, 0, 1);
            }
            catch { }
            try
            {
                byte[] b = BitConverter.GetBytes(blue);
                serialPort1.Write(b, 0, 1);
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Smoothing smooths color changes. This makes it look like less responsive but it looks nicer. Try value 40. Higher than 60 is not recommended.");
        }
    }
    public  class globalVariable
    {
        public static int oldPeak;
    }
}
