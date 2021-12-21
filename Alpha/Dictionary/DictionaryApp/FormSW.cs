﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace DictionaryApp
{
    public partial class FormSW : Form
    {
        private readonly Database.DatabaseHandle databaseHandle = new Database.DatabaseHandle();
        private Classes.Word GetWord = new Classes.Word();
        private string meaning = "";
        private string word = "";
        private string shuffle;
        private int select;
        private int desX, desY, disX, disY;
        private int move = 0;
        private int time, count_time;
        public static int number_question;
        private int count_question;
        private CustomControls.BtnChar tempBtnChar;
        private List<CustomControls.BtnChar> listBtn;
        private List<CustomControls.BtnChar> listAnswer;
        private readonly Random random = new Random();
        public static int total = 0, correct = 0;
        public static double avg_time = 0;
        private readonly Stopwatch stopwatch = new Stopwatch();
        private bool submitted = false;

        public FormSW()
        {
            InitializeComponent();
            Bounds = Screen.PrimaryScreen.Bounds;
            lb_Meaning.Location = new Point((Width - lb_Meaning.Width) / 2, Height * 30 / 100);
            do
            {
                GetWord = databaseHandle.RandomWord();
                word = GetWord.wordHeader.word;
            } while (word.Length >= 15);
            Debug.WriteLine(word);
            meaning = GetWord.senses[random.Next(0, GetWord.senses.Count)].meaning.Replace("=", string.Empty);
            meaning = meaning[0].ToString().ToUpper() + meaning.Substring(1);
            lb_Meaning.Text = meaning;
            count_question = 1;
            count_time = time = FormSWSettings.time;          
            number_question = FormSWSettings.number_question;
            lb_TimeLeftNum.Text = count_time.ToString();
            timer2.Start();
            Init();
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void Init()
        {
            InitListButton();
            InitlistBtn();
            select = 0;
        }

        private void ButtonChar_Click(object sender, EventArgs e)
        {
            CustomControls.BtnChar btn = (CustomControls.BtnChar)sender;
            int mid = word.Length / 2;
            if (btn.X == btn.Location.X && btn.Y == btn.Location.Y)
            {
                desX = (Width - btn.Width) / 2 - ((mid - select) * 2 - 1) * btn.Width;
                desY = Height * 60 / 100;
                disX = Math.Abs(desX - btn.Location.X);
                disY = Math.Abs(desY - btn.Location.Y);
                btn.Selected = select;
                listAnswer.Add(btn);
                select++;
                move = 1;
                for (int i = 0; i < listBtn.Count; i++)
                {
                    listBtn[i].Enabled = false;
                    listBtn[i].BackColor = Color.Yellow;
                }
                timer1.Start();
            }
            else
            {
                listAnswer.Remove(btn);
                for (int i = btn.Selected; i < listAnswer.Count; i++)
                {
                    listAnswer[i].Selected = i;
                    int x = (Width - btn.Width) / 2 - ((mid - i) * 2 - 1) * btn.Width;
                    int y = Height * 60 / 100;
                    listAnswer[i].Location = new Point(x, y);
                }
                desX = btn.X;
                desY = btn.Y;
                disX = Math.Abs(btn.Location.X - desX);
                disY = Math.Abs(btn.Location.Y - desY);
                btn.Selected = -1;
                select--;
                move = -1;
                tempBtnChar = btn;
                for (int i = 0; i < listBtn.Count; i++)
                {
                    listBtn[i].Enabled = false;
                    listBtn[i].BackColor = Color.Yellow;
                }
                timer1.Start();
            }
        }

        private void Submit()
        {
            timer2.Stop();
            stopwatch.Stop();
            avg_time += stopwatch.ElapsedMilliseconds;
            string answer = "";

            if (!submitted && count_question < FormSWSettings.number_question)
            {
                submitted = true;
                btn_Submit.Text = "Next";
                if (select == word.Length)
                {
                    for (int i = 0; i < word.Length; i++)
                    {
                        answer += listAnswer[i].Text;
                    }
                    if (answer == word)
                    {
                        correct++;
                        int score = (int)(count_time * (double)(100 / time));
                        lb_Score.Text = (int.Parse(lb_Score.Text) + score).ToString();
                        AnswerCorrect();
                    }
                    else
                    {
                        AnswerIncorrect();
                    }
                }
                else
                {
                    AnswerIncorrect();
                }

                if (count_question == FormSWSettings.number_question)
                {
                    btn_Submit.Text = "Finish";
                }

                return;
            }

            if (submitted && count_question < FormSWSettings.number_question)
            {
                btn_Submit.Text = "Submit";
                count_question++;
                lb_TimeLeftNum.ForeColor = Color.Black;
                LabelTimeLeft.ForeColor = Color.Black;
                LabelResult.Visible = false;
                lb_Question.Text = (int.Parse(lb_Question.Text) + 1).ToString();
                listAnswer.Clear();
                lb_TimeLeftNum.Text = FormSWSettings.time.ToString();
                for (int i = 0; i < word.Length; i++)
                {
                    Controls.Remove(listBtn[i]);
                }
                listBtn.Clear();
                do
                {
                    GetWord = databaseHandle.RandomWord();
                    word = GetWord.wordHeader.word;
                } while (word.Length >= 15);
                meaning = GetWord.senses[random.Next(0, GetWord.senses.Count)].meaning.Replace("=", string.Empty); ;
                meaning = meaning[0].ToString().ToUpper() + meaning.Substring(1);
                lb_Meaning.Text = meaning;
                count_time = time;
                timer2.Start();
                stopwatch.Reset();
                stopwatch.Start();
                Init();
                submitted = false;
            }
            else
            {
                total = int.Parse(lb_Score.Text);
                SaveProgress();
                this.Close();
            }
        }

        private void InitListButton()
        {
            listBtn = new List<CustomControls.BtnChar>();
            for (int i = 0; i < word.Length; i++)
            {
                CustomControls.BtnChar btn = new CustomControls.BtnChar();
                listBtn.Add(btn);
            }
            listAnswer = new List<CustomControls.BtnChar>();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {            
            count_time--;
            lb_TimeLeftNum.Text = count_time.ToString() + "s";
            if (count_time <= 5)
            {
                lb_TimeLeftNum.ForeColor = Color.Red;
                LabelTimeLeft.ForeColor = Color.Red;
            }
            if (count_time == 0)
            {
                timer2.Stop();
                Submit();       
            }
        }

        private void InitlistBtn()
        {
            Debug.WriteLine(word);
            shuffle = new string(word.ToCharArray().OrderBy(s => (random.Next(2) % 2) == 0).ToArray());
            //Debug.WriteLine(shuffle);
            int mid = shuffle.Length / 2;
            for (int i = 0; i < shuffle.Length; i++)
            {
                listBtn[i].FlatStyle = FlatStyle.Flat;
                listBtn[i].TabStop = false;
                listBtn[i].Text = shuffle[i].ToString();
                listBtn[i].Height = 60;
                listBtn[i].Width = 60;
                listBtn[i].BackColor = Color.Yellow;
                listBtn[i].Font = new Font("Mongolian Baiti", 20, FontStyle.Bold);
                listBtn[i].ID1 = i;
                if (word.Length % 2 == 0)
                {
                    listBtn[i].X = (Width - listBtn[i].Width) / 2 - ((mid - i) * 2 - 1) * listBtn[i].Width;
                    listBtn[i].Y = Height * 80 / 100;
                }
                else
                {
                    listBtn[i].X = (Width - listBtn[i].Width) / 2 - ((mid - i) * 2 - 1) * listBtn[i].Width;
                    listBtn[i].Y = Height * 80 / 100;
                }
                listBtn[i].Location = new Point(listBtn[i].X, listBtn[i].Y);
                listBtn[i].Visible = true;
                Controls.Add(listBtn[i]);
                listBtn[i].Click += new EventHandler(ButtonChar_Click);
            }
        }
        private void btn_Submit_Click(object sender, EventArgs e)
        {
            Submit();           
        }

        private void FormSW_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (new StackTrace().GetFrames().Any(x => x.GetMethod().Name == "btn_Submit_Click"))
            {
                FormGameResult fgr = new FormGameResult();
                fgr.Show();
            }
            else if (new StackTrace().GetFrames().Any(x => x.GetMethod().Name == "ButtonQuit_Click"))
            {
                FormGamesSelect fgr = new FormGamesSelect();
                fgr.Show();
            }
            else
                Application.Exit();
        }

        private void ButtonQuit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void AnswerCorrect()
        {
            for (int i = 0; i < listBtn.Count; i++)
            {
                listBtn[i].BackColor = Color.Green;
            }
            LabelResult.Visible = true;
            LabelResult.Text = "Correct!";
        }

        private void AnswerIncorrect()
        {
            for (int i = 0; i < listBtn.Count; i++)
            {
                listBtn[i].BackColor = Color.Red;
            }
            LabelResult.Visible = true;
            LabelResult.Text = "The correct answer is " + word;
        }
        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int x = disX / 15;
            int y = disY / 15;
            if (move > 0)
            {
                int X, Y;
                if (listAnswer[select - 1].Location.X < desX)
                {
                    X = (int)listAnswer[select - 1].Location.X + x;
                }
                else
                {
                    X = (int)listAnswer[select - 1].Location.X - x;
                }
                if (listAnswer[select - 1].Location.Y < desY)
                {
                    Y = listAnswer[select - 1].Location.Y + y;
                }
                else
                {
                    Y = listAnswer[select - 1].Location.Y - y;
                }
                listAnswer[select - 1].Location = new Point(X, Y);
                if (Math.Abs(desX - listAnswer[select - 1].Location.X) < x || Math.Abs(desY - listAnswer[select - 1].Location.Y) < y)
                {
                    listAnswer[select - 1].Location = new Point(desX, desY);
                    move = 0;
                    for (int i = 0; i < listBtn.Count; i++)
                    {
                        listBtn[i].Enabled = true;
                    }
                    timer1.Stop();
                }
            }
            if (move < 0)
            {
                int X, Y;
                if (tempBtnChar.Location.X > desX)
                {
                    X = tempBtnChar.Location.X - x;
                }
                else
                {
                    X = tempBtnChar.Location.X + x;
                }
                if (tempBtnChar.Location.Y > desY)
                {
                    Y = tempBtnChar.Location.Y - y;
                }
                else
                {
                    Y = tempBtnChar.Location.Y + y;
                }
                tempBtnChar.Location = new Point(X, Y);
                if (Math.Abs(desX - tempBtnChar.Location.X) < x || Math.Abs(desY - tempBtnChar.Location.Y) < y)
                {
                    tempBtnChar.Location = new Point(desX, desY);
                    move = 0;
                    for (int i = 0; i < listBtn.Count; i++)
                    {
                        listBtn[i].Enabled = true;
                    }
                    timer1.Stop();
                }
            }
        }

        private void SaveProgress()
        {
            XmlDocument xmlDocument = new XmlDocument();
            if (!File.Exists(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().Length - 9) + "Saved\\sw.xml"))
            {
                XmlElement root = xmlDocument.DocumentElement;

                XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDocument.InsertBefore(xmlDeclaration, root);

                XmlElement mainElement = xmlDocument.CreateElement(string.Empty, "MainInfo", string.Empty);
                mainElement.InnerText = "Do not modify the contents of this file, or risk losing your saved progress!";
                xmlDocument.AppendChild(mainElement);

                XmlElement bestScore = xmlDocument.CreateElement(string.Empty, "BestScore", string.Empty);
                bestScore.InnerText = total.ToString();
                mainElement.AppendChild(bestScore);
            }
            else
            {
                bool success = true;
                try
                {
                    xmlDocument.Load(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().Length - 9) + "Saved\\sw.xml");
                }
                catch (XmlException exception)
                {
                    success = false;
                    Debug.WriteLine(exception.Message);
                }
                if (success)
                {
                    XmlNode bestScore = xmlDocument.DocumentElement.SelectSingleNode("/MainInfo/BestScore");
                    if (bestScore != null)
                    {
                        if (double.TryParse(bestScore.InnerText, out double s))
                        {
                            if (total > s)
                                bestScore.InnerText = total.ToString();
                        }
                        else
                            bestScore.InnerText = total.ToString();
                    }
                    else
                    {
                        XmlNode mainElement = xmlDocument.SelectSingleNode("/MainInfo");
                        bestScore = xmlDocument.CreateElement(string.Empty, "BestScore", string.Empty);
                        bestScore.InnerText = total.ToString();
                        mainElement.AppendChild(bestScore);
                    }
                }
                else
                {
                    XmlElement root = xmlDocument.DocumentElement;

                    XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
                    xmlDocument.InsertBefore(xmlDeclaration, root);

                    XmlElement mainElement = xmlDocument.CreateElement(string.Empty, "MainInfo", string.Empty);
                    mainElement.InnerText = "Do not modify the contents of this file, or risk losing your saved progress!";
                    xmlDocument.AppendChild(mainElement);

                    XmlElement bestScore = xmlDocument.CreateElement(string.Empty, "BestScore", string.Empty);
                    bestScore.InnerText = total.ToString();
                    mainElement.AppendChild(bestScore);
                }
            }
            xmlDocument.Save(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().Length - 9) + "Saved\\sw.xml");
        }
    }
}