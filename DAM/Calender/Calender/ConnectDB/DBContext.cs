using System.Data.Entity;

namespace Calender
{
    public class DBContext : DbContext
    {
        public DBContext()
            : base("DefaultConnection")
        {

        }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<PlanItem> PlanItems { get; set; }
    }
}
