using Calender.BO;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Calender
{
    public partial class AJobAdmin : UserControl
    {
        private PlanItem_BO context;
        private User_BO _user_BO;
        private PlanItem_BO _planItemBo;
        private Socket Client { get; set; }
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
        public AJobAdmin(PlanItem job, Socket client)
        {
            _user_BO = new User_BO();
            _planItemBo = new PlanItem_BO();
            InitializeComponent();
            Client = client;
            cbStatus.DataSource = PlanItem.ListStatus;
            Load_User();

            this.Job = job;

            ShowInfo();

            context = new PlanItem_BO();
        }

        private void Load_User()
        {
            List<User> userList = new List<User>();
            userList = _user_BO.GetAllUserExceptAdmin();
            foreach (var i in userList)
            {
                cb_user.Items.Add(i.UserName);
            }
        }

        void ShowInfo()
        {
            txbJob.Text = Job.Job;
            nmMinute.Value = Job.EstimateMinute;
            nmHours.Value = Job.EstimateHour;
            cbStatus.SelectedIndex = PlanItem.ListStatus.IndexOf(Job.Status);
            ckbDone.Checked = PlanItem.ListStatus.IndexOf(Job.Status) == (int)EPlanItem.DONE ? true : false;
            var userList = _user_BO.GetAllUserExceptAdmin();
            for (int i = 0; i < userList.Count; i++)
            {
                if (userList[i].UserId == Job.UserID)
                {
                    cb_user.SelectedIndex = i;
                    break;
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (deleted != null)
                deleted(this, new EventArgs());
        }

        public void EditPlanItem(PlanItem planItem)
        {
            try
            {
                _planItemBo.EditPlanItem(planItem);
                Job = planItem;
                ShowInfo();

            }
            catch (Exception) { }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            AddJob f3 = new AddJob(Client, Job.Date, Job);
            f3.D = new AddJob.DD(EditPlanItem);
            f3.Show();
        }

        private void ckbDone_CheckedChanged(object sender, EventArgs e)
        {
            //cbStatus.SelectedIndex = ckbDone.Checked ? (int)EPlanItem.DONE : (int)EPlanItem.DOING;
        }

        private void nmToMinute_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
