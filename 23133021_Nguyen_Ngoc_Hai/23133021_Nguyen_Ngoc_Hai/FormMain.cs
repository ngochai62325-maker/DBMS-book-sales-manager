using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _23133021_Nguyen_Ngoc_Hai
{
    public partial class FormMain : Form
    {
        private DataProvider dataProvider = new DataProvider();
        private int maSachLoaiSach;
        private int maSachSach;
        private int maLoaiSachLoaiSach;
        private int maHoaDonHoaDon;

        public FormMain()
        {
            InitializeComponent();
            init();
        }

        private void init()
        {
            initSach();
            initLoaiSach();
            initHoaDon();
        }


        //Xu ly sach
        private void initSach()
        {
            loadDgSach();
            loadcbSachLoaiSach();
            
            // Thêm tính năng tìm kiếm riêng cho từng TextBox
            txtSachTenSach.TextChanged += txtSachTenSach_TextChanged;
            txtSachTacGia.TextChanged += txtSachTacGia_TextChanged;
            cbSachLoaiSach.SelectedIndexChanged += cbSachLoaiSach_TimKiem_SelectedIndexChanged;
        }

        private void loadDgSach()
        {
            try
            {
                DataTable dt = dataProvider.execQuery("SELECT * FROM vw_DanhSachSach");
                dgSach.DataSource = dt;

                // Kiểm tra có dữ liệu trước khi gán maSachSach
                if (dt.Rows.Count > 0)
                {
                    maSachSach = (int)dt.Rows[0][0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải danh sách sách! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadcbSachLoaiSach()
        {
            DataTable dt = dataProvider.execQuery("SELECT * FROM vw_LoaiSach");
            cbSachLoaiSach.DisplayMember = "TenLoaiSach";
            cbSachLoaiSach.ValueMember = "MaLoaiSach";

            cbSachLoaiSach.DataSource = dt;
        }

        private void dgSach_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowID = e.RowIndex;
            if (rowID >= 0 && rowID < dgSach.RowCount - 1)
            {
                DataGridViewRow row = dgSach.Rows[rowID];

                maSachSach = (int)row.Cells[0].Value;
                txtSachTenSach.Text = row.Cells[1].Value.ToString();
                cbSachLoaiSach.Text = row.Cells[2].Value.ToString();
                txtSachTacGia.Text = row.Cells[3].Value.ToString();
                numSachSoLuong.Value = (int)row.Cells[4].Value;
                numSachGiaBan.Value = Convert.ToInt32(row.Cells[5].Value);

                // Lấy MaLoaiSach từ SelectedValue thay vì query
                if (cbSachLoaiSach.SelectedValue != null)
                {
                    maSachLoaiSach = (int)cbSachLoaiSach.SelectedValue;
                }
            } 
        }

        private void btnSachThem_Click(object sender, EventArgs e)
        {
            StringBuilder query = new StringBuilder("EXEC proc_ThemSach");
            query.Append(" @tenSach = N'" + txtSachTenSach.Text + "'");
            query.Append(",@maLoaiSach = " + maSachLoaiSach);
            query.Append(",@tacGia = N'" + txtSachTacGia.Text + "'");
            query.Append(",@soLuong = " + numSachSoLuong.Value);
            query.Append(",@giaBan = " + numSachGiaBan.Value);

            try
            {
                dataProvider.execNonQuery(query.ToString());
                loadDgSach();
                MessageBox.Show("Thêm sách thành công!", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Thêm sách không thành công! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSachSua_Click(object sender, EventArgs e)
        {
            StringBuilder query = new StringBuilder("EXEC proc_CapNhatSach");
            query.Append(" @maSach = " + maSachSach);
            query.Append(",@tenSach = N'" + txtSachTenSach.Text + "'");
            query.Append(",@maLoaiSach = " + maSachLoaiSach);
            query.Append(",@tacGia = N'" + txtSachTacGia.Text + "'");
            query.Append(",@soLuong = " + numSachSoLuong.Value);
            query.Append(",@giaBan = " + numSachGiaBan.Value);

            int result = dataProvider.execNonQuery(query.ToString());

            if (result > 0)
            {
                loadDgSach();
                MessageBox.Show("Cập nhật sách thành công!", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            {
                MessageBox.Show("Cập nhật sách không thành công!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSachXoa_Click(object sender, EventArgs e)
        {
            DialogResult check = MessageBox.Show("Bạn có chắc muốn xóa sách " + txtSachTenSach.Text + " ?",
                                         "Cảnh Báo",
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);

            if (check == DialogResult.Yes)
            {
                try
                {
                    StringBuilder query = new StringBuilder("EXEC proc_XoaSach");
                    query.Append(" @maSach = " + maSachSach);

                    dataProvider.execNonQuery(query.ToString());

                    loadDgSach();
                    MessageBox.Show("Xóa sách thành công!", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Xóa sách không thành công! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void cbSachLoaiSach_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox combobox = sender as ComboBox;
            if (combobox.SelectedValue != null && combobox.SelectedValue != DBNull.Value)
            {
                maSachLoaiSach = (int)combobox.SelectedValue;
            }
        }

        

        

        //Xu ly loai sach
        private void initLoaiSach()
        {
            loadDgLoaiSach();
            
            // Thêm tính năng tìm kiếm cho Loại Sách
            txtLoaiSachTenLoaiSach.TextChanged += txtLoaiSachTenLoaiSach_TextChanged;
        }

        private void loadDgLoaiSach()
        {
            DataTable dt = new DataTable();
            dt = dataProvider.execQuery("SELECT * FROM vw_DanhSachLoaiSach");
            dgLoaiSach.DataSource = dt;

            maLoaiSachLoaiSach = (int)dt.Rows[0][0];
        }

        private void dgLoaiSach_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowID = e.RowIndex;
            if (rowID >= 0 && rowID < dgLoaiSach.RowCount - 1)
            {
                DataGridViewRow row = dgLoaiSach.Rows[rowID];

                maLoaiSachLoaiSach = (int)row.Cells[0].Value;
                txtLoaiSachTenLoaiSach.Text = row.Cells[1].Value.ToString();
            }
        }

        private void btnLoaiSachThem_Click(object sender, EventArgs e)
        {
            StringBuilder query = new StringBuilder("EXEC proc_ThemLoaiSach");
            query.Append(" @tenLoaiSach = N'" + txtLoaiSachTenLoaiSach.Text + "'");

            try
            {
                // Vì stored procedure trả về result set, dùng execQuery thay vì execNonQuery
                var result = dataProvider.execQuery(query.ToString());
                
                loadDgLoaiSach();
                loadcbSachLoaiSach();
                
                if (result.Rows.Count > 0)
                {
                    string message = result.Rows[0]["Message"].ToString();
                    MessageBox.Show(message, "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Thêm/cập nhật loại sách thành công!", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Thêm loại sách không thành công! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoaiSachSua_Click(object sender, EventArgs e)
        {
            StringBuilder query = new StringBuilder("EXEC proc_CapNhatLoaiSach");
            query.Append(" @tenLoaiSach = N'" + txtLoaiSachTenLoaiSach.Text + "'");
            query.Append(",@maLoaiSach = " + maLoaiSachLoaiSach);

            int result = dataProvider.execNonQuery(query.ToString());

            if (result > 0)
            {
                loadDgLoaiSach();
                loadDgSach();
                loadcbSachLoaiSach();
                MessageBox.Show("Cập nhật loại sách thành công!", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            {
                MessageBox.Show("Cập nhật loại sách không thành công!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoaiSachXoa_Click(object sender, EventArgs e)
        {
            DialogResult check = MessageBox.Show("Bạn có chắc muốn xóa loại sách " + txtLoaiSachTenLoaiSach.Text + " ?",
                                         "Cảnh Báo",
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);

            if (check == DialogResult.Yes)
            {
                try
                {
                    StringBuilder query = new StringBuilder("EXEC proc_XoaLoaiSach");
                    query.Append(" @maLoaiSach = " + maLoaiSachLoaiSach);

                    dataProvider.execNonQuery(query.ToString());

                    loadDgLoaiSach();
                    loadcbSachLoaiSach();
                    MessageBox.Show("Xóa loại sách thành công!", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Xóa loại sách không thành công! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        //Xu ly hoa don
        private void initHoaDon()
        {
            loadDgHoaDon();
            
            // Thêm tính năng tìm kiếm cho Hóa Đơn
            txtHoaDonTenKH.TextChanged += txtHoaDonTenKH_TextChanged;
            txtHoaDonSDTKH.TextChanged += txtHoaDonSDTKH_TextChanged;
            dateNgayLapHoaDon.ValueChanged += dateNgayLapHoaDon_ValueChanged;
        }

        private void loadDgHoaDon()
        {
            DataTable dt = new DataTable();
            dt = dataProvider.execQuery("SELECT * FROM vw_DanhSachHoaDon");
            dgHoaDon.DataSource = dt;

            maHoaDonHoaDon = (int)dt.Rows[0][0];
        }

        private void dgHoaDon_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowID = e.RowIndex;
            if (rowID >= 0 && rowID < dgHoaDon.RowCount - 1)
            {
                DataGridViewRow row = dgHoaDon.Rows[rowID];

                maHoaDonHoaDon = (int)row.Cells[0].Value;
                dateNgayLapHoaDon.Value = DateTime.Parse(row.Cells[1].Value.ToString());
                txtHoaDonTenKH.Text = row.Cells[2].Value.ToString();
                txtHoaDonSDTKH.Text = row.Cells[3].Value.ToString();
            }
        }

        private void btnHoaDonThem_Click(object sender, EventArgs e)
        {
            StringBuilder query = new StringBuilder("EXEC proc_ThemHoaDon");
            query.Append(" @ngayLapHoaDon = '" + dateNgayLapHoaDon.Value + "'");
            query.Append(",@tenKhachHang = N'" + txtHoaDonTenKH.Text + "'");
            query.Append(",@sdtKhachHang = '" + txtHoaDonSDTKH.Text + "'");

            try
            {
                // Vì stored procedure trả về result set, dùng execQuery thay vì execNonQuery
                var result = dataProvider.execQuery(query.ToString());
                
                loadDgHoaDon();
                
                if (result.Rows.Count > 0)
                {
                    string message = result.Rows[0]["Message"].ToString();
                    MessageBox.Show(message, "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Thêm/cập nhật hóa đơn thành công!", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Thêm hóa đơn không thành công! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnHoaDonSua_Click(object sender, EventArgs e)
        {
            StringBuilder query = new StringBuilder("EXEC proc_CapNhatHoaDon");
            query.Append(" @ngayLapHoaDon = '" + dateNgayLapHoaDon.Value + "'");
            query.Append(",@tenKhachHang = N'" + txtHoaDonTenKH.Text + "'");
            query.Append(",@sdtKhachHang = '" + txtHoaDonSDTKH.Text + "'");
            query.Append(",@maHoaDon = " + maHoaDonHoaDon);

            int result = dataProvider.execNonQuery(query.ToString());

            if (result > 0)
            {
                loadDgHoaDon();
                MessageBox.Show("Cập nhật hóa đơn thành công!", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            {
                MessageBox.Show("Cập nhật hóa đơn không thành công!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnHoaDonXoa_Click(object sender, EventArgs e)
        {
            DialogResult check = MessageBox.Show("Bạn có chắc muốn xóa hóa đơn có mã là " + maHoaDonHoaDon + " ?",
                                         "Cảnh Báo",
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);

            if (check == DialogResult.Yes)
            {
                try
                {
                    StringBuilder query = new StringBuilder("EXEC proc_XoaHoaDon");
                    query.Append(" @maHoaDon = " + maHoaDonHoaDon);

                    dataProvider.execNonQuery(query.ToString());

                    loadDgHoaDon();
                    MessageBox.Show("Xóa hóa đơn thành công!", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Xóa hóa đơn không thành công! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void txtHoaDonSDTKH_KeyPress(object sender, KeyPressEventArgs e)
        {
            // chỉ cho nhập số, phím điều khiển và dấu '.'
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // chỉ cho phép một dấu '.'
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void btnHoaDonChiTiet_Click(object sender, EventArgs e)
        {
            FormChiTietHoaDon form = new FormChiTietHoaDon(maHoaDonHoaDon);
            form.ShowDialog();
        }

        // Tìm kiếm theo tên sách
        private void txtSachTenSach_TextChanged(object sender, EventArgs e)
        {
            timKiemSachTheoTen();
        }

        // Tìm kiếm theo tác giả
        private void txtSachTacGia_TextChanged(object sender, EventArgs e)
        {
            timKiemSachTheoTacGia();
        }

        // Tìm kiếm theo loại sách (ComboBox)
        private void cbSachLoaiSach_TimKiem_SelectedIndexChanged(object sender, EventArgs e)
        {
            timKiemSachTheoLoai();
        }

        // Tìm kiếm theo tên loại sách
        private void txtLoaiSachTenLoaiSach_TextChanged(object sender, EventArgs e)
        {
            timKiemLoaiSachTheoTen();
        }

        // Tìm kiếm theo tên khách hàng
        private void txtHoaDonTenKH_TextChanged(object sender, EventArgs e)
        {
            timKiemHoaDonTheoTenKH();
        }

        // Tìm kiếm theo số điện thoại khách hàng
        private void txtHoaDonSDTKH_TextChanged(object sender, EventArgs e)
        {
            timKiemHoaDonTheoSDT();
        }

        // Tìm kiếm theo ngày lập hóa đơn
        private void dateNgayLapHoaDon_ValueChanged(object sender, EventArgs e)
        {
            timKiemHoaDonTheoNgay();
        }

        // Hàm tìm kiếm theo tên sách
        private void timKiemSachTheoTen()
        {
            string tenSach = txtSachTenSach.Text.Trim();
            
            if (string.IsNullOrEmpty(tenSach))
            {
                loadDgSach();
                return;
            }

            string safeKeyword = tenSach.Replace("'", "''");
            string query = "SELECT * FROM vw_DanhSachSach WHERE [Tên Sách] LIKE N'%" + safeKeyword + "%'";

            try
            {
                dgSach.DataSource = dataProvider.execQuery(query);
            }
            catch (Exception)
            {
                loadDgSach();
            }
        }

        // Hàm tìm kiếm theo tác giả
        private void timKiemSachTheoTacGia()
        {
            string tacGia = txtSachTacGia.Text.Trim();
            
            if (string.IsNullOrEmpty(tacGia))
            {
                loadDgSach();
                return;
            }

            string safeKeyword = tacGia.Replace("'", "''");
            string query = "SELECT * FROM vw_DanhSachSach WHERE [Tác Giả] LIKE N'%" + safeKeyword + "%'";

            try
            {
                dgSach.DataSource = dataProvider.execQuery(query);
            }
            catch (Exception)
            {
                loadDgSach();
            }
        }

        // Hàm tìm kiếm theo loại sách
        private void timKiemSachTheoLoai()
        {
            if (cbSachLoaiSach.SelectedIndex < 0 || 
                cbSachLoaiSach.SelectedValue == null || 
                string.IsNullOrEmpty(cbSachLoaiSach.Text))
            {
                loadDgSach();
                return;
            }

            string tenLoaiSach = cbSachLoaiSach.Text.Replace("'", "''");
            string query = "SELECT * FROM vw_DanhSachSach WHERE [Tên Loại Sách] = N'" + tenLoaiSach + "'";

            try
            {
                dgSach.DataSource = dataProvider.execQuery(query);
            }
            catch (Exception)
            {
                loadDgSach();
            }
        }

        // Hàm tìm kiếm loại sách theo tên
        private void timKiemLoaiSachTheoTen()
        {
            string tenLoaiSach = txtLoaiSachTenLoaiSach.Text.Trim();
            
            if (string.IsNullOrEmpty(tenLoaiSach))
            {
                loadDgLoaiSach();
                return;
            }

            string query = "SELECT * FROM vw_DanhSachLoaiSach WHERE [Tên Loại Sách] LIKE N'%" + tenLoaiSach + "%'";

            try
            {
                DataTable dt = dataProvider.execQuery(query);
                dgLoaiSach.DataSource = dt;
                
                if (dt.Rows.Count > 0)
                {
                    maLoaiSachLoaiSach = (int)dt.Rows[0][0];
                }
            }
            catch (Exception)
            {
                loadDgLoaiSach();
            }
        }

        // Hàm tìm kiếm hóa đơn theo tên khách hàng
        private void timKiemHoaDonTheoTenKH()
        {
            string tenKH = txtHoaDonTenKH.Text.Trim();
            
            if (string.IsNullOrEmpty(tenKH))
            {
                loadDgHoaDon();
                return;
            }

            string query = "SELECT * FROM vw_DanhSachHoaDon WHERE [Tên Khách Hàng] LIKE N'%" + tenKH + "%'";

            try
            {
                DataTable dt = dataProvider.execQuery(query);
                dgHoaDon.DataSource = dt;
                
                if (dt.Rows.Count > 0)
                {
                    maHoaDonHoaDon = (int)dt.Rows[0][0];
                }
            }
            catch (Exception)
            {
                loadDgHoaDon();
            }
        }

        // Hàm tìm kiếm hóa đơn theo số điện thoại
        private void timKiemHoaDonTheoSDT()
        {
            string sdt = txtHoaDonSDTKH.Text.Trim();
            
            if (string.IsNullOrEmpty(sdt))
            {
                loadDgHoaDon();
                return;
            }

            string query = "SELECT * FROM vw_DanhSachHoaDon WHERE [Số Điện Thoại] LIKE '%" + sdt + "%'";

            try
            {
                DataTable dt = dataProvider.execQuery(query);
                dgHoaDon.DataSource = dt;
                
                if (dt.Rows.Count > 0)
                {
                    maHoaDonHoaDon = (int)dt.Rows[0][0];
                }
            }
            catch (Exception)
            {
                loadDgHoaDon();
            }
        }

        // Hàm tìm kiếm hóa đơn theo ngày lập
        private void timKiemHoaDonTheoNgay()
        {
            DateTime ngayLap = dateNgayLapHoaDon.Value.Date;
            
            string query = "SELECT * FROM vw_DanhSachHoaDon WHERE CAST([Ngày Lập Hoá Đơn] AS DATE) = '" + ngayLap.ToString("yyyy-MM-dd") + "'";

            try
            {
                DataTable dt = dataProvider.execQuery(query);
                dgHoaDon.DataSource = dt;
                
                if (dt.Rows.Count > 0)
                {
                    maHoaDonHoaDon = (int)dt.Rows[0][0];
                }
            }
            catch (Exception)
            {
                loadDgHoaDon();
            }
        }

     
        private void btnKhoiPhuc_Click(object sender, EventArgs e)
        {
            // Tạm thời tắt event để tránh tìm kiếm khi clear
            txtSachTenSach.TextChanged -= txtSachTenSach_TextChanged;
            txtSachTacGia.TextChanged -= txtSachTacGia_TextChanged;
            cbSachLoaiSach.SelectedIndexChanged -= cbSachLoaiSach_TimKiem_SelectedIndexChanged;

            // Clear tất cả TextBox - Tab Sách
            txtSachTenSach.Text = "";
            txtSachTacGia.Text = "";
            cbSachLoaiSach.SelectedIndex = -1;

            // Load lại dữ liệu sách
            loadDgSach();

            // Bật lại event
            txtSachTenSach.TextChanged += txtSachTenSach_TextChanged;
            txtSachTacGia.TextChanged += txtSachTacGia_TextChanged;
            cbSachLoaiSach.SelectedIndexChanged += cbSachLoaiSach_TimKiem_SelectedIndexChanged;
        }

        

        private void btnLoaiSachKhoiPhuc_Click(object sender, EventArgs e)
        {
            // Tạm thời tắt event để tránh tìm kiếm khi clear
            txtLoaiSachTenLoaiSach.TextChanged -= txtLoaiSachTenLoaiSach_TextChanged;

            // Clear TextBox - Tab Loại Sách
            txtLoaiSachTenLoaiSach.Text = "";

            // Load lại dữ liệu loại sách
            loadDgLoaiSach();

            // Bật lại event
            txtLoaiSachTenLoaiSach.TextChanged += txtLoaiSachTenLoaiSach_TextChanged;
        }

        

        private void btnHoaDonKhoiPhuc_Click(object sender, EventArgs e)
        {
            // Tạm thời tắt event để tránh tìm kiếm khi clear
            txtHoaDonTenKH.TextChanged -= txtHoaDonTenKH_TextChanged;
            txtHoaDonSDTKH.TextChanged -= txtHoaDonSDTKH_TextChanged;
            dateNgayLapHoaDon.ValueChanged -= dateNgayLapHoaDon_ValueChanged;

            // Clear TextBox và DateTimePicker - Tab Hóa Đơn
            txtHoaDonTenKH.Text = "";
            txtHoaDonSDTKH.Text = "";
            dateNgayLapHoaDon.Value = DateTime.Now;

            // Load lại dữ liệu hóa đơn
            loadDgHoaDon();

            // Bật lại event
            txtHoaDonTenKH.TextChanged += txtHoaDonTenKH_TextChanged;
            txtHoaDonSDTKH.TextChanged += txtHoaDonSDTKH_TextChanged;
            dateNgayLapHoaDon.ValueChanged += dateNgayLapHoaDon_ValueChanged;
        }
    }
}