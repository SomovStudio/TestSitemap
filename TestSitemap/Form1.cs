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
        public Form1()
        {
            InitializeComponent();
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
            textBox2.Text = "";
            textBox3.Text = "";
            toolStripStatusLabel2.Text = DateTime.Now.ToString();
            toolStripStatusLabel4.Text = "0:00";

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
                XmlDataDocument xmldoc = new XmlDataDocument();
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
            textBox2.Text = "";
            textBox3.Text = "";
            toolStripStatusLabel2.Text = DateTime.Now.ToString();
            toolStripStatusLabel4.Text = "0:00";

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
            try
            {
                ArrayList listLinks = readUrlXML(textBox4.Text);
                int count = listLinks.Count;
                int index = 1;
                String process = "";
                HttpClient client;
                HttpResponseMessage response;
                foreach (String link in listLinks)
                {
                    client = new HttpClient();
                    client.BaseAddress = new Uri(link);

                    response = client.GetAsync(link).Result;
                    int statusCode = (int)response.StatusCode;
                    if (statusCode != 200)
                    {
                        Action action3 = () => textBox3.Text = textBox3.Text + link + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                        textBox3.Invoke(action3);
                    }

                    process = textBox2.Text;
                    Action action2 = () => textBox2.Text = "[" + index.ToString() + " / " + count.ToString() + "] " + link + " STATUS: " + statusCode.ToString() + Environment.NewLine + process;
                    textBox2.Invoke(action2);
                    index++;
                }
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
            try
            {
                ArrayList listLinks = readXML(textBox1.Text);
                int count = listLinks.Count;
                int index = 1;
                String process = "";
                HttpClient client;
                HttpResponseMessage response;
                foreach (String link in listLinks)
                {
                    client = new HttpClient();
                    client.BaseAddress = new Uri(link);

                    response = client.GetAsync(link).Result;
                    int statusCode = (int)response.StatusCode;
                    if (statusCode != 200)
                    {
                        Action action3 = () => textBox3.Text = textBox3.Text + link + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                        textBox3.Invoke(action3);
                    }

                    process = textBox2.Text;
                    Action action2 = () => textBox2.Text = "[" + index.ToString() + " / " + count.ToString() + "] " + link + " STATUS: " + statusCode.ToString() + Environment.NewLine + process;
                    textBox2.Invoke(action2);
                    index++;
                }
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
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(filename);
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
    }
}
