using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using IniParser;
using IniParser.Model;

namespace Converter
{
    public partial class MainForm : Form
    {
        public string path;
        public string path1;
        public string path2;
        public string fileName;
        public string[] gk;
        public string[] tk;
        public string[] go;
        public string[] to;
        public FileIniDataParser parser;
        public IniData data;
        public FileIniDataParser parser1;
        public IniData data1;
        public FileIniDataParser parser2;
        public IniData data2;

        public MainForm()
        {
            InitializeComponent();
#if !DEBUG
            button1.Visible = false;
            button2.Visible = false;
#endif
            textBox1.Text = @"term";
            textBox2.Text = @"D:\коэфф. АЦП\";
            textBox3.Text = @"D:\коэфф. АЦП\коэфф. АЦП 7.1.0.7\";
            textBox4.Text = @"D:\коэфф. АЦП\Смещения нуля 7.1.0.7\";
            //ParseFile();
        }

        private void ParseFile()
        {
            fileName = textBox1.Text;
            path = textBox2.Text + fileName + ".txt";
            path1 = textBox3.Text + fileName + ".txt";
            path2 = textBox4.Text + fileName + ".txt";
            parser = new FileIniDataParser();
            if (!File.Exists(path))
                using (File.Create(path)) { }
            data = parser.ReadFile(path);
            parser1 = new FileIniDataParser();
            if (!File.Exists(path1))
                using (File.Create(path1)) { }
            data1 = parser.ReadFile(path1);
            parser2 = new FileIniDataParser();
            if (!File.Exists(path2))
                using (File.Create(path2)) { }
            data2 = parser.ReadFile(path2);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                ParseFile();
                richTextBox1.Clear();
                richTextBox2.Clear();
                int count = data["KoefChanelAdc"].Count;
                gk = new string[count / 2];
                tk = new string[count / 2];
                go = new string[count / 2];
                to = new string[count / 2];
                foreach (SectionData section in data.Sections)
                {
                    int i = 0;
                    foreach (KeyData key in section.Keys)
                    {
                        if (i % 2 == 0)
                        {
                            if (section.SectionName == "KoefChanelAdc")
                                gk[i / 2] = key.Value;
                            else
                                go[i / 2] = key.Value;
                        }
                        else
                        {
                            if (section.SectionName == "KoefChanelAdc")
                                tk[(i - 1) / 2] = key.Value;
                            else
                                to[(i - 1) / 2] = key.Value;
                        }
                        i++;  
                    }
                }
                for (int i = 0; i < count / 2; i++)
                {
                    richTextBox1.AppendText(gk[i] + System.Environment.NewLine);
                    richTextBox1.AppendText(tk[i] + System.Environment.NewLine);
                    richTextBox2.AppendText(go[i] + System.Environment.NewLine);
                    richTextBox2.AppendText(to[i] + System.Environment.NewLine);
                }
                MessageBox.Show("считано");
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                int count = gk[0].Split(';').GetLength(0);
                for (int i = 0; i < gk.GetLength(0); i++)
                {
                    string[] gkSplit = gk[i].Split(';');
                    string[] tkSplit = tk[i].Split(';');
                    string[] goSplit = go[i].Split(';');
                    string[] toSplit = to[i].Split(';');
                    for (int j = 0; j < count; j++)
                    {
                        //data1.Sections.AddSection("E" + (j + 1).ToString());
                        data1["E" + (j + 1).ToString()].AddKey((i + 1).ToString(), gkSplit[j].Trim().Replace(".", ",") + "; " + tkSplit[j].Trim().Replace(".", ",") + "; ; ; ; ; ; ; ; ; ; ; ");
                        //data2.Sections.AddSection("E" + (j + 1).ToString());
                        data2["E" + (j + 1).ToString()].AddKey((i + 1).ToString(), goSplit[j].Trim() + "; " + toSplit[j].Trim() + "; 0; 0; 0; 0; 0; 0; 0; 0; 0; 0;");
                    }
                    parser1.WriteFile(path1, data1);
                    parser2.WriteFile(path2, data2);
                }
                DelEmptyLines(path1);
                DelEmptyLines(path2);
                MessageBox.Show("сохранено");
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            try
            {
                ParseFile();
                richTextBox1.Clear();
                richTextBox2.Clear();
                if (data1.Sections.Count == 0)//если секций нет (файл создан в ТБ200) добавляем секцию [E1] в начало файла
                {
                    AddSection(path1);
                    ParseFile();
                }
                string[] str = new string[data1.Sections.Count];
                string[,] keyName = new string[data1.Sections.Count, 13];
                int i = 0;
                int j;
                foreach (SectionData section in data1.Sections)
                {
                    str[i] = section.SectionName;

                    j = 0;
                    foreach (KeyData key in section.Keys)
                    {
                        int r = Convert.ToInt32(Regex.Replace(key.KeyName.Split('-')[0], @"[^\d]+", ""));//извлекаем число из имени ключа
                        keyName[i, r - 1] = key.KeyName;
                        j++;
                    }
                    i++;
                }
                for (i = 0; i < data1.Sections.Count; i++)
                {
                    for (j = 0; j < 13; j++)
                    {
                        if (keyName[i, j] == null) //если ключ отсутсвует то задаем стандартное название
                            keyName[i, j] = (j + 1).ToString();
                    }
                }
                gk = new string[13];
                tk = new string[13];
                go = new string[13];
                to = new string[13];
                for (i = 0; i < 13; i++)
                {
                    for (j = 0; j < data1.Sections.Count; j++)
                    {
                        string ch = String.Empty;
                        if (j > 0) ch = ";";
                        string values1;
                        string values2;
                        values1 = data1[str[j]][keyName[j, i]];
                        values2 = data2[str[j]][keyName[j, i]];
                        if (values1 == null)
                            values1 = ";";
                        if (values2 == null)
                            values2 = ";";
                        string[] values1Split = values1.Split(';');
                        string[] values2Split = values2.Split(';');
                        if (values1Split[0].Trim() == String.Empty) values1Split[0] = "1.0000";
                        if (values1Split[1].Trim() == String.Empty) values1Split[1] = "1.0000";
                        if (values2Split[0].Trim() == String.Empty) values2Split[0] = "6";
                        if (values2Split[1].Trim() == String.Empty) values2Split[1] = "0";
                        gk[i] += ch + values1Split[0].Trim().Replace(",", ".");
                        tk[i] += ch + values1Split[1].Trim().Replace(",", ".");
                        go[i] += ch + values2Split[0].Trim();
                        to[i] += ch + values2Split[1].Trim();
                    }
                    richTextBox1.AppendText(gk[i] + System.Environment.NewLine + tk[i] + System.Environment.NewLine);
                    richTextBox2.AppendText(go[i] + System.Environment.NewLine + to[i] + System.Environment.NewLine);
                }
                MessageBox.Show("считано");
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            try
            {
                //data.Sections.AddSection("KoefChanelAdc");
                //data.Sections.AddSection("OfsZeroChanelAdc");
                for (int i = 0; i < gk.GetLength(0); i++)
                {
                    data["KoefChanelAdc"].AddKey((i * 2 + 1).ToString(), gk[i]);
                    data["OfsZeroChanelAdc"].AddKey((i * 2 + 1).ToString(), go[i]);
                    data["KoefChanelAdc"].AddKey((i * 2 + 2).ToString(), tk[i]);
                    data["OfsZeroChanelAdc"].AddKey((i * 2 + 2).ToString(), to[i]);
                }
                parser.WriteFile(path, data);
                DelEmptyLines(path);
                MessageBox.Show("сохранено");
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void DelEmptyLines(string path)
        {
            var allLines = File.ReadAllLines(path, Encoding.UTF8).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToArray();
            File.WriteAllLines(path, allLines, Encoding.UTF8);
        }

        private void AddSection(string path)
        {
            var allLines = File.ReadAllLines(path, Encoding.UTF8).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToArray();
            File.WriteAllText(path, "[E1]" + System.Environment.NewLine, Encoding.UTF8);
            File.AppendAllLines(path, allLines, Encoding.UTF8);
        }
    }
}
