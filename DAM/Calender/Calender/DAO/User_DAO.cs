using System.Collections.Generic;
using System.Linq;

namespace Calender.ConnectDB
{
    public class User_DAO
    {
        private DBContext context;
        public User_DAO()
        {
            context = new DBContext();
        }

        public User CheckLogin(string userName, string password)
        {
            User user = context?.Users?.FirstOrDefault(o => o.UserName == userName && o.Password == password);
            if (user != null)
            {
                return user;
            }
            return null;
        }

        public List<User> GetAllUserExceptAdmin()
        {
            return context.Users.Where(o => o.Role != "Admin").ToList();
        }

        public string GetNameById(int userId)
        {
            return context.Users.FirstOrDefault(o => o.UserId == userId)?.UserName;
        }

        public User GetUserFromUserName(string userName)
        {
            User user = context.Users.FirstOrDefault(o => o.UserName == userName);
            if (user != null)
            {
                return user;
            }
            return null;
        }
    }
}
