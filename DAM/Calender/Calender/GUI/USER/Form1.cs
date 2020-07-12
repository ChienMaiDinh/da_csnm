using Calender.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;

namespace Calender
{
    public partial class Form1 : Form
    {
        #region Properties   
        private PlanItem_BO context;
        private User_BO context2;
        public int UserId { get; set; }
        private int appTime;
        IPEndPoint IP;
        Socket client;
        public int AppTime
        {
            get { return appTime; }
            set { appTime = value; }
        }

        private List<List<Button>> matrix;

        public List<List<Button>> Matrix
        {
            get { return matrix; }
            set { matrix = value; }
        }

        private PlanData job;

        public PlanData Job
        {
            get { return job; }
            set { job = value; }
        }

        private List<string> dateOfWeek = new List<string>() { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        #endregion

        public Form1(int userId)
        {
            context = new PlanItem_BO();
            context2 = new User_BO();
            InitializeComponent();
            Connect();
            UserId = userId;
            //RegistryKey regkey = Registry.CurrentUser.CreateSubKey("Software\\LapLich");
            //mo registry khoi dong cung win
            //RegistryKey regstart = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            string keyvalue = "1";
            //try
            //{
            //chen gia tri key
            //regkey.SetValue("Index", keyvalue);
            //regstart.SetValue("LapLich", Application.StartupPath + "\\Lập lịch.exe");
            //}
            //catch (System.Exception ex)
            //{
            //}

            tmNotify.Start();
            appTime = 0;
            LoadMatrix();

            try
            {
                Job = new PlanData()
                {
                    Job = context.GetAllPlanItems()
                };
            }
            catch
            {
            }
        }

        void LoadMatrix()
        {
            Matrix = new List<List<Button>>();

            Button oldBtn = new Button() { Width = 0, Height = 0, Location = new Point(-Cons.margin, 0) };
            for (int i = 0; i < Cons.DayOfColumn; i++)
            {
                Matrix.Add(new List<Button>());
                for (int j = 0; j < Cons.DayOfWeek; j++)
                {
                    Button btn = new Button() { Width = Cons.dateButtonWidth, Height = Cons.dateButtonHeight };
                    btn.Location = new Point(oldBtn.Location.X + oldBtn.Width + Cons.margin, oldBtn.Location.Y);
                    btn.Click += btn_Click;

                    pnlMatrix.Controls.Add(btn);
                    Matrix[i].Add(btn);

                    oldBtn = btn;
                }
                oldBtn = new Button() { Width = 0, Height = 0, Location = new Point(-Cons.margin, oldBtn.Location.Y + Cons.dateButtonHeight) };
            }

            SetDefaultDate();
        }

        void btn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty((sender as Button).Text))
                return;
            DailyPlan daily = new DailyPlan(new DateTime(dtpkDate.Value.Year, dtpkDate.Value.Month, Convert.ToInt32((sender as Button).Text)), Job, UserId, client);
            daily.ShowDialog();
        }

        int DayOfMonth(DateTime date)
        {
            switch (date.Month)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    return 31;
                case 2:
                    if ((date.Year % 4 == 0 && date.Year % 100 != 0) || date.Year % 400 == 0)
                        return 29;
                    else
                        return 28;
                default:
                    return 30; ;
            }
        }

        void AddNumberIntoMatrixByDate(DateTime date)
        {
            ClearMatrix();
            DateTime useDate = new DateTime(date.Year, date.Month, 1);

            int line = 0;

            for (int i = 1; i <= DayOfMonth(date); i++)
            {
                int column = dateOfWeek.IndexOf(useDate.DayOfWeek.ToString());
                Button btn = Matrix[line][column];
                btn.Text = i.ToString();

                if (isEqualDate(useDate, DateTime.Now))
                {
                    btn.BackColor = Color.Pink;
                }

                if (isEqualDate(useDate, date))
                {
                    btn.BackColor = Color.Aqua;
                }

                if (column >= 6)
                    line++;

                useDate = useDate.AddDays(1);
            }
        }

        bool isEqualDate(DateTime dateA, DateTime dateB)
        {
            return dateA.Year == dateB.Year && dateA.Month == dateB.Month && dateA.Day == dateB.Day;
        }

        void ClearMatrix()
        {
            for (int i = 0; i < Matrix.Count; i++)
            {
                for (int j = 0; j < Matrix[i].Count; j++)
                {
                    Button btn = Matrix[i][j];
                    btn.Text = "";
                    btn.BackColor = Color.WhiteSmoke;
                }
            }
        }

        void SetDefaultDate()
        {
            dtpkDate.Value = DateTime.Now;
        }

        private void dtpkDate_ValueChanged(object sender, EventArgs e)
        {
            AddNumberIntoMatrixByDate((sender as DateTimePicker).Value);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            dtpkDate.Value = dtpkDate.Value.AddMonths(1);
        }

        private void btnPreviours_Click(object sender, EventArgs e)
        {
            dtpkDate.Value = dtpkDate.Value.AddMonths(-1);
        }

        private void btnToDay_Click(object sender, EventArgs e)
        {
            SetDefaultDate();
        }

        private void tmNotify_Tick(object sender, EventArgs e)
        {
            if (!ckbNotify.Checked)
                return;

            AppTime++;

            if (AppTime < Cons.notifyTime)
                return;

            if (Job == null || Job.Job == null)
                return;

            DateTime currentDate = DateTime.Now;
            List<PlanItem> todayjobs = Job.Job.Where(p => p.Date.Year == currentDate.Year && p.Date.Month == currentDate.Month && p.Date.Day == currentDate.Day).ToList();
            Notify.ShowBalloonTip(Cons.notifyTimeOut, "Lịch công việc", string.Format("Bạn có {0} việc trong ngày hôm nay", todayjobs.Count), ToolTipIcon.Info);

            AppTime = 0;
        }

        private void nmNotify_ValueChanged(object sender, EventArgs e)
        {
            Cons.notifyTime = (int)nmNotify.Value;
        }

        private void ckbNotify_CheckedChanged(object sender, EventArgs e)
        {
            nmNotify.Enabled = ckbNotify.Checked;
        }

        private void Connect()
        {
            IP = new IPEndPoint(IPAddress.Parse(Cons.ip), 6740);
            //IP = new IPEndPoint(IPAddress.Parse("localhost"), 6740);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                client.Connect(IP);
            }
            catch
            {
                MessageBox.Show("Cannot connect to server" + MessageBoxButtons.OK + MessageBoxIcon.Error);
                return;
            }
            Thread listen = new Thread(Receive);
            listen.IsBackground = true;
            listen.Start();
        }
        private void Dissconnect()
        {
            client.Close();
        }
        private void Receive()
        {
            string userName = context2.GetNameById(UserId);
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024];
                    client.Receive(data);
                    string message = Deserialize(data) as string;
                    string[] str = message.Split(',');
                    if (str[0] == "Admin" && str[1] == userName)
                        AddMessage(message);
                }
            }
            catch
            {
                Dissconnect();
            }
        }
        private void AddMessage(string message)
        {
            DateTime currentDate = DateTime.Now;
            string[] str = message.Split(',');
            string result = string.Format("Admin:Đã thêm người dùng {0} công việc {1}", str[1], str[2]);
            Notify.ShowBalloonTip(Cons.notifyTimeOut, "Lịch công việc", result, ToolTipIcon.Info);
        }
        byte[] Serialize(string obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatte = new BinaryFormatter();
            formatte.Serialize(stream, obj);
            return stream.ToArray();
        }
        object Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatte = new BinaryFormatter();
            return formatte.Deserialize(stream);

        }

        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            Dissconnect();

        }
    }
}
