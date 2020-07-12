using Calender.ConnectDB;
using System.Collections.Generic;

namespace Calender.BO
{
    public class User_BO
    {
        private User_DAO dal;
        public User_BO()
        {
            dal = new User_DAO();
        }

        public List<User> GetAllUserExceptAdmin()
        {
            return dal.GetAllUserExceptAdmin();
        }

        public string GetNameById(int userId)
        {
            return dal.GetNameById(userId);
        }

        public User CheckLogin(string userName, string password)
        {
            return dal.CheckLogin(userName, password);
        }

        public User GetUserFromUserName(string userName)
        {
            return dal.GetUserFromUserName(userName);
        }
    }
}
