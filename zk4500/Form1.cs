using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AxZKFPEngXControl;

namespace zk4500
{
    public partial class Form1 : Form
    {
        private AxZKFPEngX ZkFprint = new AxZKFPEngX();
        private bool Check;

        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {

            Controls.Add(ZkFprint);
            InitialAxZkfp();
        }

        private void InitialAxZkfp()
        {
            try
            {

                ZkFprint.OnImageReceived += zkFprint_OnImageReceived;
                ZkFprint.OnFeatureInfo += zkFprint_OnFeatureInfo;
                //zkFprint.OnFingerTouching 
                //zkFprint.OnFingerLeaving
                ZkFprint.OnEnroll += zkFprint_OnEnroll;

                if (ZkFprint.InitEngine() == 0)
                {
                    ZkFprint.FPEngineVersion = "9";
                    ZkFprint.EnrollCount = 3;
                    deviceSerial.Text += " " + ZkFprint.SensorSN + " Count: " + ZkFprint.SensorCount.ToString() + " Index: " + ZkFprint.SensorIndex.ToString();
                    ShowHintInfo("Device successfully connected");
                }

            }
            catch(Exception ex)
            {
                ShowHintInfo("Device init err, error: " + ex.Message);
            }
        }

    private void zkFprint_OnImageReceived(object sender, IZKFPEngXEvents_OnImageReceivedEvent e)
        {
            Graphics g = fpicture.CreateGraphics();
            Bitmap bmp = new Bitmap(fpicture.Width, fpicture.Height);
            g = Graphics.FromImage(bmp);
            int dc = g.GetHdc().ToInt32();
            ZkFprint.PrintImageAt(dc, 0, 0, bmp.Width, bmp.Height);
            g.Dispose();
            fpicture.Image = bmp;
        }

        private void zkFprint_OnFeatureInfo(object sender, IZKFPEngXEvents_OnFeatureInfoEvent e)
        {

            String strTemp = string.Empty;
            if (ZkFprint.EnrollIndex != 1)
            {
                if (ZkFprint.IsRegister)
                {
                    if (ZkFprint.EnrollIndex - 1 > 0)
                    {
                        int eindex = ZkFprint.EnrollIndex - 1;
                        strTemp = "Please scan again ..." + eindex;
                    }
                }
            }
            ShowHintInfo(strTemp);
        }
        private void zkFprint_OnEnroll(object sender, IZKFPEngXEvents_OnEnrollEvent e)
        {
            if (e.actionResult)
            {


                string template = ZkFprint.EncodeTemplate1(e.aTemplate);
                txtTemplate.Text = template;
                ShowHintInfo("Registration successful. You can verify now");
                btnRegister.Enabled = false;
                btnVerify.Enabled = true;
            }
            else
            {
                ShowHintInfo("Error, please register again.");

            }
        }
        private void zkFprint_OnCapture(object sender, IZKFPEngXEvents_OnCaptureEvent e)
        {
            string template = ZkFprint.EncodeTemplate1(e.aTemplate);


            if (ZkFprint.VerFingerFromStr(ref template, txtTemplate.Text, false, ref Check))
            {
                ShowHintInfo("Verified");
            }
            else
                ShowHintInfo("Not Verified");

        }



        private void ShowHintInfo(String s)
        {
              prompt.Text = s;
        }

      
        private void btnVerify_Click(object sender, EventArgs e)
        {
            if (ZkFprint.IsRegister)
            {
                ZkFprint.CancelEnroll();
            }
            ZkFprint.OnCapture += zkFprint_OnCapture;
            ZkFprint.BeginCapture();
            ShowHintInfo("Please give fingerprint sample.");
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {

            ZkFprint.CancelEnroll();
            ZkFprint.EnrollCount = 3;
            ZkFprint.BeginEnroll();
            ShowHintInfo("Please give fingerprint sample.");
            
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            fpicture.Image = null;
        }

    }
}
