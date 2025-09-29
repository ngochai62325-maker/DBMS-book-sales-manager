using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _23133021_Nguyen_Ngoc_Hai
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Hiển thị form đăng nhập trước
            FormLogin loginForm = new FormLogin();
            
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                // Nếu đăng nhập thành công, chạy form chính với connection string đã xác thực
                Application.Run(new FormMain(loginForm.ConnectedConnectionString, loginForm.LoggedInUser, loginForm.IsAdmin));
            }
            // Nếu hủy đăng nhập thì thoát chương trình
        }
    }
}
