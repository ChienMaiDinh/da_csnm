using Calender.BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Calender
{
    public partial class DailyPlan : Form
    {
        private Socket Client { get; set; }

        private PlanItem_BO context;

        public int UserId { get; set; }

        public DateTime Date { get; set; }

        public PlanData Job { get; set; }

        FlowLayoutPanel fPanel = new FlowLayoutPanel();

        public DailyPlan(DateTime date, PlanData job)
        {
            context = new PlanItem_BO();
            InitializeComponent();

            Date = date;
            Job = job;

            fPanel.Width = pnlJob.Width;
            fPanel.Height = pnlJob.Height;
            pnlJob.Controls.Add(fPanel);

            dtpkDate.Value = Date;
        }

        public DailyPlan(DateTime date, PlanData job, int userId, Socket client)
        {
            context = new PlanItem_BO();
            InitializeComponent();
            UserId = userId;
            Date = date;
            Job = job;
            Client = client;
            fPanel.Width = pnlJob.Width;
            fPanel.Height = pnlJob.Height;
            pnlJob.Controls.Add(fPanel);

            dtpkDate.Value = Date;
        }

        void ShowJobByDate(DateTime date)
        {
            fPanel.Controls.Clear();  // clear dữ liệu trong panel cũ
            Job.Job = context.GetAllPlanItems();
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
            AJob aJob = new AJob(job, UserId, Client);
            aJob.Edited += aJob_Edited;
            aJob.Deleted += aJob_Deleteed;

            fPanel.Controls.Add(aJob);
        }

        void aJob_Deleteed(object sender, EventArgs e)
        {
            AJob uc = sender as AJob;
            PlanItem job = uc.Job;

            fPanel.Controls.Remove(uc);
            Job.Job.Remove(job);
        }

        void aJob_Edited(object sender, EventArgs e)
        {
        }

        List<PlanItem> GetJobByDay(DateTime date)
        {
            return Job.Job.Where(p => p.Date.Year == date.Year && p.Date.Month == date.Month && p.Date.Day == date.Day && UserId == p.UserID).ToList();
        }

        private void dtpkDate_ValueChanged(object sender, EventArgs e)
        {
            ShowJobByDate((sender as DateTimePicker).Value);
        }


        // bắt sự kiện cho button hôm sau
        private void btnNextDay_Click(object sender, EventArgs e)
        {
            dtpkDate.Value = dtpkDate.Value.AddDays(1);
        }

        // bắt sự kiện cho button hôm qua
        private void btnPrevioursDay_Click(object sender, EventArgs e)
        {
            dtpkDate.Value = dtpkDate.Value.AddDays(-1);
        }

        private void mnsiAddJob_Click(object sender, EventArgs e)
        {
            PlanItem item = new PlanItem() { Date = dtpkDate.Value };
            Job.Job.Add(item);
            AddJob(item);
        }

        private void mnsiToDay_Click(object sender, EventArgs e)
        {
            dtpkDate.Value = DateTime.Now;
        }
    }
}