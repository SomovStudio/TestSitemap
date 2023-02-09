using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml;
using System.Diagnostics;
using TestSitemap;

namespace QASupport.TestSitemap
{
    public partial class FormTestSitemap : Form
    {
        public FormTestSitemap()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private bool processRun = false;
        private Thread thread;

        private void showProgressTest(double totalPages, double onePercent, double step)
        {
            toolStripProgressBar1.Maximum = Convert.ToInt32(totalPages);
            toolStripProgressBar1.Value = Convert.ToInt32(step);
            double progressPercent = 0;
            if (totalPages < 100 && onePercent > 0) progressPercent = (step * onePercent);
            if (totalPages >= 100) progressPercent = (step / onePercent);
            progressPercent = Math.Round(progressPercent, 0);
            if (progressPercent < 100) toolStripStatusLabel2.Text = Convert.ToString(progressPercent) + "%";
            else toolStripStatusLabel2.Text = "99%";

            double dSec = (totalPages - step) * 1;

            int sec = Convert.ToInt32(dSec);
            int minutes = sec / 60;
            int newSec = sec - minutes * 60;
            int hour = minutes / 60;
            int newMinnutes = minutes - hour * 60;
            TimeSpan time = new TimeSpan(hour, newMinnutes, newSec);
            toolStripStatusLabel5.Text = time.ToString();
        }

        private ArrayList readUrlXML(string filename)
        {
            ArrayList list = new ArrayList();

            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                XmlDocument xDoc;
                if (checkBoxUserAgent.Checked == true)
                {
                    xDoc = new XmlDocument();
                    xDoc.Load(filename);
                }
                else
                {
                    WebClient client = new WebClient();
                    client.Headers["User-Agent"] = textBoxUserAgent.Text;
                    client.Headers["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                    string data = client.DownloadString(filename);

                    xDoc = new XmlDocument();
                    xDoc.LoadXml(data);
                }

                XmlElement xRoot = xDoc.DocumentElement;
                foreach (XmlNode xnode in xRoot)
                {
                    for (int j = 0; j <= xnode.ChildNodes.Count; j++)
                    {
                        if (xnode.ChildNodes[j].Name == "loc")
                        {
                            list.Add(xnode.ChildNodes[j].InnerText);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }

            return list;
        }

        private ArrayList readXML(string filename)
        {
            ArrayList list = new ArrayList();

            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                XmlDataDocument xmldoc = new XmlDataDocument();
                xmldoc.PreserveWhitespace = true;
                xmldoc.XmlResolver = null;
                XmlNodeList xmlnode;
                int i = 0;
                string str = null;
                FileStream fs = new FileStream(@filename, FileMode.Open, FileAccess.Read);
                xmldoc.Load(fs);
                xmlnode = xmldoc.GetElementsByTagName("url");
                if (xmlnode.Count <= 0) xmlnode = xmldoc.GetElementsByTagName("sitemap");

                for (i = 0; i <= xmlnode.Count - 1; i++)
                {
                    for (int j = 0; j <= xmlnode[i].ChildNodes.Count; j++)
                    {
                        if (xmlnode[i].ChildNodes.Item(j).Name == "loc")
                        {
                            string link = xmlnode[i].ChildNodes.Item(j).InnerText.Trim();
                            list.Add(link);
                            break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }

            return list;
        }


        private void FormTestSitemap_Load(object sender, EventArgs e)
        {
            thread = new Thread(TestUrl);
        }

        private void FormTestSitemap_FormClosed(object sender, FormClosedEventArgs e)
        {
            try { thread.Abort(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
        }

        private void закрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void сохранитьСписокПроверкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try {
                saveFileDialog1.FileName = "report.txt";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter SW = new StreamWriter(new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write));
                    SW.Write(textBoxProcess.Text);
                    SW.Close();
                    MessageBox.Show("Файл сохранён", "Сообщение");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void сохранитьСписокОтветов100ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.FileName = "report_100.txt";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter SW = new StreamWriter(new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write));
                    SW.Write(richTextBox100.Text);
                    SW.Close();
                    MessageBox.Show("Файл сохранён", "Сообщение");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void сохранитьСписокОтветов200ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.FileName = "report_200.txt";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter SW = new StreamWriter(new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write));
                    SW.Write(richTextBox200.Text);
                    SW.Close();
                    MessageBox.Show("Файл сохранён", "Сообщение");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void сохранитьСписокОтветов300ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.FileName = "report_300.txt";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter SW = new StreamWriter(new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write));
                    SW.Write(richTextBox300.Text);
                    SW.Close();
                    MessageBox.Show("Файл сохранён", "Сообщение");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void сохранитьСписокОтветов400ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.FileName = "report_400.txt";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter SW = new StreamWriter(new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write));
                    SW.Write(richTextBox400.Text);
                    SW.Close();
                    MessageBox.Show("Файл сохранён", "Сообщение");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void сохранитьСписокОтветов500ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.FileName = "report_500.txt";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter SW = new StreamWriter(new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write));
                    SW.Write(richTextBox500.Text);
                    SW.Close();
                    MessageBox.Show("Файл сохранён", "Сообщение");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void сохранитьСписокРазныхОтветовToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.FileName = "report_errors.txt";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter SW = new StreamWriter(new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write));
                    SW.Write(richTextBoxOther.Text);
                    SW.Close();
                    MessageBox.Show("Файл сохранён", "Сообщение");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void checkBoxUserAgent_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxUserAgent.Checked == true)
            {
                checkBoxUserAgent.Text = "Включен User-Agent по умолчанию";
                textBoxUserAgent.Text = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1";
                textBoxUserAgent.Enabled = false;
            }
            else
            {
                checkBoxUserAgent.Text = "Отключен User-Agent по умолчанию";
                textBoxUserAgent.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxXML.Text = openFileDialog1.FileName;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                textBoxTXT.Text = openFileDialog2.FileName;
            }
        }

        private void toolStripButtonStart_Click(object sender, EventArgs e)
        {
            TestBegin();
        }

        private void запуститьПроверкуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TestBegin();
        }

        private void toolStripButtonStop_Click(object sender, EventArgs e)
        {
            TestStop();
        }

        private void остановитьПроверкуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TestStop();
        }

        private void TestEnd()
        {
            processRun = false;

            int res100 = richTextBox100.Lines.Count() - 1;
            if (res100 < 0) res100 = 0;
            int res200 = richTextBox200.Lines.Count() - 1;
            if (res200 < 0) res200 = 0;
            int res300 = richTextBox300.Lines.Count() - 1;
            if (res300 < 0) res300 = 0;
            int res400 = richTextBox400.Lines.Count() - 1;
            if (res400 < 0) res400 = 0;
            int res500 = richTextBox500.Lines.Count() - 1;
            if (res500 < 0) res500 = 0;
            int resOther = richTextBoxOther.Lines.Count() - 1;
            if (resOther < 0) resOther = 0;

            textBoxProcess.AppendText(Environment.NewLine);
            textBoxProcess.AppendText("== Результат ==========================" + Environment.NewLine);
            textBoxProcess.AppendText("1хх ответов: " + res100.ToString() + Environment.NewLine);
            textBoxProcess.AppendText("2хх ответов: " + res200.ToString() + Environment.NewLine);
            textBoxProcess.AppendText("3хх ответов: " + res300.ToString() + Environment.NewLine);
            textBoxProcess.AppendText("4хх ответов: " + res400.ToString() + Environment.NewLine);
            textBoxProcess.AppendText("5хх ответов: " + res500.ToString() + Environment.NewLine);
            textBoxProcess.AppendText("Других ответов:  " + resOther.ToString() + Environment.NewLine);
            textBoxProcess.AppendText("=======================================" + Environment.NewLine);
            this.Update();
            MessageBox.Show("Процесс проверки - завершен!");
        }

        private void TestStop()
        {
            try
            {
                processRun = false;
                thread.Abort();
                MessageBox.Show("Процесс проверки - остановлен!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Предупреждение");
            }
        }

        private void TestBegin()
        {
            if (processRun == true)
            {
                MessageBox.Show("Процесс уже запущен", "Сообщение");
                return;
            }

            textBoxProcess.Text = "";
            richTextBox100.Text = "";
            richTextBox200.Text = "";
            richTextBox300.Text = "";
            richTextBox400.Text = "";
            richTextBox500.Text = "";
            richTextBoxOther.Text = "";
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Maximum = 0;
            toolStripStatusLabel2.Text = "0%";
            toolStripStatusLabel5.Text = "0:00";

            if (radioButton1.Checked) CheckURL();
            if (radioButton2.Checked) CheckLocal();
            if (radioButton3.Checked) CheckLocal2();
        }

        private void CheckURL()
        {
            thread = new Thread(TestUrl);
            thread.Start();
        }

        private void TestUrl()
        {
            processRun = true;
            try
            {
                ArrayList listLinks = readUrlXML(textBoxURL.Text);
                int count = listLinks.Count;
                int index = 1;
                double totalPages = count;
                double onePercent = 0;
                if (totalPages < 100) onePercent = (100 / totalPages);
                else onePercent = (totalPages / 100);

                HttpClient client;
                HttpResponseMessage response;
                HttpClientHandler handler = new HttpClientHandler();
                handler.AllowAutoRedirect = false;
                foreach (String link in listLinks)
                {
                    try
                    {
                        client = new HttpClient(handler);
                        if (checkBoxUserAgent.Checked == false) client.DefaultRequestHeaders.UserAgent.ParseAdd(textBoxUserAgent.Text);
                        client.BaseAddress = new Uri(link);

                        response = client.GetAsync(link).Result;
                        int statusCode = (int)response.StatusCode;

                        textBoxProcess.AppendText("[" + index.ToString() + "/" + totalPages.ToString() + "] STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine);
                        textBoxProcess.ScrollToCaret();

                        if (statusCode >= 100 && statusCode <= 199)
                        {
                            //textBox100.AppendText("STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine);
                            richTextBox100.Text = richTextBox100.Text + "STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine;
                            //textBox100.ScrollToCaret();
                        }
                        if (statusCode >= 200 && statusCode <= 299)
                        {
                            //textBox200.AppendText("STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine);
                            richTextBox200.Text = richTextBox200.Text + "STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine;
                            //textBox200.ScrollToCaret();
                        }
                        if (statusCode >= 300 && statusCode <= 399)
                        {
                            //textBox300.AppendText("STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine);
                            richTextBox300.Text = richTextBox300.Text + "STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine;
                            //textBox300.ScrollToCaret();
                        }
                        if (statusCode >= 400 && statusCode <= 499)
                        {
                            //textBox400.AppendText("STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine);
                            richTextBox400.Text = richTextBox400.Text + "STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine;
                            //textBox400.ScrollToCaret();
                        }
                        if (statusCode >= 500 && statusCode <= 599)
                        {
                            //textBox500.AppendText("STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine);
                            richTextBox500.Text = richTextBox500.Text + "STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine;
                            //textBox500.ScrollToCaret();
                        }
                        if (statusCode <= 99 || statusCode >= 600)
                        {
                            //textBoxOther.AppendText("STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine);
                            richTextBoxOther.Text = richTextBoxOther.Text + "STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine;
                            //textBoxOther.ScrollToCaret();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка");
                        //textBoxOther.AppendText("ERROR [" + ex.Message + "]: " + link + Environment.NewLine);
                        richTextBoxOther.Text = richTextBoxOther.Text + "ERROR [" + ex.Message + "]: " + link + Environment.NewLine;
                        //textBoxOther.ScrollToCaret();
                    }

                    showProgressTest(totalPages, onePercent, index);
                    index++;
                }

                toolStripStatusLabel2.Text = "100%";
                toolStripStatusLabel5.Text = "0:00";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
            TestEnd();
        }

        private void CheckLocal()
        {
            thread = new Thread(TestLocal);
            thread.Start();
        }

        private void TestLocal()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            processRun = true;
            try
            {
                ArrayList listLinks = readXML(textBoxXML.Text);
                int count = listLinks.Count;
                int index = 1;
                double totalPages = count;
                double onePercent = 0;
                if (totalPages < 100) onePercent = (100 / totalPages);
                else onePercent = (totalPages / 100);

                HttpClient client;
                HttpResponseMessage response;
                HttpClientHandler handler = new HttpClientHandler();
                handler.AllowAutoRedirect = false;
                foreach (String link in listLinks)
                {
                    try
                    {
                        client = new HttpClient(handler);
                        if (checkBoxUserAgent.Checked == false) client.DefaultRequestHeaders.UserAgent.ParseAdd(textBoxUserAgent.Text);
                        client.BaseAddress = new Uri(link);

                        response = client.GetAsync(link).Result;
                        int statusCode = (int)response.StatusCode;

                        textBoxProcess.AppendText("[" + index.ToString() + "/" + totalPages.ToString() + "] STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine);
                        textBoxProcess.ScrollToCaret();

                        if (statusCode >= 100 && statusCode <= 199)
                        {
                            richTextBox100.Text = richTextBox100.Text + "STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine;
                        }
                        if (statusCode >= 200 && statusCode <= 299)
                        {
                            richTextBox200.Text = richTextBox200.Text + "STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine;
                        }
                        if (statusCode >= 300 && statusCode <= 399)
                        {
                            richTextBox300.Text = richTextBox300.Text + "STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine;
                        }
                        if (statusCode >= 400 && statusCode <= 499)
                        {
                            richTextBox400.Text = richTextBox400.Text + "STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine;
                        }
                        if (statusCode >= 500 && statusCode <= 599)
                        {
                            richTextBox500.Text = richTextBox500.Text + "STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine;
                        }
                        if (statusCode <= 99 || statusCode >= 600)
                        {
                            richTextBoxOther.Text = richTextBoxOther.Text + "STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка");
                        richTextBoxOther.Text = richTextBoxOther.Text + "ERROR [" + ex.Message + "]: " + link + Environment.NewLine;
                    }

                    showProgressTest(totalPages, onePercent, index);
                    index++;
                }

                toolStripStatusLabel2.Text = "100%";
                toolStripStatusLabel5.Text = "0:00";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
            TestEnd();
        }

        private void CheckLocal2()
        {
            thread = new Thread(TestLocal2);
            thread.Start();
        }

        private void TestLocal2()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            processRun = true;
            try
            {
                ArrayList listLinks = new ArrayList();
                string path = @textBoxTXT.Text;
                using (StreamReader sr = new StreamReader(path))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        listLinks.Add(line.ToString());
                    }
                }

                int count = listLinks.Count;
                int index = 1;
                double totalPages = count;
                double onePercent = 0;
                if (totalPages < 100) onePercent = (100 / totalPages);
                else onePercent = (totalPages / 100);

                HttpClient client;
                HttpResponseMessage response;
                HttpClientHandler handler = new HttpClientHandler();
                handler.AllowAutoRedirect = false;
                foreach (String link in listLinks)
                {
                    try
                    {
                        client = new HttpClient(handler);
                        if (checkBoxUserAgent.Checked == false) client.DefaultRequestHeaders.UserAgent.ParseAdd(textBoxUserAgent.Text);
                        client.BaseAddress = new Uri(link);

                        response = client.GetAsync(link).Result;
                        int statusCode = (int)response.StatusCode;

                        textBoxProcess.AppendText("[" + index.ToString() + "/" + totalPages.ToString() + "] STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine);
                        textBoxProcess.ScrollToCaret();

                        if (statusCode >= 100 && statusCode <= 199)
                        {
                            richTextBox100.Text = richTextBox100.Text + "STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine;
                        }
                        if (statusCode >= 200 && statusCode <= 299)
                        {
                            richTextBox200.Text = richTextBox200.Text + "STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine;
                        }
                        if (statusCode >= 300 && statusCode <= 399)
                        {
                            richTextBox300.Text = richTextBox300.Text + "STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine;
                        }
                        if (statusCode >= 400 && statusCode <= 499)
                        {
                            richTextBox400.Text = richTextBox400.Text + "STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine;
                        }
                        if (statusCode >= 500 && statusCode <= 599)
                        {
                            richTextBox500.Text = richTextBox500.Text + "STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine;
                        }
                        if (statusCode <= 99 || statusCode >= 600)
                        {
                            richTextBoxOther.Text = richTextBoxOther.Text + "STATUS [" + statusCode.ToString() + "]: " + link + Environment.NewLine;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка");
                        richTextBoxOther.Text = richTextBoxOther.Text + "ERROR [" + ex.Message + "]: " + link + Environment.NewLine;
                    }

                    showProgressTest(totalPages, onePercent, index);
                    index++;
                }

                toolStripStatusLabel2.Text = "100%";
                toolStripStatusLabel5.Text = "0:00";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
            TestEnd();
        }

        private void richTextBox100_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try { Process.Start(e.LinkText); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
        }

        private void richTextBox200_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try { Process.Start(e.LinkText); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
        }

        private void richTextBox300_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try { Process.Start(e.LinkText); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
        }

        private void richTextBox400_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try { Process.Start(e.LinkText); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
        }

        private void richTextBox500_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try { Process.Start(e.LinkText); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
        }

        private void richTextBoxOther_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try { Process.Start(e.LinkText); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
        }

        private void вырезатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox100.Cut();
        }

        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox100.Copy();
        }

        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox100.Paste();
        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendKeys.Send("");
        }

        private void выделитьВсёToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox100.SelectAll();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            richTextBox200.Copy();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            richTextBox200.SelectAll();
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            richTextBox300.Copy();
        }

        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            richTextBox300.SelectAll();
        }

        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            richTextBox400.Copy();
        }

        private void toolStripMenuItem15_Click(object sender, EventArgs e)
        {
            richTextBox400.SelectAll();
        }

        private void toolStripMenuItem17_Click(object sender, EventArgs e)
        {
            richTextBox500.Copy();
        }

        private void toolStripMenuItem20_Click(object sender, EventArgs e)
        {
            richTextBox500.SelectAll();
        }

        private void toolStripMenuItem22_Click(object sender, EventArgs e)
        {
            richTextBoxOther.Copy();
        }

        private void toolStripMenuItem25_Click(object sender, EventArgs e)
        {
            richTextBoxOther.SelectAll();
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout about = new FormAbout();
            about.ShowDialog();
        }
    }
}
