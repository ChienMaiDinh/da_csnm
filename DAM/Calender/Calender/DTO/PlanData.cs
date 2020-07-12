using System;
using System.Collections.Generic;

namespace Calender
{
    [Serializable]
    public class PlanData   // chứa danh sách các plan item
    {
        public List<PlanItem> Job { get; set; }
    }
}