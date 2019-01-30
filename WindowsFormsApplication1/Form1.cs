using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net;
using System.Xml;

namespace WindowsFormsApplication1
{

    public partial class Form1 : Form
    {
        static int count = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            webBrowser1.Navigate("http://www.youtube-mp3.org/");
            button1.Enabled = false;
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            String text = webBrowser1.DocumentText;
            button2.Enabled = false;
            if (text.Contains("submit-form"))
                button1.Enabled = true;
            else
            {
                button1.Enabled = false;
                MessageBox.Show("אירעה שגיאהת נסה שוב מאוחר יותר", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
            }
        }
        string videoId = null;
        private void button1_Click(object sender, EventArgs e)
        {
            webBrowser1.Document.GetElementById("youtube-url").SetAttribute("value", textBox1.Text);
            webBrowser1.Document.GetElementById("submit").InvokeMember("click");

            if (!textBox1.Text.Equals(""))
            {
                if (textBox1.Text.StartsWith("https://www.youtube.com/watch?v=") || textBox1.Text.StartsWith("http://www.youtube.com/watch?v="))
                {
                    HtmlElement el = webBrowser1.Document.GetElementById("title");
                    string s = el.InnerText; // get the : Title: **Developers**
                    Console.WriteLine(s);
                    String[] text1 = textBox1.Text.Split('=');
                    text1[0] = s;
                    Console.WriteLine(text1[0]);
                    String img = text1[1];
                    videoId = text1[1];
                    Console.WriteLine(videoId);
                    if (img.Equals(""))
                        MessageBox.Show("כתובת וידאו לא תקנית", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
                    else
                    {
                        try
                        {
                            pictureBox1.Load("http://i.ytimg.com/vi/" + img + "/default.jpg");
                        }
                        catch (SystemException exc)
                        {
                            pictureBox1.Image = null;
                            ShowError(MessageBoxOptions.RightAlign);
                            Console.WriteLine(exc.ToString());
                        }
                        // button1.Enabled = false;
                        button2.Enabled = true;

                    }
                }
                else
                    MessageBox.Show("כתובת וידאו לא תקנית\nכתובת תקנית חייבת להתחיל כמו בדוגמא\nhttps://www.youtube.com/watch?v=", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
            }
            else
                MessageBox.Show("לא ניתן להשאיר שדה כתובת ריק", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
        }

        private void button2_Click(object sender, EventArgs e)
        {

            HtmlElement dl_link = webBrowser1.Document.GetElementById("dl_link");
            HtmlElementCollection links = dl_link.GetElementsByTagName("a");
            String url = null;
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    if (links[i].GetAttribute("href").Contains("get?ab="))
                    {
                        url = links[i].GetAttribute("href");
                        break;
                    }
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            try
            {
                WebClient download = new WebClient();
                SaveFileDialog savefile = new SaveFileDialog();
                savefile.FileName = videoId;
                savefile.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
                if (savefile.ShowDialog() == DialogResult.OK)
                    download.DownloadFile(url, savefile.FileName);
            }
            catch
            {
                ShowError(MessageBoxOptions.RightAlign);
            }
        }
        public void ShowError(MessageBoxOptions options)
        {
            MessageBox.Show(".אירעה שגיאה, נסה שוב מאוחר יותר\nשגיאה זו נגרמת בדרך כלל עקב בעיות זכויות יוצרים או מפאת אורכו של הווידאו\nאנו תומכים רק בקטעי וידאו עם מקסימום של 20 דקות", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, options);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                this.TopMost = true;
            if (!checkBox1.Checked)
                this.TopMost = false;
        }

        public void PasteLink()
        {
            String link = Clipboard.GetText();
            if (link.Contains("youtube.com"))
                textBox1.Text = link;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            PasteLink();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
                System.Diagnostics.Process.Start(textBox1.Text);

        }

        private String BindVideo(string videoId)
        {
            string sViewCount = string.Empty;
            string sTitle = string.Empty;
            bool bPublished = false;
            bool bTitle = false;
            try
            {
                XmlTextReader reader = new XmlTextReader("http://gdata.youtube.com/feed/api/videos/" + videoId);
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            {
                                switch (reader.Name)
                                {
                                    case "published":
                                        bPublished = true;
                                        break;
                                    case "yt:statistics":
                                        {
                                            while (reader.MoveToNextAttribute())
                                            {
                                                if (reader.Name.ToLower().Equals("viewcount"))
                                                    sViewCount = reader.Value;
                                            }
                                        }
                                        break;
                                    case "media:title":
                                        bTitle = true;
                                        break;
                                }
                                break;
                            }
                        case XmlNodeType.Text:
                            if (bPublished)
                            {
                               var dtPublishDate = Convert.ToDateTime(reader.Value);
                            }
                            else if (bTitle)
                            {
                                sTitle = reader.Value;
                                Console.WriteLine(sTitle);
                            }
                            break;
                        case XmlNodeType.EndElement:
                            {
                                bPublished = false;
                                bTitle = false;
                                break;
                            }
                    }
                }
                reader.Close();
                Console.WriteLine(sViewCount);
                Console.WriteLine(sTitle);
                Console.WriteLine(bTitle);
                Console.WriteLine(bPublished);
            }
            catch { }
            return sTitle;
        }
       
    }
}