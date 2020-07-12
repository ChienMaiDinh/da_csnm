using Calender.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Calender
{
    public partial class Plan : Form
    {
        private Socket Client { get; set; }
        private PlanItem_BO context;
        public int UserId { get; set; }
        public DateTime Date { get; set; }


        public PlanData Job { get; set; }

        FlowLayoutPanel fPanel = new FlowLayoutPanel();

        public Plan(DateTime date, PlanData job)
        {
            context = new PlanItem_BO();
            InitializeComponent();

            this.Date = date;
            this.Job = job;

            fPanel.Width = pnlJob.Width;
            fPanel.Height = pnlJob.Height;
            pnlJob.Controls.Add(fPanel);

            dtpkDate.Value = Date;

        }

        public Plan(DateTime date, PlanData job, int userId, Socket client)
        {
            context = new PlanItem_BO();
            InitializeComponent();
            Client = client;
            this.UserId = userId;
            this.Date = date;
            this.Job = job;

            fPanel.Width = pnlJob.Width;
            fPanel.Height = pnlJob.Height;
            pnlJob.Controls.Add(fPanel);

            dtpkDate.Value = Date;

        }

        //hiển thị công việc trong ngày được chọn
        void ShowJobByDate(DateTime date)
        {
            Job.Job = context.GetAllPlanItems();
            fPanel.Controls.Clear();                // xoa danh sách cũ
            if (Job != null && Job.Job != null)
            {
                List<PlanItem> todayJob = GetJobByDay(date);
                for (int i = 0; i < todayJob.Count; i++)
                {
                    AddJob(todayJob[i]);
                }
            }
        }

        void AddJob(PlanItem job)
        {
            AJobAdmin AJobAdmin = new AJobAdmin(job, Client);
            AJobAdmin.Edited += AJobAdmin_Edited;
            AJobAdmin.Deleted += AJobAdmin_Deleteed;

            fPanel.Controls.Add(AJobAdmin);
        }

        void AJobAdmin_Deleteed(object sender, EventArgs e)
        {
            AJobAdmin uc = sender as AJobAdmin;
            PlanItem job = uc.Job;

            fPanel.Controls.Remove(uc);
            Job.Job.Remove(job);
        }

        void AJobAdmin_Edited(object sender, EventArgs e)
        {
        }

        List<PlanItem> GetJobByDay(DateTime date)
        {
            return Job.Job.Where(p => p.Date.Year == date.Year && p.Date.Month == date.Month && p.Date.Day == date.Day).ToList();
        }

        private void dtpkDate_ValueChanged(object sender, EventArgs e)
        {
            ShowJobByDate((sender as DateTimePicker).Value);
        }

        private void btnNextDay_Click(object sender, EventArgs e)
        {
            dtpkDate.Value = dtpkDate.Value.AddDays(1);
        }

        private void btnPrevioursDay_Click(object sender, EventArgs e)
        {
            dtpkDate.Value = dtpkDate.Value.AddDays(-1);
        }

        private void mnsiAddJob_Click(object sender, EventArgs e)
        {
            AddJob guiAddHob = new AddJob(Client, dtpkDate.Value);
            guiAddHob.ShowDialog();
            if (guiAddHob.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                ShowJobByDate(dtpkDate.Value);
            }
        }

        private void mnsiToDay_Click(object sender, EventArgs e)
        {
            dtpkDate.Value = DateTime.Now;
        }
    }
}
