using Calender.BO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace Calender
{
    public partial class AddJob : Form
    {
        private User_BO _userBO;
        private PlanItem_BO _planItemBO;
        private Socket _client { get; set; }

        public DateTime _dateTime;
        public PlanItem _plan_Item;

        public delegate void DD(PlanItem v);
        public DD D;

        public AddJob(Socket client, DateTime dateTime)
        {
            _userBO = new User_BO();
            _planItemBO = new PlanItem_BO();
            _client = client;
            _dateTime = dateTime;
            InitializeComponent();
            Load_User();
            cb_status.DataSource = PlanItem.ListStatus;
            cb_status.SelectedIndex = 0;
            _plan_Item = null;
        }

        public AddJob(Socket client, DateTime dateTime, PlanItem planItem)
        {
            _userBO = new User_BO();
            _planItemBO = new PlanItem_BO();
            _client = client;
            _dateTime = dateTime;
            _plan_Item = planItem;
            InitializeComponent();
            cb_status.DataSource = PlanItem.ListStatus;
            Load_User();
            Show();
            cb_status.DataSource = PlanItem.ListStatus;
        }

        private void Show()
        {
            tb_job_name.Text = _plan_Item.Job;
            tb_job_content.Text = _plan_Item.JobContent;
            tb_link.Text = _plan_Item.Link;
            nmToHours.Value = _plan_Item.EstimateHour;
            nmtominute.Value = _plan_Item.EstimateMinute;
            for (int i = 0; i < PlanItem.ListStatus.Count; i++)
            {
                if (PlanItem.ListStatus[i] == _plan_Item.Status)
                {
                    cb_status.SelectedIndex = i;
                    break;
                }
            }

            for (int i = 0; i < combo_user.Items.Count; i++)
            {
                if (combo_user.Items[i].ToString() == _userBO.GetNameById(_plan_Item.UserID))
                {
                    combo_user.SelectedIndex = i;
                    break;
                }
            }

        }

        private void Load_User()
        {
            List<User> userList = new List<User>();
            userList = _userBO.GetAllUserExceptAdmin();
            foreach (var i in userList)
            {
                combo_user.Items.Add(i.UserName);
            }
        }

        private void btn_save_Click(object sender, System.EventArgs e)
        {
            try
            {
                if (_plan_Item == null)
                {
                    var user = _userBO.GetUserFromUserName(combo_user.SelectedItem.ToString());
                    PlanItem planItem = new PlanItem();
                    planItem.Date = _dateTime;
                    planItem.Job = tb_job_name.Text;
                    planItem.JobContent = tb_job_content.Text;
                    planItem.EstimateHour = (int)nmToHours.Value;
                    planItem.EstimateMinute = (int)nmtominute.Value;
                    planItem.Link = tb_link.Text;
                    planItem.Status = cb_status.SelectedItem.ToString();
                    planItem.UserID = user == null ? 0 : user.UserId;
                    if (!_planItemBO.CheckPlanItem(planItem))
                    {
                        MessageBox.Show("Hãy chọn đũ các trường bắt buộc.");
                        return;
                    }
                    _planItemBO.AddPlanItem(planItem);
                    _client.Send(Serialize(string.Format("Admin,{0},{1}", user.UserName, planItem.Job)));

                }
                else
                {
                    var user = _userBO.GetUserFromUserName(combo_user.SelectedItem.ToString());
                    _plan_Item.Date = _dateTime;
                    _plan_Item.Job = tb_job_name.Text;
                    _plan_Item.JobContent = tb_job_content.Text;
                    _plan_Item.EstimateHour = (int)nmToHours.Value;
                    _plan_Item.EstimateMinute = (int)nmtominute.Value;
                    _plan_Item.Link = tb_link.Text;
                    _plan_Item.Status = cb_status.SelectedItem.ToString();
                    _plan_Item.UserID = user == null ? 0 : user.UserId;
                    if (!_planItemBO.CheckPlanItem(_plan_Item))
                    {
                        MessageBox.Show("Hãy chọn đũ các trường bắt buộc.");
                        return;
                    }
                    this.D.Invoke(_plan_Item);
                }
                this.DialogResult = DialogResult.OK;
                this.Close();

            }
            catch (Exception)
            {
                MessageBox.Show("Hãy chọn đũ các trường bắt buộc.");
            }
        }


        byte[] Serialize(string obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatte = new BinaryFormatter();
            formatte.Serialize(stream, obj);
            return stream.ToArray();
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
