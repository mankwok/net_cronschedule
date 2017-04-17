using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace App2
{
    public partial class Form1 : Form
    {
        CrontabSchedule cron; 

        public Form1()
        {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync();
            this.maskedTextBox_baseTime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            cron = new CrontabSchedule();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                //this.toolStripStatusLabel1.Text = "Time Now: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                backgroundWorker1.ReportProgress(1);
                System.Threading.Thread.Sleep(1000);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.maskedTextBox_baseTime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cron.SetCronFields(this.textBox_cron.Text);
            DateTime baseTime = DateTime.Parse(this.maskedTextBox_baseTime.Text);
            DateTime scheduleTime = cron.GetNextScheduledTime(baseTime);
            this.richTextBox1.Text += "\r\nCron: " + cron.Schedule 
                + "\r\nCurrent Time: " + baseTime.ToString("dd/MM/yyyy HH:mm:ss") 
                + "\r\nSchedule Time: " + scheduleTime.ToString("dd/MM/yyyy HH:mm:ss") 
                + "\r\n---------------------------";

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Text = "";
        }
    }
}
