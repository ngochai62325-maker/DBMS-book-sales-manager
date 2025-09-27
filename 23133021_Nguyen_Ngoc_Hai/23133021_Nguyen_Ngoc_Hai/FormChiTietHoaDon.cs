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
            string query = "SELECT [Tên Sách], [Số Lượng], [Giá Bán], [Thành Tiền] " +
                          "FROM vw_ChiTietHoaDon WHERE [Mã Hóa Đơn] = " + maHoaDon;
            dgChiTietHoaDon.DataSource = dataProvider.execQuery(query);
        }

        private void dgChiTietHoaDon_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowID = e.RowIndex;
            if (rowID >= 0 && rowID < dgChiTietHoaDon.RowCount - 1)
            {
                DataGridViewRow row = dgChiTietHoaDon.Rows[rowID];

                // Kiểm tra null trước khi gán giá trị
                Sach.Text = row.Cells[0].Value?.ToString() ?? "";                // Tên Sách
                
                if (row.Cells[1].Value != null && row.Cells[1].Value != DBNull.Value)
                {
                    numSoLuongSach.Value = (int)row.Cells[1].Value;          // Số Lượng đã mua
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
                MessageBox.Show("Vui lòng chọn sách!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (numSoLuongSach.Value <= 0)
            {
                MessageBox.Show("Số lượng phải lớn hơn 0!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Lấy mã sách từ tên sách
                string getMaSachQuery = "SELECT MaSach FROM Sach WHERE TenSach = N'" + Sach.Text + "'";
                var maSachResult = dataProvider.execScaler(getMaSachQuery);
                
                if (maSachResult == null)
                {
                    MessageBox.Show("Không tìm thấy sách!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("Vui lòng chọn sách để sửa!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (numSoLuongSach.Value <= 0)
            {
                MessageBox.Show("Số lượng phải lớn hơn 0!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Lấy mã sách từ tên sách
                string getMaSachQuery = "SELECT MaSach FROM Sach WHERE TenSach = N'" + Sach.Text + "'";
                var maSachResult = dataProvider.execScaler(getMaSachQuery);
                
                if (maSachResult == null)
                {
                    MessageBox.Show("Không tìm thấy sách!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int maSach = (int)maSachResult;
                int soLuongMoi = (int)numSoLuongSach.Value;

                // Cập nhật chi tiết hóa đơn
                StringBuilder query = new StringBuilder("EXEC proc_CapNhatChiTietHoaDon");
                query.Append(" @maHoaDon = " + maHoaDon);
                query.Append(",@maSach = " + maSach);
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

            DialogResult check = MessageBox.Show("Bạn có chắc muốn xóa sách '" + Sach.Text + "' khỏi hóa đơn?",
                                         "Cảnh Báo",
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);

            if (check == DialogResult.Yes)
            {
                try
                {
                    // Lấy mã sách từ tên sách
                    string getMaSachQuery = "SELECT MaSach FROM Sach WHERE TenSach = N'" + Sach.Text + "'";
                    var maSachResult = dataProvider.execScaler(getMaSachQuery);
                    
                    if (maSachResult == null)
                    {
                        MessageBox.Show("Không tìm thấy sách!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    int maSach = (int)maSachResult;

                    // Xóa chi tiết hóa đơn
                    StringBuilder query = new StringBuilder("EXEC proc_XoaChiTietHoaDon");
                    query.Append(" @maHoaDon = " + maHoaDon);
                    query.Append(",@maSach = " + maSach);

                    dataProvider.execNonQuery(query.ToString());

                    loadChiTietHoaDon();
                    capNhatTongTien(); // Cập nhật tổng tiền
                    MessageBox.Show("Xóa chi tiết hóa đơn thành công!", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    
                    // Clear form
                    Sach.Text = "";
                    numSoLuongSach.Value = 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Xóa chi tiết hóa đơn không thành công! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
    

