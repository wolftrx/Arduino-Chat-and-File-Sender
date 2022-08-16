using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Globalization;

namespace AD_Arduino_File_Sender_v1._0
{
    public partial class Form1 : Form
    {
        SerialPort connection=new SerialPort();
        Thread th;
        Thread filesender;
        bool sending = false;
        bool listening = false;
        string namedata = "Name;";
        char seperater = ';';
        string starterdata = "file;";
        string filedatacountstring="filecount;";
        string finishdata = "fileend;";
        string filename = "";
        int filedatacount;
        string cht = "cht;";
        string[] file;
        string data="";
        string listboxdata = "";
        int waitingtime = 800;
        public Form1()
        {
            InitializeComponent();
            toolStripStatusLabel1.Text = "Wellcome to Arduino Destek Arduino File Sender v1.0";
        }

        private void portAyarlarıToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void SearchPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            comboBox1.Items.Clear();
            foreach (string port in ports)
            {
                comboBox1.Items.Add(port);
            }
            toolStripStatusLabel1.Text = "Ports listed!";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SearchPorts();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            openport();


        }

        private void button5_Click(object sender, EventArgs e)
        {
            closeport();
        }
        private void Listener()
        {
            bool filecoming = false;
            int packetcount = 0;

            while (listening == true)
            {
                //data = connection.ReadLine();
                //connection.DataReceived += new SerialDataReceivedEventHandler(connection_DataReceived);

                data = connection.ReadLine();
                data.Trim();
                if (data == "\r")
                {

                }
                else
                {
                    if (data.Contains(namedata) == true)
                    {
                        string[] a = data.Split(seperater);
                        string c= a[1].Trim();
                        filename = c;
                        listboxdata = "-Getting File:" + c;
                        AddTextToListBox();
                    }
                    else if (data.Contains(filedatacountstring) == true)
                    {
                        string[] a = data.Split(seperater);
                        filedatacount = Convert.ToInt32(a[1]);
                        Addprogressbar();
                    }
                    else if (data.Contains(starterdata) == true)
                    {
                        filecoming = true;
                        file = new string[filedatacount];

                    }
                    else if (data.Contains(cht) == true)
                    {
                        string[] a = data.Split(seperater);
                        listboxdata ="-:"+ a[1];
                        AddTextToListBox();
                        //listBox1.Items.Add(data);

                    }
                    else if (data.Contains(finishdata) == true)
                    {
                        filecoming = false;
                        string base64data = "";
                        foreach (var item in file)
                        {
                            base64data += item;
                        }
                        byte[] buffer = Convert.FromBase64String(base64data);
                        SaveFileDialog sav = new SaveFileDialog();
                        string savefilename = "";
                        File.WriteAllBytes(Directory.GetCurrentDirectory() + "\\"+filename, buffer);
                        System.Threading.Thread.Sleep(waitingtime);
                        toolStripStatusLabel1.Text = "File downloaded to " + Directory.GetCurrentDirectory() + "\\" + filename;
                        listboxdata = Directory.GetCurrentDirectory() +"\\"+ filename + " downloaded!";
                        AddTextToListBox();
                        progressbarreset();
                        packetcount=0;

                    }
                    else
                    {
                        file[packetcount] = data;
                        packetcount++;
                        progressbarincreaser();
                    }
                }


                data = "";
            }
        }

        private void FileSeder()
        {
            OpenFileDialog op = new OpenFileDialog();
            string sendingfilename = "";
            string justfilename = "";
            byte[] buffer;
            string sendingfilebase64 = "";
            int sendingpacketcount = 0;
            if (op.ShowDialog()==DialogResult.OK)
            {
                progressBar1.Value = 0;
                sendingfilename = op.FileName;
                justfilename = ConvertTRCharToENChar(op.SafeFileName);
                buffer = File.ReadAllBytes(sendingfilename);
                sendingfilebase64=Convert.ToBase64String(buffer);
                int modcount = sendingfilebase64.Length % 31;
                if (modcount != 0)
                {
                    sendingpacketcount = Convert.ToInt32(sendingfilebase64.Length / 31) + 1;

                }
                else
                {
                    sendingpacketcount = (sendingfilebase64.Length / 31);
                }
                progressBar1.Maximum = sendingpacketcount;
            }
            if (sendingfilename != "")
            {
                if (justfilename.Length > 26)
                {
                    toolStripStatusLabel1.Text = "File name length so much! Change name of file and try again!";
                }
                else
                {
                    int sendedpacketcount = 1;
                    connection.WriteLine(namedata + justfilename);
                    System.Threading.Thread.Sleep(waitingtime);
                    connection.WriteLine(filedatacountstring + sendingpacketcount);
                    System.Threading.Thread.Sleep(waitingtime);
                    connection.WriteLine(starterdata);
                    System.Threading.Thread.Sleep(waitingtime);

                    while (sendedpacketcount <= sendingpacketcount)
                    {
                        if (sendedpacketcount < sendingpacketcount)
                        {
                            string subsstring = sendingfilebase64.Substring((sendedpacketcount - 1) * 31, 31);
                            connection.WriteLine(subsstring);
                            sendedpacketcount++;
                            progressBar1.Value++;
                        }
                        else
                        {
                            string subsstring = sendingfilebase64.Substring((sendedpacketcount - 1) * 31, sendingfilebase64.Length % 31);
                            connection.WriteLine(subsstring);
                            sendedpacketcount++;
                            progressBar1.Value++;
                        }
                        System.Threading.Thread.Sleep(waitingtime);

                    }
                    connection.WriteLine(finishdata);
                    System.Threading.Thread.Sleep(waitingtime);
                    toolStripStatusLabel1.Text = sendingfilename + " sended!";
                    System.Threading.Thread.Sleep(waitingtime);
                    listBox1.Items.Add(sendingfilename + " sended!");
                    progressbarreset();
                    //filesender.Abort();
                }

            }
            else
            {
                toolStripStatusLabel1.Text = "Please select a file and try again!";
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            sending = true;
            FileSeder();
            //filesender = new Thread(new ThreadStart(FileSeder));
            
            //filesender.Start();

        }

        private void button2_Click(object sender, EventArgs e)
        {

            connection.WriteLine(cht + textBox1.Text);
            listBox1.Items.Add("+:" + textBox1.Text);

        }
        private void connection_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }
        private void AddTextToListBox()
        {
            
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new MethodInvoker(AddTextToListBox), new object[] { Text });
                return;
            }
            listBox1.Items.Add(listboxdata);
        }
        private void Addprogressbar()
        {

            if (progressBar1.InvokeRequired)
            {
                progressBar1.Invoke(new MethodInvoker(Addprogressbar), new object[] { Text });
                return;
            }
            progressBar1.Maximum = filedatacount;
        }
        private void progressbarincreaser()
        {

            if (progressBar1.InvokeRequired)
            {
                progressBar1.Invoke(new MethodInvoker(progressbarincreaser), new object[] { Text });
                return;
            }
            progressBar1.Value=progressBar1.Value+1;
        }
        private void progressbarreset()
        {

            if (progressBar1.InvokeRequired)
            {
                progressBar1.Invoke(new MethodInvoker(progressbarreset), new object[] { Text });
                return;
            }
            progressBar1.Value = 0;
        }
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            waitingtime = Convert.ToInt32(numericUpDown1.Value);
        }
        private void openport()
        {
            try
            {
                connection.PortName = comboBox1.Text;
                connection.BaudRate = Convert.ToInt32(comboBox2.Text);
                connection.Open(); 
                toolStripStatusLabel1.Text = "Connection started!";
                th = new Thread(Listener);
                listening = true;
                th.Start();
            }
            catch (Exception ex)
            {

                toolStripStatusLabel1.Text = "Connection failed!";
                MessageBox.Show(ex.Message);
            }
        }
        private void closeport()
        {
            try
            {
                connection.Close();
                listening = false;
                th.Abort();
                toolStripStatusLabel1.Text = "Connection closed!";


            }
            catch (Exception ex)
            {

                toolStripStatusLabel1.Text = "Connection didn't closed!";
                MessageBox.Show(ex.Message);
            }
        }
        string ConvertTRCharToENChar(string text)
        {
            return String.Join("", text.Normalize(NormalizationForm.FormD)
            .Where(c => char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
        }

        private void websiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://arduinodestek.com");
        }

        private void programToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 f = new Form2();
            f.ShowDialog();
        }
    }
}
