using Calender.BO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;

namespace Calender
{
    public partial class QuanLyLichNhanVien : Form
    {
        #region Properties   
        private PlanItem_BO context;
        private User_BO context2;
        public int UserId { get; set; }
        IPEndPoint IP;
        Socket client;
        public int AppTime { get; set; }
        private string filePath = "data.xml";

        public List<List<Button>> Matrix { get; set; }

        public PlanData Job { get; set; }

        #endregion
        public QuanLyLichNhanVien()
        {
            InitializeComponent();
            context = new PlanItem_BO();
            context2 = new User_BO();
            RegistryKey regkey = Registry.CurrentUser.CreateSubKey("Software\\LapLich");
            //mo registry khoi dong cung win
            RegistryKey regstart = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            string keyvalue = "1";
            //string subkey = "Software\\ManhQuyen";
            try
            {
                //chen gia tri key
                regkey.SetValue("Index", keyvalue);
                //regstart.SetValue("taoregistrytronghethong", "E:\\Studing\\Bai Tap\\CSharp\\Channel 4\\bai temp\\tao registry trong he thong\\tao registry trong he thong\\bin\\Debug\\tao registry trong he thong.exe");
                regstart.SetValue("LapLich", Application.StartupPath + "\\Lập lịch.exe");
                ////dong tien trinh ghi key
                //regkey.Close();
            }
            catch (System.Exception ex)
            {
            }

            tmNotify.Start();
            AppTime = 0;
            LoadMatrix();

            try
            {
                //Job = DeserializeFromXML(filePath) as PlanData;
                Job = new PlanData()
                {
                    Job = context.GetAllPlanItems()
                };
            }
            catch
            {
                SetDefaultJob();
            }
        }


        public QuanLyLichNhanVien(int userId)
        {
            InitializeComponent();
            Connect();
            context = new PlanItem_BO();
            UserId = userId;
            RegistryKey regkey = Registry.CurrentUser.CreateSubKey("Software\\LapLich");
            //mo registry khoi dong cung win
            RegistryKey regstart = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            string keyvalue = "1";
            //string subkey = "Software\\ManhQuyen";
            try
            {
                //chen gia tri key
                regkey.SetValue("Index", keyvalue);
                //regstart.SetValue("taoregistrytronghethong", "E:\\Studing\\Bai Tap\\CSharp\\Channel 4\\bai temp\\tao registry trong he thong\\tao registry trong he thong\\bin\\Debug\\tao registry trong he thong.exe");
                regstart.SetValue("LapLich", Application.StartupPath + "\\Lập lịch.exe");
                ////dong tien trinh ghi key
                //regkey.Close();
            }
            catch (System.Exception ex)
            {
            }

            tmNotify.Start();
            AppTime = 0;
            LoadMatrix();

            try
            {
                //Job = DeserializeFromXML(filePath) as PlanData;
                Job = new PlanData()
                {
                    Job = context.GetAllPlanItems()
                };
            }
            catch
            {
                SetDefaultJob();
            }
        }

        void SetDefaultJob()
        {
            Job = new PlanData();
            Job.Job = new List<PlanItem>();
            Job.Job.Add(new PlanItem()
            {
                Date = DateTime.Now,
                Job = "Thử nghiệm thôi",
                Status = PlanItem.ListStatus[(int)EPlanItem.COMING]
            });
        }

        //load ma trận button hiển thị
        void LoadMatrix()
        {
            Matrix = new List<List<Button>>(); // khởi tạo ma trân btn

            Button oldBtn = new Button() { Width = 0, Height = 0, Location = new Point(-Cons.margin, 0) };
            for (int i = 0; i < Cons.DayOfColumn; i++)
            {
                Matrix.Add(new List<Button>());
                for (int j = 0; j < Cons.DayOfWeek; j++)
                {
                    Button btn = new Button() { Width = Cons.dateButtonWidth, Height = Cons.dateButtonHeight };
                    btn.Location = new Point(oldBtn.Location.X + oldBtn.Width + Cons.margin, oldBtn.Location.Y); //location của button ms = location btn cũ + width + kc giữa các button
                    btn.Click += btn_Click;

                    pnlMatrix.Controls.Add(btn);  // add button vào panel ma trận ngày
                    Matrix[i].Add(btn);  // add vào ma trận button

                    oldBtn = btn;  // gán btn cho oldbtn
                }
                oldBtn = new Button() { Width = 0, Height = 0, Location = new Point(-Cons.margin, oldBtn.Location.Y + Cons.dateButtonHeight) };
            }

            SetDefaultDate();
        }

        void btn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty((sender as Button).Text))
                return;
            Plan plan = new Plan(new DateTime(dtpkDate.Value.Year, dtpkDate.Value.Month, Convert.ToInt32((sender as Button).Text)), Job, UserId, client);
            plan.ShowDialog();
        }


        //tính số ngày trong tháng hiện tại
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

        private List<string> dateOfWeek = new List<string>() { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };// tạo mãng : 2->cn 0->6

        //in số ngày lên ma trận ngày
        void AddNumberIntoMatrixByDate(DateTime date)
        {
            ClearMatrix();
            DateTime useDate = new DateTime(date.Year, date.Month, 1);

            int line = 0;   // bắt đầu từ hàng 0

            for (int i = 1; i <= DayOfMonth(date); i++)
            {
                int column = dateOfWeek.IndexOf(useDate.DayOfWeek.ToString()); // cột: lấy ra thứ của ngày hiện tại => ứng vs cột tương ứng 2->cn 0->6
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

                useDate = useDate.AddDays(1);  // tăng ngày lên 1
            }
        }

        //so sánh ngày
        bool isEqualDate(DateTime dateA, DateTime dateB)
        {
            return dateA.Year == dateB.Year && dateA.Month == dateB.Month && dateA.Day == dateB.Day;
        }

        //xóa giá trị ngày cũ của ma trân button
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


        //set mặc định date time picker = ngày hiện tại
        void SetDefaultDate()
        {
            dtpkDate.Value = DateTime.Now;
        }


        // sự kiện nếu mà date time picker thay đổi => call hàm AddNumberIntoMatrixByDate để in lại ngày trên ma trận
        private void dtpkDate_ValueChanged(object sender, EventArgs e)
        {
            AddNumberIntoMatrixByDate((sender as DateTimePicker).Value);
        }

        //bắt sự kiện khi nhấn btn tháng sau
        private void btnNext_Click(object sender, EventArgs e)
        {
            dtpkDate.Value = dtpkDate.Value.AddMonths(1);//tăng tháng của dtpc lên 1
        }

        //bắt sự kiện khi nhấn btn tháng trước
        private void btnPreviours_Click(object sender, EventArgs e)
        {
            dtpkDate.Value = dtpkDate.Value.AddMonths(-1); // giảm tháng của dtpc đi 1
        }

        //bắt sự kiện khi nhấn btn hôm nay
        private void btnToDay_Click(object sender, EventArgs e)
        {
            SetDefaultDate();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void Connect()
        {
            IP = new IPEndPoint(IPAddress.Parse(Cons.ip), 6740);
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
        private void Send()
        {
            // (tbMessage.Text != string.Empty)
            //    client.Send(Serialize(tbMessage.Text));

        }
        private void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024];
                    client.Receive(data);
                    string message = Deserialize(data) as string;
                    string[] str = message.Split(',');
                    if (str[0] == "User")
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
            //lvMessage.Items.Add(new ListViewItem() { Text = message });
            //tbMessage.Clear();
            DateTime currentDate = DateTime.Now;
            string[] str = message.Split(',');
            string result = string.Format("User:{0} đã chuyển trạng thái công việc {1} thành {2}", str[1], str[2], str[3]);
            Notify.ShowBalloonTip(Cons.notifyTimeOut, "Lịch công việc", result, ToolTipIcon.Info);
        }

        // lưu dl
        byte[] Serialize(string obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatte = new BinaryFormatter();
            formatte.Serialize(stream, obj);
            return stream.ToArray();
        }

        // lấy dl lên chuyển thành kiểu chuổ
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
