using System.Collections.Generic;
using System.Linq;

namespace Calender
{
    public class PlanItem_DAO
    {
        DBContext context;
        public PlanItem_DAO()
        {
            context = new DBContext();
        }

        public List<PlanItem> GetAllPlanItems()
        {
            return context.PlanItems.ToList();
        }

        public void EditPlanItem(PlanItem item)
        {
            var tmp = context.PlanItems.FirstOrDefault(o => o.PlanItemId == item.PlanItemId);
            tmp.Status = item.Status;
            tmp.Job = item.Job;
            tmp.UserID = item.UserID;
            tmp.EstimateHour = item.EstimateHour;
            tmp.EstimateMinute = item.EstimateMinute;
            tmp.JobContent = item.JobContent;
            context.SaveChanges();
        }

        public void AddPlanItem(PlanItem item)
        {
            context.PlanItems.Add(item);
            context.SaveChanges();
        }
    }
}
