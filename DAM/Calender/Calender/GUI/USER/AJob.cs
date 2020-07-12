using Calender.BO;
using Calender.GUI.USER;
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace Calender
{
    public partial class AJob : UserControl
    {
        private PlanItem_BO context;
        private User_BO context2;
        private Socket Client { get; set; }
        private int UserId { get; set; }
        public PlanItem Job { get; set; }

        private event EventHandler edited;
        public event EventHandler Edited
        {
            add { edited += value; }
            remove { edited -= value; }
        }

        private event EventHandler deleted;
        public event EventHandler Deleted
        {
            add { deleted += value; }
            remove { deleted -= value; }
        }
        public AJob(PlanItem job, int userId, Socket client)
        {
            InitializeComponent();
            Client = client;
            UserId = userId;
            cbStatus.DataSource = PlanItem.ListStatus;

            this.Job = job;

            ShowInfo();
            context2 = new User_BO();
            context = new PlanItem_BO();
        }

        // mapping dữ liệu cho mỗi plan item
        void ShowInfo()
        {
            txbJob.Text = Job.Job;
              nmHours.Value = Job.EstimateHour;
              nmMinute.Value = Job.EstimateMinute;
            cbStatus.SelectedIndex = PlanItem.ListStatus.IndexOf(Job.Status);
            ckbDone.Checked = PlanItem.ListStatus.IndexOf(Job.Status) == (int)EPlanItem.DONE ? true : false;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (deleted != null)
                deleted(this, new EventArgs());
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            Job.Job = txbJob.Text;
            // Job.FromTime = new Point((int)nmFromHours.Value, (int)nmFromMinute.Value);
            // Job.ToTime = new Point((int)nmToHours.Value, (int)nmToMinute.Value);
            Job.Status = PlanItem.ListStatus[cbStatus.SelectedIndex];

            string username = context2.GetNameById(UserId);
            Client.Send(Serialize(string.Format("User,{0},{1},{2}", username, Job.Job, Job.Status)));
            context.EditPlanItem(Job);
        }

        private void ckbDone_CheckedChanged(object sender, EventArgs e)
        {
            cbStatus.SelectedIndex = ckbDone.Checked ? (int)EPlanItem.DONE : (int)EPlanItem.DOING;
        }

        byte[] Serialize(string obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatte = new BinaryFormatter();
            formatte.Serialize(stream, obj);
            return stream.ToArray();
        }

        private void btn_detail_Click(object sender, EventArgs e)
        {
            JobDetail jobDetailForm = new JobDetail(Job);
            jobDetailForm.ShowDialog();
        }
    }
}
