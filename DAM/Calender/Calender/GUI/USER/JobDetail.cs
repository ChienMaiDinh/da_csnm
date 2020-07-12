using Calender.BO;
using System.Windows.Forms;

namespace Calender.GUI.USER
{
    public partial class JobDetail : Form
    {
        private PlanItem _planItem;
        public User_BO _userBo;
        public JobDetail(PlanItem planItem)
        {
            InitializeComponent();
            _userBo = new User_BO();
            _planItem = planItem;
            Show();
        }

        public void Show()
        {
            tb_job_name.Text = _planItem.Job;
            tb_job_content.Text = _planItem.JobContent;
            link_lb.Text = _planItem.Link;
            nmToHours.Value = _planItem.EstimateHour;
            nmtominute.Value = _planItem.EstimateMinute;
            combo_user.Items.Add(_userBo.GetNameById(_planItem.UserID));
            combo_user.SelectedIndex = 0;
            cb_status.Items.Add(_planItem.Status);
            cb_status.SelectedIndex = 0;
        }

    }
}
