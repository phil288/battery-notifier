using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Management;
using System.IO;

namespace BatteryNotifier
{
    public partial class Form1 : Form
    {
        Timer timer = new Timer();
        int minValue = 20;
        public Form1()
        {
            try
            {
                string file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "min-battery.txt");
                if (File.Exists(file))
                {
                    minValue = int.Parse(File.ReadAllText(file));
                }
            }
            catch (Exception)
            {

            }
            InitializeComponent();
            this.Hide();
            this.ShowInTaskbar = false;
            timer.Interval = 60*1000;
            timer.Tick += Timer_Tick;
            timer.Start();
            this.Timer_Tick();
        }

        private void Timer_Tick(object sender = null, EventArgs e = null)
        {
            try
            {
                ObjectQuery query = new ObjectQuery("Select * FROM Win32_Battery");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

                ManagementObjectCollection collection = searcher.Get();

                double batteryStatus = 0;
                foreach (ManagementObject mo in collection)
                {
                    foreach (PropertyData property in mo.Properties)
                    {
                        if (property.Name == "BatteryStatus")
                        {
                            double.TryParse(property.Value.ToString(), out batteryStatus);
                        }
                        if (property.Name == "EstimatedChargeRemaining")
                        {
                            if (double.TryParse(property.Value.ToString(), out double charge))
                            {
                                if (charge == 100)
                                {
                                    this.notifyIcon1.Text = "ok";
                                }
                                else
                                {
                                    this.notifyIcon1.Text = charge.ToString() + "%";
                                }
                                if (batteryStatus == 1)
                                {
                                    this.CreateTextIcon(charge.ToString(), Color.Red);
                                }
                                else
                                {
                                    if (charge == 100)
                                    {
                                        this.CreateTextIcon("ok", Color.Green);
                                    }
                                    else
                                    {
                                        this.CreateTextIcon(charge.ToString(), Color.Green);
                                    }
                                }
                                if (charge <= this.minValue && batteryStatus == 1)
                                {
                                    this.Show();
                                    this.Activate();
                                    this.BringToFront();
                                    this.WindowState = FormWindowState.Normal;
                                }
                                else
                                {
                                    this.Hide();
                                    this.WindowState = FormWindowState.Minimized;
                                }
                            }
                        }
                        Console.WriteLine("Property {0}: Value is {1}", property.Name, property.Value);
                    }
                }
            }
            catch (Exception)
            {

            }
        }
        public void CreateTextIcon(string str, Color color)
        {
            Font fontToUse = new Font("Arial", 12, FontStyle.Regular, GraphicsUnit.Pixel);
            Brush brushToUse = new SolidBrush(color);
            Bitmap bitmapText = new Bitmap(16, 16);
            Graphics g = System.Drawing.Graphics.FromImage(bitmapText);
            IntPtr hIcon;
            g.Clear(Color.Transparent);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            g.DrawString(str, fontToUse, brushToUse, 0, 0);
            hIcon = (bitmapText.GetHicon());
            this.notifyIcon1.Icon = System.Drawing.Icon.FromHandle(hIcon);
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {

        }
    }
}
