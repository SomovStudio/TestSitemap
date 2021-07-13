using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml;
using System.IO;
using System.Threading;

namespace TestSitemap
{
    public partial class Form1 : Form
    {
        private bool processRun = false;

        private void showProgressTest(double totalPages, double onePercent, double step)
        {
            toolStripProgressBar1.Maximum = Convert.ToInt32(totalPages);
            toolStripProgressBar1.Value = Convert.ToInt32(step);
            double progressPercent = 0;
            if (totalPages < 100 && onePercent > 0) progressPercent = (step * onePercent);
            if (totalPages >= 100) progressPercent = (step / onePercent);
            progressPercent = Math.Round(progressPercent, 0);
            if(progressPercent < 100) toolStripStatusLabel6.Text = Convert.ToString(progressPercent) + "%";
            else toolStripStatusLabel6.Text = "99%";

            double dSec = (totalPages - step) * 1;

            int sec = Convert.ToInt32(dSec);
            int minutes = sec / 60;
            int newSec = sec - minutes * 60;
            int hour = minutes / 60;
            int newMinnutes = minutes - hour * 60;
            TimeSpan time = new TimeSpan(hour, newMinnutes, newSec);
            toolStripStatusLabel8.Text = "Осталось: " + time.ToString();
        }

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                thread.Abort();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void закрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            thread = new Thread(TestUrl);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (processRun == true)
            {
                MessageBox.Show("Процесс уже запущен");
                return;
            }

            textBox2.Text = "";
            textBox3.Text = "";
            toolStripStatusLabel2.Text = DateTime.Now.ToString();
            toolStripStatusLabel4.Text = "0:00";
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Maximum = 0;
            toolStripStatusLabel6.Text = "0%";
            toolStripStatusLabel8.Text = "0:00";

            CheckLocal();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            TestEnd();
            try
            {
                thread.Abort();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
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
                for (i = 0; i <= xmlnode.Count - 1; i++)
                {
                    for(int j = 0; j <= xmlnode[i].ChildNodes.Count; j++)
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
            catch (Exception error)
            {
                if (подробноеОписаниеОшибокToolStripMenuItem.Checked == false) MessageBox.Show(error.Message);
                else MessageBox.Show(error.ToString());
            }
                        
            return list;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private Thread thread;

        private void button3_Click(object sender, EventArgs e)
        {
            if(processRun == true)
            {
                MessageBox.Show("Процесс уже запущен");
                return;
            }
            
            textBox2.Text = "";
            textBox3.Text = "";
            toolStripStatusLabel2.Text = DateTime.Now.ToString();
            toolStripStatusLabel4.Text = "0:00";
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Maximum = 0;
            toolStripStatusLabel6.Text = "0%";
            toolStripStatusLabel8.Text = "0:00";

            CheckURL();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            TestEnd();
            try
            {
                thread.Abort();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }
        
        private void CheckURL()
        {
            //https://rsdn.org/article/dotnet/CSThreading1.xml
            //http://www.cyberforum.ru/windows-forms/thread642295.html
            thread = new Thread(TestUrl);
            thread.Start();
        }

        private void TestUrl()
        {
            processRun = true;
            try
            {
                ArrayList listLinks = readUrlXML(textBox4.Text);
                int count = listLinks.Count;
                int index = 1;
                double totalPages = count;
                double onePercent = 0;
                if (totalPages < 100) onePercent = (100 / totalPages);
                else onePercent = (totalPages / 100);

                String process = "";
                HttpClient client;
                HttpResponseMessage response;
                HttpClientHandler handler = new HttpClientHandler();
                handler.AllowAutoRedirect = false;
                foreach (String link in listLinks)
                {
                    client = new HttpClient(handler);
                    if (checkBox1.Checked == false) client.DefaultRequestHeaders.UserAgent.ParseAdd(textBox6.Text);
                    client.BaseAddress = new Uri(link);

                    response = client.GetAsync(link).Result;
                    int statusCode = (int)response.StatusCode;
                    if (statusCode != 200)
                    {
                        //Action action3 = () => textBox3.Text = textBox3.Text + link + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                        //textBox3.Invoke(action3);
                        if (всеОтветыКоторыеНе200ToolStripMenuItem.Checked == false)
                        {
                            if (statusCode >= 300 && statusCode <= 399 && учитывать300еОтветыКакОшибкиToolStripMenuItem.Checked == true) textBox3.Text = textBox3.Text + link + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                            if (statusCode >= 400 && statusCode <= 499 && учитывать400еОтветыКакОшибкиToolStripMenuItem.Checked == true) textBox3.Text = textBox3.Text + link + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                            if (statusCode >= 500 && statusCode <= 599 && учитывать500еОтветыКакОшибкиToolStripMenuItem.Checked == true) textBox3.Text = textBox3.Text + link + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                            if (statusCode >= 600) textBox3.Text = textBox3.Text + link + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                        }
                        else
                        {
                            textBox3.Text = textBox3.Text + link.ToString() + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                        }
                    }

                    process = textBox2.Text;
                    //Action action2 = () => textBox2.Text = "[" + index.ToString() + " / " + count.ToString() + "] " + link + " STATUS: " + statusCode.ToString() + Environment.NewLine + process;
                    //textBox2.Invoke(action2);
                    textBox2.Text = "[" + index.ToString() + " / " + count.ToString() + "] " + link + " STATUS: " + statusCode.ToString() + Environment.NewLine + process;

                    showProgressTest(totalPages, onePercent, index);
                    index++;
                }

                toolStripStatusLabel6.Text = "100%";
                toolStripStatusLabel8.Text = "0:00";
            }
            catch (Exception error)
            {
                if (подробноеОписаниеОшибокToolStripMenuItem.Checked == false) MessageBox.Show(error.Message);
                else MessageBox.Show(error.ToString());
            }
            finally
            {
                TestEnd();
                thread.Abort();
            }

            TestEnd();
            thread.Abort();
        }

        private void CheckLocal()
        {
            //https://rsdn.org/article/dotnet/CSThreading1.xml
            //http://www.cyberforum.ru/windows-forms/thread642295.html
            thread = new Thread(TestLocal);
            thread.Start();
        }

        private void TestLocal()
        {
            processRun = true;
            try
            {
                ArrayList listLinks = readXML(textBox1.Text);
                int count = listLinks.Count;
                int index = 1;
                double totalPages = count;
                double onePercent = 0;
                if (totalPages < 100) onePercent = (100 / totalPages);
                else onePercent = (totalPages / 100);

                String process = "";
                HttpClient client;
                HttpResponseMessage response;
                HttpClientHandler handler = new HttpClientHandler();
                handler.AllowAutoRedirect = false;
                foreach (String link in listLinks)
                {
                    client = new HttpClient(handler);
                    if (checkBox2.Checked == false) client.DefaultRequestHeaders.UserAgent.ParseAdd(textBox7.Text);
                    client.BaseAddress = new Uri(link);

                    response = client.GetAsync(link).Result;
                    int statusCode = (int)response.StatusCode;
                    if (statusCode != 200)
                    {
                        //Action action3 = () => textBox3.Text = textBox3.Text + link + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                        //textBox3.Invoke(action3);
                        if (всеОтветыКоторыеНе200ToolStripMenuItem.Checked == false)
                        {
                            if (statusCode >= 300 && statusCode <= 399 && учитывать300еОтветыКакОшибкиToolStripMenuItem.Checked == true) textBox3.Text = textBox3.Text + link + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                            if (statusCode >= 400 && statusCode <= 499 && учитывать400еОтветыКакОшибкиToolStripMenuItem.Checked == true) textBox3.Text = textBox3.Text + link + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                            if (statusCode >= 500 && statusCode <= 599 && учитывать500еОтветыКакОшибкиToolStripMenuItem.Checked == true) textBox3.Text = textBox3.Text + link + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                            if (statusCode >= 600) textBox3.Text = textBox3.Text + link + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                        }
                        else
                        {
                            textBox3.Text = textBox3.Text + link.ToString() + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                        }
                    }

                    process = textBox2.Text;
                    //Action action2 = () => textBox2.Text = "[" + index.ToString() + " / " + count.ToString() + "] " + link + " STATUS: " + statusCode.ToString() + Environment.NewLine + process;
                    //textBox2.Invoke(action2);
                    textBox2.Text = "[" + index.ToString() + " / " + count.ToString() + "] " + link + " STATUS: " + statusCode.ToString() + Environment.NewLine + process;

                    showProgressTest(totalPages, onePercent, index);
                    index++;
                }

                toolStripStatusLabel6.Text = "100%";
                toolStripStatusLabel8.Text = "0:00";
            }
            catch (Exception error)
            {
                if (подробноеОписаниеОшибокToolStripMenuItem.Checked == false) MessageBox.Show(error.Message);
                else MessageBox.Show(error.ToString());
            }
            finally
            {
                TestEnd();
                thread.Abort();
            }
            TestEnd();
            thread.Abort();
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
                if (checkBox1.Checked == true)
                {
                    xDoc = new XmlDocument();
                    xDoc.Load(filename);
                }
                else
                {
                    WebClient client = new WebClient();
                    client.Headers["User-Agent"] = textBox6.Text;
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
            catch (Exception error)
            {
                if (подробноеОписаниеОшибокToolStripMenuItem.Checked == false) MessageBox.Show(error.Message);
                else MessageBox.Show(error.ToString());
            }
            
            return list;
        }

        private void TestEnd()
        {
            toolStripStatusLabel4.Text = DateTime.Now.ToString();
            MessageBox.Show("Процесс проверки - завершен!");
            processRun = false;
        }

        private void сохранитьСписокПроверкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamWriter SW = new StreamWriter(new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write));
                SW.Write(textBox2.Text);
                SW.Close();
            }
        }

        private void сохранитьСписокОшибокToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamWriter SW = new StreamWriter(new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write));
                SW.Write(textBox3.Text);
                SW.Close();
            }
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm about = new AboutForm();
            about.ShowDialog();
        }

        private void подробноеОписаниеОшибокToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(подробноеОписаниеОшибокToolStripMenuItem.Checked == false) подробноеОписаниеОшибокToolStripMenuItem.Checked = true;
            else подробноеОписаниеОшибокToolStripMenuItem.Checked = false;
        }

        private void валидаторXMLSitemapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(@"https://www.xml-sitemaps.com/validate-xml-sitemap.html");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                textBox5.Text = openFileDialog2.FileName;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (processRun == true)
            {
                MessageBox.Show("Процесс уже запущен");
                return;
            }

            textBox2.Text = "";
            textBox3.Text = "";
            toolStripStatusLabel2.Text = DateTime.Now.ToString();
            toolStripStatusLabel4.Text = "0:00";
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Maximum = 0;
            toolStripStatusLabel6.Text = "0%";
            toolStripStatusLabel8.Text = "0:00";

            CheckLocal2();
        }

        private void CheckLocal2()
        {
            thread = new Thread(TestLocal2);
            thread.Start();
        }

        private void TestLocal2()
        {
            processRun = true;
            try
            {
                ArrayList listLinks = new ArrayList();
                string path = @textBox5.Text;
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

                String process = "";
                HttpClient client;
                HttpResponseMessage response;
                HttpClientHandler handler = new HttpClientHandler();
                handler.AllowAutoRedirect = false;
                foreach (String link in listLinks)
                {
                    client = new HttpClient(handler);
                    if (checkBox3.Checked == false) client.DefaultRequestHeaders.UserAgent.ParseAdd(textBox8.Text);
                    client.BaseAddress = new Uri(link.ToString());

                    response = client.GetAsync(link.ToString()).Result;
                    int statusCode = (int)response.StatusCode;
                    if (statusCode != 200)
                    {
                        //Action action3 = () => textBox3.Text = textBox3.Text + link.ToString() + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                        //textBox3.Invoke(action3);
                        if (всеОтветыКоторыеНе200ToolStripMenuItem.Checked == false)
                        {
                            if (statusCode >= 300 && statusCode <= 399 && учитывать300еОтветыКакОшибкиToolStripMenuItem.Checked == true) textBox3.Text = textBox3.Text + link + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                            if (statusCode >= 400 && statusCode <= 499 && учитывать400еОтветыКакОшибкиToolStripMenuItem.Checked == true) textBox3.Text = textBox3.Text + link + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                            if (statusCode >= 500 && statusCode <= 599 && учитывать500еОтветыКакОшибкиToolStripMenuItem.Checked == true) textBox3.Text = textBox3.Text + link + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                            if (statusCode >= 600) textBox3.Text = textBox3.Text + link + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                        }
                        else
                        {
                            textBox3.Text = textBox3.Text + link.ToString() + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                        }
                        
                    }

                    process = textBox2.Text;
                    //Action action2 = () => textBox2.Text = "[" + index.ToString() + " / " + count.ToString() + "] " + link.ToString() + " STATUS: " + statusCode.ToString() + Environment.NewLine + process;
                    //textBox2.Invoke(action2);
                    textBox2.Text = "[" + index.ToString() + " / " + count.ToString() + "] " + link.ToString() + " STATUS: " + statusCode.ToString() + Environment.NewLine + process;

                    showProgressTest(totalPages, onePercent, index);
                    index++;
                }

                toolStripStatusLabel6.Text = "100%";
                toolStripStatusLabel8.Text = "0:00";
            }
            catch (Exception error)
            {
                if (подробноеОписаниеОшибокToolStripMenuItem.Checked == false) MessageBox.Show(error.Message);
                else MessageBox.Show(error.ToString());
            }
            finally
            {
                TestEnd();
                thread.Abort();
            }

            TestEnd();
            thread.Abort();

        }

        private void button6_Click(object sender, EventArgs e)
        {
            TestEnd();
            try
            {
                thread.Abort();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked == true) textBox6.Enabled = false;
            else textBox6.Enabled = true;

            if (checkBox1.Checked == true) textBox6.Text = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1";
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true) textBox7.Enabled = false;
            else textBox7.Enabled = true;

            if (checkBox2.Checked == true) textBox7.Text = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1";
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked == true) textBox8.Enabled = false;
            else textBox8.Enabled = true;

            if (checkBox3.Checked == true) textBox8.Text = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1";
        }

        private void валидаторXMLSitemapToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(@"https://www.mysitemapgenerator.com/ru/service/check.html");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void учитывать300еОтветыКакОшибкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (учитывать300еОтветыКакОшибкиToolStripMenuItem.Checked) учитывать300еОтветыКакОшибкиToolStripMenuItem.Checked = false;
            else учитывать300еОтветыКакОшибкиToolStripMenuItem.Checked = true;
            всеОтветыКоторыеНе200ToolStripMenuItem.Checked = false;
        }

        private void учитывать400еОтветыКакОшибкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (учитывать400еОтветыКакОшибкиToolStripMenuItem.Checked) учитывать400еОтветыКакОшибкиToolStripMenuItem.Checked = false;
            else учитывать400еОтветыКакОшибкиToolStripMenuItem.Checked = true;
            всеОтветыКоторыеНе200ToolStripMenuItem.Checked = false;
        }

        private void учитывать500еОтветыКакОшибкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (учитывать500еОтветыКакОшибкиToolStripMenuItem.Checked) учитывать500еОтветыКакОшибкиToolStripMenuItem.Checked = false;
            else учитывать500еОтветыКакОшибкиToolStripMenuItem.Checked = true;
            всеОтветыКоторыеНе200ToolStripMenuItem.Checked = false;
        }

        private void всеОтветыКоторыеНе200ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (всеОтветыКоторыеНе200ToolStripMenuItem.Checked)
            {
                всеОтветыКоторыеНе200ToolStripMenuItem.Checked = false;
            }
            else
            {
                всеОтветыКоторыеНе200ToolStripMenuItem.Checked = true;
                учитывать300еОтветыКакОшибкиToolStripMenuItem.Checked = false;
                учитывать400еОтветыКакОшибкиToolStripMenuItem.Checked = false;
                учитывать500еОтветыКакОшибкиToolStripMenuItem.Checked = false;
            }
        }
    }
}
