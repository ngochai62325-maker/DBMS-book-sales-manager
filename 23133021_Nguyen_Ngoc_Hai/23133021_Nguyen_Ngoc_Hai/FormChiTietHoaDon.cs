using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _23133021_Nguyen_Ngoc_Hai
{
    public partial class FormChiTietHoaDon : Form
    {
        private int maHoaDon;
        private int selectedMaSach; // Biến lưu mã sách được chọn ban đầu
        private DataProvider dataProvider = new DataProvider();
        

        public FormChiTietHoaDon(int maHoaDon)
        {
            InitializeComponent();
            this.maHoaDon = maHoaDon;
            init();
        }

        private void init()
        {
            title.Text = "Chi Tiết Hóa Đơn " + maHoaDon;
            loadChiTietHoaDon();
            capNhatTongTien(); // Tính tổng tiền ngay khi khởi tạo
        }

        // Tính và hiển thị tổng tiền sử dụng function fn_TinhTongTienHoaDon
        private void capNhatTongTien()
        {
            try
            {
                string query = "SELECT dbo.fn_TinhTongTienHoaDon(" + maHoaDon + ") AS TongTien";
                var result = dataProvider.execScaler(query);
                
                decimal giaTriTongTien = 0;
                if (result != null && result != DBNull.Value)
                {
                    giaTriTongTien = Convert.ToDecimal(result);
                }

                tongTien.Text = "Tổng tiền: " + giaTriTongTien.ToString("N0") + " VNĐ";
            }
            catch (Exception ex)
            {
                tongTien.Text = "Tổng tiền: 0 VNĐ";
                // Có thể log error nếu cần
            }
        }

        // Load chi tiết hóa đơn (chỉ hiển thị sách đã mua của hóa đơn cụ thể)
        private void loadChiTietHoaDon()
        {
            string query = "SELECT [Mã Sách], [Tên Sách], [Tác Giả], [Số Lượng], [Giá Bán], [Thành Tiền] " +
                          "FROM vw_ChiTietHoaDon WHERE [Mã Hóa Đơn] = " + maHoaDon;
            DataTable dt = dataProvider.execQuery(query);
            dgChiTietHoaDon.DataSource = dt;
            
            // Ẩn cột Mã Sách vì người dùng không cần thấy
            if (dgChiTietHoaDon.Columns.Count > 0)
            {
                dgChiTietHoaDon.Columns[0].Visible = false;
            }
            
            // Khởi tạo selectedMaSach với dòng đầu tiên nếu có dữ liệu
            if (dt.Rows.Count > 0)
            {
                selectedMaSach = Convert.ToInt32(dt.Rows[0][0]);
            }
        }

        private void dgChiTietHoaDon_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowID = e.RowIndex;
            if (rowID >= 0 && rowID < dgChiTietHoaDon.RowCount - 1)
            {
                DataGridViewRow row = dgChiTietHoaDon.Rows[rowID];

                // Kiểm tra null trước khi gán giá trị
                // Cấu trúc cột: [Mã Sách], [Tên Sách], [Tác Giả], [Số Lượng], [Giá Bán], [Thành Tiền]
                
                // Lưu Mã Sách được chọn
                if (row.Cells[0].Value != null && row.Cells[0].Value != DBNull.Value)
                {
                    selectedMaSach = Convert.ToInt32(row.Cells[0].Value);
                }
                
                Sach.Text = row.Cells[1].Value?.ToString() ?? "";                // Tên Sách
                tacGia.Text = row.Cells[2].Value?.ToString() ?? "";              // Tác Giả
                
                if (row.Cells[3].Value != null && row.Cells[3].Value != DBNull.Value)
                {
                    numSoLuongSach.Value = (int)row.Cells[3].Value;          // Số Lượng đã mua (index 3)
                }
                else
                {
                    numSoLuongSach.Value = 1;
                }
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(Sach.Text))
            {
                MessageBox.Show("Vui lòng nhập tên sách!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Sach.Focus();
                return;
            }

            if (string.IsNullOrEmpty(tacGia.Text))
            {
                MessageBox.Show("Vui lòng nhập tác giả!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tacGia.Focus();
                return;
            }

            if (numSoLuongSach.Value <= 0)
            {
                MessageBox.Show("Số lượng phải lớn hơn 0!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numSoLuongSach.Focus();
                return;
            }

            try
            {
                // Lấy mã sách từ tên sách và tác giả để đảm bảo chính xác
                string getMaSachQuery = "SELECT MaSach FROM Sach WHERE TenSach = N'" + Sach.Text.Trim().Replace("'", "''") + "' AND TacGia = N'" + tacGia.Text.Trim().Replace("'", "''") + "'";
                var maSachResult = dataProvider.execScaler(getMaSachQuery);
                
                if (maSachResult == null)
                {
                    MessageBox.Show("Không tìm thấy sách với tên và tác giả này!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Sach.Focus();
                    return;
                }

                int maSach = (int)maSachResult;
                int soLuong = (int)numSoLuongSach.Value;

                // Thêm chi tiết hóa đơn
                StringBuilder query = new StringBuilder("EXEC proc_ThemChiTietHoaDon");
                query.Append(" @maHoaDon = " + maHoaDon);
                query.Append(",@maSach = " + maSach);
                query.Append(",@soLuong = " + soLuong);

                var result = dataProvider.execQuery(query.ToString());
                
                loadChiTietHoaDon(); // Refresh danh sách
                capNhatTongTien(); // Cập nhật tổng tiền

                if (result.Rows.Count > 0)
                {
                    string message = result.Rows[0]["Message"].ToString();
                    MessageBox.Show(message, "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Thêm chi tiết hóa đơn thành công!", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }

                // Clear form
                Sach.Text = "";
                tacGia.Text = "";
                numSoLuongSach.Value = 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Thêm chi tiết hóa đơn không thành công! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(Sach.Text))
            {
                MessageBox.Show("Vui lòng nhập tên sách!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Sach.Focus();
                return;
            }

            if (string.IsNullOrEmpty(tacGia.Text))
            {
                MessageBox.Show("Vui lòng nhập tác giả!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tacGia.Focus();
                return;
            }

            if (numSoLuongSach.Value <= 0)
            {
                MessageBox.Show("Số lượng phải lớn hơn 0!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numSoLuongSach.Focus();
                return;
            }

            if (selectedMaSach <= 0)
            {
                MessageBox.Show("Vui lòng chọn một dòng để sửa!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int soLuongMoi = (int)numSoLuongSach.Value;

                // Cập nhật chi tiết hóa đơn sử dụng selectedMaSach
                // Không cần thay đổi sách, chỉ cập nhật số lượng
                StringBuilder query = new StringBuilder("EXEC proc_CapNhatChiTietHoaDon");
                query.Append(" @maHoaDon = " + maHoaDon);
                query.Append(",@maSach = " + selectedMaSach);
                query.Append(",@soLuongMoi = " + soLuongMoi);

                int result = dataProvider.execNonQuery(query.ToString());

                if (result > 0)
                {
                    loadChiTietHoaDon();
                    capNhatTongTien(); // Cập nhật tổng tiền
                    MessageBox.Show("Cập nhật chi tiết hóa đơn thành công!", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                else
                {
                    MessageBox.Show("Cập nhật chi tiết hóa đơn không thành công!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cập nhật chi tiết hóa đơn không thành công! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            // Kiểm tra có chọn sách để xóa không
            if (string.IsNullOrEmpty(Sach.Text))
            {
                MessageBox.Show("Vui lòng chọn sách để xóa!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedMaSach <= 0)
            {
                MessageBox.Show("Vui lòng chọn một dòng để xóa!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult check = MessageBox.Show("Bạn có chắc muốn xóa sách '" + Sach.Text + "' khỏi hóa đơn?",
                                         "Cảnh Báo",
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);

            if (check == DialogResult.Yes)
            {
                try
                {
                    // Xóa chi tiết hóa đơn sử dụng selectedMaSach
                    StringBuilder query = new StringBuilder("EXEC proc_XoaChiTietHoaDon");
                    query.Append(" @maHoaDon = " + maHoaDon);
                    query.Append(",@maSach = " + selectedMaSach);

                    dataProvider.execNonQuery(query.ToString());

                    loadChiTietHoaDon();
                    capNhatTongTien(); // Cập nhật tổng tiền
                    MessageBox.Show("Xóa chi tiết hóa đơn thành công!", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    
                    // Clear form
                    Sach.Text = "";
                    tacGia.Text = "";
                    numSoLuongSach.Value = 1;
                    selectedMaSach = 0; // Reset selectedMaSach
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Xóa chi tiết hóa đơn không thành công! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
    

