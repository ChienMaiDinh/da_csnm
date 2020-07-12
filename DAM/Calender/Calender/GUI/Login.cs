using Calender.BO;
using System;
using System.Windows.Forms;

namespace Calender
{
    public partial class Login : Form
    {
        private User_BO context;
        public Login()
        {
            context = new User_BO();
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string userName = txtUserName.Text;
            string password = txtPassword.Text;
            User user = context.CheckLogin(userName, password);
            if (user != null)
            {
                if (user.Role == "User")
                {
                    this.Visible = false;
                    Form1 form = new Form1(user.UserId);
                    form.ShowDialog();
                }
                else if (user.Role == "Admin")
                {
                    this.Visible = false;
                    QuanLyLichNhanVien form = new QuanLyLichNhanVien(user.UserId);
                    form.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng.");
            }
        }
    }
}
