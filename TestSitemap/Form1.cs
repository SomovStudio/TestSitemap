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

        private void закрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            textBox3.Text = "";
            toolStripStatusLabel2.Text = DateTime.Now.ToString();
            toolStripStatusLabel4.Text = "0:00";

            try { 
                ArrayList listLinks = readXML(textBox1.Text);
                int count = listLinks.Count;
                int index = 1;
                foreach(String link in listLinks)
                {
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri(link);

                    HttpResponseMessage response = client.GetAsync(link).Result;
                    int statusCode = (int)response.StatusCode;

                    if (statusCode != 200)
                    {
                        textBox3.Text = textBox3.Text + link + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                        textBox3.ScrollToCaret();
                    }
                    String process = textBox2.Text;
                    textBox2.Text = "[" + index.ToString() + " / " + count.ToString() + "] " + link + " STATUS: " + statusCode.ToString() + Environment.NewLine + process;
                    index++;
                    this.Update();
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }

            toolStripStatusLabel4.Text = DateTime.Now.ToString();
        }

        private ArrayList readXML(string filename)
        {
            ArrayList list = new ArrayList();

            XmlDataDocument xmldoc = new XmlDataDocument();
            XmlNodeList xmlnode;
            int i = 0;
            string str = null;
            FileStream fs = new FileStream(@filename, FileMode.Open, FileAccess.Read);
            xmldoc.Load(fs);
            xmlnode = xmldoc.GetElementsByTagName("url");
            for (i = 0; i <= xmlnode.Count - 1; i++)
            {
                string link = xmlnode[i].ChildNodes.Item(0).InnerText.Trim();
                list.Add(link);
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

        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            textBox3.Text = "";
            toolStripStatusLabel2.Text = DateTime.Now.ToString();
            toolStripStatusLabel4.Text = "0:00";

            try
            {
                ArrayList listLinks = readUrlXML(textBox4.Text);
                int count = listLinks.Count;
                int index = 1;
                foreach (String link in listLinks)
                {
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri(link);

                    HttpResponseMessage response = client.GetAsync(link).Result;
                    int statusCode = (int)response.StatusCode;

                    if (statusCode != 200)
                    {
                        textBox3.Text = textBox3.Text + link + " STATUS: " + statusCode.ToString() + Environment.NewLine;
                        textBox3.ScrollToCaret();
                    }
                    String process = textBox2.Text;
                    textBox2.Text = "[" + index.ToString() + " / " + count.ToString() + "] " + link + " STATUS: " + statusCode.ToString() + Environment.NewLine + process;
                    index++;
                    this.Update();
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }            
            toolStripStatusLabel4.Text = DateTime.Now.ToString();
        }

        private ArrayList readUrlXML(string filename)
        {
            ArrayList list = new ArrayList();
 
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(filename);
            XmlElement xRoot = xDoc.DocumentElement;
            foreach (XmlNode xnode in xRoot)
            {
                list.Add(xnode.ChildNodes[0].InnerText);
            }

            return list;
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
    }
}
