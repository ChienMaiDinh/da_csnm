using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Calender
{
    public class PlanItem
    {
        [Key] public int PlanItemId { get; set; }

        public int UserID { get; set; }

        public DateTime Date { get; set; }

        public string Job { get; set; }

        public string JobContent { get; set; }

        public int EstimateHour { get; set; }

        public int EstimateMinute { get; set; }

        public string Link { get; set; }
        public string Status { get; set; }

        public static List<string> ListStatus = new List<string>() { "COMING", "DONE", "DOING", "MISSED" };
    }

    public enum EPlanItem
    {
        DONE,
        DOING,
        COMING,
        MISSED
    }
}