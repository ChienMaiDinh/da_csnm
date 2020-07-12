using System;
using System.Collections.Generic;

namespace Calender.BO
{
    public class PlanItem_BO
    {
        private PlanItem_DAO _planItemBo;

        public PlanItem_BO()
        {
            _planItemBo = new PlanItem_DAO();
        }

        public List<PlanItem> GetAllPlanItems()
        {
            return _planItemBo.GetAllPlanItems();
        }

        public void EditPlanItem(PlanItem item)
        {
            _planItemBo.EditPlanItem(item);
        }

        public void AddPlanItem(PlanItem item)
        {
            _planItemBo.AddPlanItem(item);
        }

        public bool CheckPlanItem(PlanItem item)
        {
            if (item == null || item.Job == String.Empty || item.UserID == 0)
            {
                return false;
            }

            return true;
        }
    }
}
