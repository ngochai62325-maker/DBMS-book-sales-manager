use QLBanSach;
go

-- View hiển thị thông tin sách kết hợp với loại sách
create view vw_DanhSachSach
as
select 
    MaSach as [Mã Sách],
    TenSach as [Tên Sách],
    TenLoaiSach as [Tên Loại Sách],
    TacGia as [Tác Giả],
    SoLuong as [Số Lượng],
    GiaBan as [Giá Bán]
from Sach s
inner join LoaiSach ls on s.MaLoaiSach = ls.MaLoaiSach;
go

-- View hiển thị thông tin loại sách
create view vw_DanhSachLoaiSach
as
select 
    MaLoaiSach as [Mã Loại Sách],
    TenLoaiSach as [Tên Loại Sách]
from LoaiSach;
go

-- View cho ComboBox loại sách (không có alias)
create view vw_LoaiSach
as
select 
    MaLoaiSach,
    TenLoaiSach
from LoaiSach;
go


-- View hiển thị thông tin hóa đơn
create view vw_DanhSachHoaDon
as
select 
    MaHoaDon as [Mã Hóa Đơn],
    NgayLapHoaDon as [Ngày Lập Hoá Đơn],
    TenKhachHang as [Tên Khách Hàng],
    SDT as [Số Điện Thoại]
from HoaDon;
go

-- View hiển thị chi tiết hóa đơn 
create view vw_ChiTietHoaDon
as
select 
    ct.MaHoaDon as [Mã Hóa Đơn],
    ct.MaSach as [Mã Sách],
    s.TenSach as [Tên Sách],
    s.TacGia as [Tác Giả],
    ct.SoLuong as [Số Lượng],  
    s.GiaBan as [Giá Bán],
    dbo.fn_TinhThanhTien(ct.SoLuong, s.GiaBan) as [Thành Tiền]
from ChiTietHoaDon ct
inner join Sach s on ct.MaSach = s.MaSach;
go




