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
        private int selectedMaSachSach; // Biến lưu ID sách được chọn ban đầu
        private int maLoaiSachLoaiSach;
        private int selectedMaLoaiSachLoaiSach; // Biến lưu ID loại sách được chọn ban đầu
        private int maHoaDonHoaDon;
        private int selectedMaHoaDonHoaDon; // Biến lưu ID hóa đơn được chọn ban đầu

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
                    selectedMaSachSach = maSachSach; // Khởi tạo giá trị mặc định
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

                // Tạm thời tắt event tìm kiếm để tránh trigger khi cập nhật dữ liệu
                txtSachTenSach.TextChanged -= txtSachTenSach_TextChanged;
                txtSachTacGia.TextChanged -= txtSachTacGia_TextChanged;
                cbSachLoaiSach.SelectedIndexChanged -= cbSachLoaiSach_TimKiem_SelectedIndexChanged;

                // Kiểm tra null trước khi gán giá trị
                if (row.Cells[0].Value != null && row.Cells[0].Value != DBNull.Value)
                {
                    maSachSach = (int)row.Cells[0].Value;
                    selectedMaSachSach = maSachSach; // Cập nhật ID được chọn
                }

                txtSachTenSach.Text = row.Cells[1].Value?.ToString() ?? "";
                cbSachLoaiSach.Text = row.Cells[2].Value?.ToString() ?? "";
                txtSachTacGia.Text = row.Cells[3].Value?.ToString() ?? "";

                if (row.Cells[4].Value != null && row.Cells[4].Value != DBNull.Value)
                {
                    numSachSoLuong.Value = (int)row.Cells[4].Value;
                }
                else
                {
                    numSachSoLuong.Value = 0;
                }

                if (row.Cells[5].Value != null && row.Cells[5].Value != DBNull.Value)
                {
                    numSachGiaBan.Value = Convert.ToInt32(row.Cells[5].Value);
                }
                else
                {
                    numSachGiaBan.Value = 0;
                }

                // Lấy MaLoaiSach từ SelectedValue thay vì query
                if (cbSachLoaiSach.SelectedValue != null && cbSachLoaiSach.SelectedValue != DBNull.Value)
                {
                    maSachLoaiSach = (int)cbSachLoaiSach.SelectedValue;
                }

                // Bật lại event tìm kiếm
                txtSachTenSach.TextChanged += txtSachTenSach_TextChanged;
                txtSachTacGia.TextChanged += txtSachTacGia_TextChanged;
                cbSachLoaiSach.SelectedIndexChanged += cbSachLoaiSach_TimKiem_SelectedIndexChanged;
            } 
        }

        private void btnSachThem_Click(object sender, EventArgs e)
        {
            // Validate dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(txtSachTenSach.Text))
            {
                MessageBox.Show("Vui lòng nhập tên sách!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSachTenSach.Focus();
                return;
            }

            if (maSachLoaiSach <= 0)
            {
                MessageBox.Show("Vui lòng chọn loại sách!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbSachLoaiSach.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSachTacGia.Text))
            {
                MessageBox.Show("Vui lòng nhập tác giả!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSachTacGia.Focus();
                return;
            }

            if (numSachSoLuong.Value <= 0)
            {
                MessageBox.Show("Số lượng phải lớn hơn 0!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numSachSoLuong.Focus();
                return;
            }

            if (numSachGiaBan.Value <= 0)
            {
                MessageBox.Show("Giá bán phải lớn hơn 0!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numSachGiaBan.Focus();
                return;
            }

            // Kiểm tra trùng sách trước khi thêm
            string newTenSach = txtSachTenSach.Text.Trim();
            string newTacGia = txtSachTacGia.Text.Trim();
            int newMaLoaiSach = maSachLoaiSach;
            
            string checkQuery = "SELECT COUNT(*) FROM Sach WHERE " +
                               "UPPER(LTRIM(RTRIM(TenSach))) = UPPER(N'" + newTenSach.Replace("'", "''") + "') AND " +
                               "UPPER(LTRIM(RTRIM(TacGia))) = UPPER(N'" + newTacGia.Replace("'", "''") + "') AND " +
                               "MaLoaiSach = " + newMaLoaiSach;
            
            try
            {
                var checkResult = dataProvider.execScaler(checkQuery);
                int duplicateCount = Convert.ToInt32(checkResult ?? 0);
                
                if (duplicateCount > 0)
                {
                    MessageBox.Show("Sách với tên, tác giả và loại sách này đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtSachTenSach.Focus();
                    return;
                }

                StringBuilder query = new StringBuilder("EXEC proc_ThemSach");
                query.Append(" @tenSach = N'" + newTenSach.Replace("'", "''") + "'");
                query.Append(",@maLoaiSach = " + newMaLoaiSach);
                query.Append(",@tacGia = N'" + newTacGia.Replace("'", "''") + "'");
                query.Append(",@soLuong = " + numSachSoLuong.Value);
                query.Append(",@giaBan = " + numSachGiaBan.Value);

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
            // Validate dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(txtSachTenSach.Text))
            {
                MessageBox.Show("Vui lòng nhập tên sách!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSachTenSach.Focus();
                return;
            }

            if (maSachLoaiSach <= 0)
            {
                MessageBox.Show("Vui lòng chọn loại sách!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbSachLoaiSach.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSachTacGia.Text))
            {
                MessageBox.Show("Vui lòng nhập tác giả!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSachTacGia.Focus();
                return;
            }

            if (numSachSoLuong.Value < 0)
            {
                MessageBox.Show("Số lượng phải lớn hơn hoặc bằng 0!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numSachSoLuong.Focus();
                return;
            }

            if (numSachGiaBan.Value <= 0)
            {
                MessageBox.Show("Giá bán phải lớn hơn 0!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numSachGiaBan.Focus();
                return;
            }

            // Kiểm tra trùng sách trước khi gọi stored procedure
            string newTenSach = txtSachTenSach.Text.Trim();
            string newTacGia = txtSachTacGia.Text.Trim();
            int newMaLoaiSach = maSachLoaiSach;
            
            string checkQuery = "SELECT COUNT(*) FROM Sach WHERE " +
                               "UPPER(LTRIM(RTRIM(TenSach))) = UPPER(N'" + newTenSach.Replace("'", "''") + "') AND " +
                               "UPPER(LTRIM(RTRIM(TacGia))) = UPPER(N'" + newTacGia.Replace("'", "''") + "') AND " +
                               "MaLoaiSach = " + newMaLoaiSach + " AND " +
                               "MaSach != " + selectedMaSachSach;
            
            try
            {
                var checkResult = dataProvider.execScaler(checkQuery);
                int duplicateCount = Convert.ToInt32(checkResult ?? 0);
                
                if (duplicateCount > 0)
                {
                    MessageBox.Show("Sách với tên, tác giả và loại sách này đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtSachTenSach.Focus();
                    return;
                }

                // Nếu không trùng thì thực hiện cập nhật (sử dụng selectedMaSachSach)
                StringBuilder query = new StringBuilder("EXEC proc_CapNhatSach");
                query.Append(" @maSach = " + selectedMaSachSach);
                query.Append(",@tenSach = N'" + newTenSach.Replace("'", "''") + "'");
                query.Append(",@maLoaiSach = " + newMaLoaiSach);
                query.Append(",@tacGia = N'" + newTacGia.Replace("'", "''") + "'");
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
            catch (Exception ex)
            {
                MessageBox.Show("Cập nhật sách không thành công! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    query.Append(" @maSach = " + selectedMaSachSach);

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

            if (dt.Rows.Count > 0)
            {
                maLoaiSachLoaiSach = (int)dt.Rows[0][0];
                selectedMaLoaiSachLoaiSach = maLoaiSachLoaiSach; // Khởi tạo giá trị mặc định
            }
        }

        private void dgLoaiSach_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowID = e.RowIndex;
            if (rowID >= 0 && rowID < dgLoaiSach.RowCount - 1)
            {
                DataGridViewRow row = dgLoaiSach.Rows[rowID];

                // Tạm thời tắt event tìm kiếm để tránh trigger khi cập nhật dữ liệu
                txtLoaiSachTenLoaiSach.TextChanged -= txtLoaiSachTenLoaiSach_TextChanged;

                // Kiểm tra null trước khi gán giá trị
                if (row.Cells[0].Value != null && row.Cells[0].Value != DBNull.Value)
                {
                    maLoaiSachLoaiSach = (int)row.Cells[0].Value;
                    selectedMaLoaiSachLoaiSach = maLoaiSachLoaiSach; // Cập nhật ID được chọn
                }

                txtLoaiSachTenLoaiSach.Text = row.Cells[1].Value?.ToString() ?? "";

                // Bật lại event tìm kiếm
                txtLoaiSachTenLoaiSach.TextChanged += txtLoaiSachTenLoaiSach_TextChanged;
            }
        }

        private void btnLoaiSachThem_Click(object sender, EventArgs e)
        {
            // Validate dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(txtLoaiSachTenLoaiSach.Text))
            {
                MessageBox.Show("Vui lòng nhập tên loại sách!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLoaiSachTenLoaiSach.Focus();
                return;
            }

            StringBuilder query = new StringBuilder("EXEC proc_ThemLoaiSach");
            query.Append(" @tenLoaiSach = N'" + txtLoaiSachTenLoaiSach.Text.Trim() + "'");

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
            // Validate dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(txtLoaiSachTenLoaiSach.Text))
            {
                MessageBox.Show("Vui lòng nhập tên loại sách!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLoaiSachTenLoaiSach.Focus();
                return;
            }

            // Kiểm tra trùng tên trước khi gọi stored procedure
            string newName = txtLoaiSachTenLoaiSach.Text.Trim();
            string checkQuery = "SELECT COUNT(*) FROM LoaiSach WHERE UPPER(LTRIM(RTRIM(TenLoaiSach))) = UPPER(N'" + newName + "') AND MaLoaiSach != " + selectedMaLoaiSachLoaiSach;
            
            try
            {
                var checkResult = dataProvider.execScaler(checkQuery);
                int duplicateCount = Convert.ToInt32(checkResult ?? 0);
                
                if (duplicateCount > 0)
                {
                    MessageBox.Show("Tên loại sách đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtLoaiSachTenLoaiSach.Focus();
                    return;
                }

                // Nếu không trùng thì thực hiện cập nhật (sử dụng selectedMaLoaiSachLoaiSach thay vì maLoaiSachLoaiSach)
                StringBuilder query = new StringBuilder("EXEC proc_CapNhatLoaiSach");
                query.Append(" @tenLoaiSach = N'" + newName + "'");
                query.Append(",@maLoaiSach = " + selectedMaLoaiSachLoaiSach);

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
            catch (Exception ex)
            {
                MessageBox.Show("Cập nhật loại sách không thành công! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    query.Append(" @maLoaiSach = " + selectedMaLoaiSachLoaiSach);

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

            // Chỉ khởi tạo selectedMaHoaDonHoaDon nếu chưa có giá trị hoặc bằng 0
            if (dt.Rows.Count > 0)
            {
                maHoaDonHoaDon = (int)dt.Rows[0][0];
                if (selectedMaHoaDonHoaDon <= 0)
                {
                    selectedMaHoaDonHoaDon = maHoaDonHoaDon; // Chỉ khởi tạo lần đầu
                }
            }
        }

        private void dgHoaDon_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowID = e.RowIndex;
            if (rowID >= 0 && rowID < dgHoaDon.RowCount - 1)
            {
                DataGridViewRow row = dgHoaDon.Rows[rowID];

                // Tạm thời tắt event tìm kiếm để tránh trigger khi cập nhật dữ liệu
                txtHoaDonTenKH.TextChanged -= txtHoaDonTenKH_TextChanged;
                txtHoaDonSDTKH.TextChanged -= txtHoaDonSDTKH_TextChanged;
                dateNgayLapHoaDon.ValueChanged -= dateNgayLapHoaDon_ValueChanged;

                // Kiểm tra null trước khi gán giá trị
                if (row.Cells[0].Value != null && row.Cells[0].Value != DBNull.Value)
                {
                    maHoaDonHoaDon = (int)row.Cells[0].Value;
                    selectedMaHoaDonHoaDon = maHoaDonHoaDon; // Cập nhật ID được chọn
                }

                if (row.Cells[1].Value != null && row.Cells[1].Value != DBNull.Value)
                {
                    try
                    {
                        dateNgayLapHoaDon.Value = DateTime.Parse(row.Cells[1].Value.ToString());
                    }
                    catch
                    {
                        dateNgayLapHoaDon.Value = DateTime.Now;
                    }
                }
                else
                {
                    dateNgayLapHoaDon.Value = DateTime.Now;
                }

                txtHoaDonTenKH.Text = row.Cells[2].Value?.ToString() ?? "";
                txtHoaDonSDTKH.Text = row.Cells[3].Value?.ToString() ?? "";

                // Bật lại event tìm kiếm
                txtHoaDonTenKH.TextChanged += txtHoaDonTenKH_TextChanged;
                txtHoaDonSDTKH.TextChanged += txtHoaDonSDTKH_TextChanged;
                dateNgayLapHoaDon.ValueChanged += dateNgayLapHoaDon_ValueChanged;
            }
        }

        private void btnHoaDonThem_Click(object sender, EventArgs e)
        {
            // Validate dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(txtHoaDonTenKH.Text))
            {
                MessageBox.Show("Vui lòng nhập tên khách hàng!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHoaDonTenKH.Focus();
                return;
            }

            // Kiểm tra trùng hóa đơn trước khi thêm
            DateTime newNgayLap = dateNgayLapHoaDon.Value.Date;
            string newSDT = txtHoaDonSDTKH.Text.Trim();
            
            // Chỉ kiểm tra trùng nếu có số điện thoại
            if (!string.IsNullOrEmpty(newSDT))
            {
                string checkQuery = "SELECT COUNT(*) FROM HoaDon WHERE " +
                                   "CAST(NgayLapHoaDon AS DATE) = '" + newNgayLap.ToString("yyyy-MM-dd") + "' AND " +
                                   "LTRIM(RTRIM(SDT)) = '" + newSDT.Replace("'", "''") + "'";
                
                try
                {
                    var checkResult = dataProvider.execScaler(checkQuery);
                    int duplicateCount = Convert.ToInt32(checkResult ?? 0);
                    
                    if (duplicateCount > 0)
                    {
                        MessageBox.Show("Đã tồn tại hóa đơn trong ngày " + newNgayLap.ToString("dd/MM/yyyy") + 
                                       " cho số điện thoại " + newSDT + "!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtHoaDonSDTKH.Focus();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi kiểm tra trùng hóa đơn! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            StringBuilder query = new StringBuilder("EXEC proc_ThemHoaDon");
            query.Append(" @ngayLapHoaDon = '" + dateNgayLapHoaDon.Value + "'");
            query.Append(",@tenKhachHang = N'" + txtHoaDonTenKH.Text.Trim().Replace("'", "''") + "'");
            query.Append(",@sdtKhachHang = '" + newSDT.Replace("'", "''") + "'");

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
            // Validate dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(txtHoaDonTenKH.Text))
            {
                MessageBox.Show("Vui lòng nhập tên khách hàng!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHoaDonTenKH.Focus();
                return;
            }

            // Kiểm tra trùng hóa đơn trước khi gọi stored procedure
            DateTime newNgayLap = dateNgayLapHoaDon.Value.Date;
            string newSDT = txtHoaDonSDTKH.Text.Trim();
            
            // Chỉ kiểm tra trùng nếu có số điện thoại
            if (!string.IsNullOrEmpty(newSDT))
            {
                string checkQuery = "SELECT COUNT(*) FROM HoaDon WHERE " +
                                   "CAST(NgayLapHoaDon AS DATE) = '" + newNgayLap.ToString("yyyy-MM-dd") + "' AND " +
                                   "LTRIM(RTRIM(SDT)) = '" + newSDT.Replace("'", "''") + "' AND " +
                                   "MaHoaDon != " + selectedMaHoaDonHoaDon;
                
                try
                {
                    var checkResult = dataProvider.execScaler(checkQuery);
                    int duplicateCount = Convert.ToInt32(checkResult ?? 0);
                    
                    if (duplicateCount > 0)
                    {
                        MessageBox.Show("Đã tồn tại hóa đơn trong ngày " + newNgayLap.ToString("dd/MM/yyyy") + 
                                       " cho số điện thoại " + newSDT + "!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtHoaDonSDTKH.Focus();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi kiểm tra trùng hóa đơn! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Nếu không trùng thì thực hiện cập nhật
            StringBuilder query = new StringBuilder("EXEC proc_CapNhatHoaDon");
            query.Append(" @ngayLapHoaDon = '" + dateNgayLapHoaDon.Value + "'");
            query.Append(",@tenKhachHang = N'" + txtHoaDonTenKH.Text.Trim().Replace("'", "''") + "'");
            query.Append(",@sdtKhachHang = '" + newSDT.Replace("'", "''") + "'");
            query.Append(",@maHoaDon = " + selectedMaHoaDonHoaDon);

            try
            {
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
            catch (Exception ex)
            {
                MessageBox.Show("Cập nhật hóa đơn không thành công! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnHoaDonXoa_Click(object sender, EventArgs e)
        {
            DialogResult check = MessageBox.Show("Bạn có chắc muốn xóa hóa đơn có mã là " + selectedMaHoaDonHoaDon + " ?",
                                         "Cảnh Báo",
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);

            if (check == DialogResult.Yes)
            {
                try
                {
                    StringBuilder query = new StringBuilder("EXEC proc_XoaHoaDon");
                    query.Append(" @maHoaDon = " + selectedMaHoaDonHoaDon);

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
            FormChiTietHoaDon form = new FormChiTietHoaDon(selectedMaHoaDonHoaDon);
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
            
            try
            {
                string query = "SELECT * FROM fn_TimKiemSachTheoTen(N'" + tenSach.Replace("'", "''") + "')";
                dgSach.DataSource = dataProvider.execQuery(query);
                // KHÔNG cập nhật selectedMaSachSach khi tìm kiếm
                // Chỉ cập nhật khi user thực sự click vào một dòng
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
            
            try
            {
                string query = "SELECT * FROM fn_TimKiemSachTheoTacGia(N'" + tacGia.Replace("'", "''") + "')";
                dgSach.DataSource = dataProvider.execQuery(query);
                // KHÔNG cập nhật selectedMaSachSach khi tìm kiếm
            }
            catch (Exception)
            {
                loadDgSach();
            }
        }

        // Hàm tìm kiếm theo loại sách
        private void timKiemSachTheoLoai()
        {
            string tenLoaiSach = "";
            
            if (cbSachLoaiSach.SelectedIndex >= 0 && 
                cbSachLoaiSach.SelectedValue != null && 
                !string.IsNullOrEmpty(cbSachLoaiSach.Text))
            {
                tenLoaiSach = cbSachLoaiSach.Text.Replace("'", "''");
            }

            try
            {
                string query = "SELECT * FROM fn_TimKiemSachTheoLoai(N'" + tenLoaiSach + "')";
                dgSach.DataSource = dataProvider.execQuery(query);
                // KHÔNG cập nhật selectedMaSachSach khi tìm kiếm
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
            
            try
            {
                string query = "SELECT * FROM fn_TimKiemLoaiSachTheoTen(N'" + tenLoaiSach.Replace("'", "''") + "')";
                DataTable dt = dataProvider.execQuery(query);
                dgLoaiSach.DataSource = dt;
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
            
            try
            {
                string query = "SELECT * FROM fn_TimKiemHoaDonTheoTenKH(N'" + tenKH.Replace("'", "''") + "')";
                DataTable dt = dataProvider.execQuery(query);
                dgHoaDon.DataSource = dt;
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
            
            try
            {
                string query = "SELECT * FROM fn_TimKiemHoaDonTheoSDT('" + sdt.Replace("'", "''") + "')";
                DataTable dt = dataProvider.execQuery(query);
                dgHoaDon.DataSource = dt;
                
                // KHÔNG cập nhật selectedMaHoaDonHoaDon khi tìm kiếm
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
            
            try
            {
                string query = "SELECT * FROM fn_TimKiemHoaDonTheoNgay('" + ngayLap.ToString("yyyy-MM-dd") + "')";
                DataTable dt = dataProvider.execQuery(query);
                dgHoaDon.DataSource = dt;
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