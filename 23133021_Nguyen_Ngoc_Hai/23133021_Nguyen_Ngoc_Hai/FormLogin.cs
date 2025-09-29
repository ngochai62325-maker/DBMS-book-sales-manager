using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace _23133021_Nguyen_Ngoc_Hai
{
    public partial class FormLogin : Form
    {
        public string ConnectedConnectionString { get; private set; }
        public string LoggedInUser { get; private set; }
        public bool IsAdmin { get; private set; }

        public FormLogin()
        {
            InitializeComponent();
            // Chỉ đặt username mặc định, không đặt password để bảo mật
            txtUsername.Text = "";
            txtPassword.Text = "";
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string server = txtServer.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(server))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Thử kết nối với SQL Authentication
            string connectionString = $"Data Source={server};Initial Catalog=QLBanSach;User ID={username};Password={password};Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // Kiểm tra user thuộc role nào
                    using (SqlCommand cmd = new SqlCommand("SELECT IS_ROLEMEMBER('app_admin'), IS_ROLEMEMBER('app_employee')", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool isAdmin = reader.GetInt32(0) == 1;
                                bool isEmployee = reader.GetInt32(1) == 1;
                                
                                if (isAdmin || isEmployee)
                                {
                                    ConnectedConnectionString = connectionString;
                                    LoggedInUser = username;
                                    IsAdmin = isAdmin;
                                    
                                    string roleText = isAdmin ? "Quản trị viên" : "Nhân viên";
                                    MessageBox.Show($"Đăng nhập thành công!\nChào mừng {username} ({roleText})", 
                                                  "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    
                                    this.DialogResult = DialogResult.OK;
                                    this.Close();
                                    return;
                                }
                                else
                                {
                                    MessageBox.Show("Tài khoản không có quyền truy cập hệ thống!", 
                                                  "Lỗi quyền", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                string errorMessage = "Đăng nhập thất bại!\n";
                
                if (ex.Number == 18456) // Login failed
                {
                    errorMessage += "Sai tên đăng nhập hoặc mật khẩu.";
                }
                else if (ex.Number == 2) // Network error
                {
                    errorMessage += "Không thể kết nối tới server. Vui lòng kiểm tra tên server.";
                }
                else if (ex.Number == 4060) // Database not found
                {
                    errorMessage += "Không tìm thấy database QLBanSach.";
                }
                else
                {
                    errorMessage += ex.Message;
                }
                
                MessageBox.Show(errorMessage, "Lỗi đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi không xác định: " + ex.Message, "Lỗi", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FormLogin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }
    }
}