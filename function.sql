use QLBanSach;
go

-- Hàm tính thành tiền cho một chi tiết hóa đơn
create function fn_TinhThanhTien(@soLuong int, @giaBan decimal(10,2))
returns decimal(12,2)
as
begin
    declare @thanhTien decimal(12,2);
    set @thanhTien = @soLuong * @giaBan;
    return @thanhTien;
end;
go

-- Hàm tính tổng tiền cho một hóa đơn
create function fn_TinhTongTienHoaDon(@maHoaDon int)
returns decimal(15,2)
as
begin
    declare @tongTien decimal(15,2);
    
    select @tongTien = sum(ct.SoLuong * s.GiaBan)
    from ChiTietHoaDon ct
    inner join Sach s on ct.MaSach = s.MaSach
    where ct.MaHoaDon = @maHoaDon;
    
    -- Nếu không có chi tiết thì trả về 0
    if @tongTien is null
        set @tongTien = 0;
    
    return @tongTien;
end;
go


-- CÁC HÀM TÌM KIẾM
-- Hàm tìm kiếm sách theo tên
CREATE FUNCTION fn_TimKiemSachTheoTen(@tenSach NVARCHAR(200))
RETURNS TABLE
AS
RETURN
(
    SELECT 
        MaSach as [Mã Sách],
        TenSach as [Tên Sách],
        TenLoaiSach as [Tên Loại Sách],
        TacGia as [Tác Giả],
        SoLuong as [Số Lượng],
        GiaBan as [Giá Bán]
    FROM Sach s
    INNER JOIN LoaiSach ls ON s.MaLoaiSach = ls.MaLoaiSach
    WHERE (@tenSach IS NULL OR @tenSach = '' OR TenSach LIKE N'%' + @tenSach + '%')
);
GO

-- Hàm tìm kiếm sách theo tác giả
CREATE FUNCTION fn_TimKiemSachTheoTacGia(@tacGia NVARCHAR(100))
RETURNS TABLE
AS
RETURN
(
    SELECT 
        MaSach as [Mã Sách],
        TenSach as [Tên Sách],
        TenLoaiSach as [Tên Loại Sách],
        TacGia as [Tác Giả],
        SoLuong as [Số Lượng],
        GiaBan as [Giá Bán]
    FROM Sach s
    INNER JOIN LoaiSach ls ON s.MaLoaiSach = ls.MaLoaiSach
    WHERE (@tacGia IS NULL OR @tacGia = '' OR TacGia LIKE N'%' + @tacGia + '%')
);
GO

-- Hàm tìm kiếm sách theo loại sách
CREATE FUNCTION fn_TimKiemSachTheoLoai(@tenLoaiSach NVARCHAR(100))
RETURNS TABLE
AS
RETURN
(
    SELECT 
        MaSach as [Mã Sách],
        TenSach as [Tên Sách],
        TenLoaiSach as [Tên Loại Sách],
        TacGia as [Tác Giả],
        SoLuong as [Số Lượng],
        GiaBan as [Giá Bán]
    FROM Sach s
    INNER JOIN LoaiSach ls ON s.MaLoaiSach = ls.MaLoaiSach
    WHERE (@tenLoaiSach IS NULL OR @tenLoaiSach = '' OR TenLoaiSach = @tenLoaiSach)
);
GO

-- Hàm tìm kiếm loại sách theo tên
CREATE FUNCTION fn_TimKiemLoaiSachTheoTen(@tenLoaiSach NVARCHAR(100))
RETURNS TABLE
AS
RETURN
(
    SELECT 
        MaLoaiSach as [Mã Loại Sách],
        TenLoaiSach as [Tên Loại Sách]
    FROM LoaiSach
    WHERE (@tenLoaiSach IS NULL OR @tenLoaiSach = '' OR TenLoaiSach LIKE N'%' + @tenLoaiSach + '%')
);
GO

-- Hàm tìm kiếm hóa đơn theo tên khách hàng
CREATE FUNCTION fn_TimKiemHoaDonTheoTenKH(@tenKhachHang NVARCHAR(100))
RETURNS TABLE
AS
RETURN
(
    SELECT 
        MaHoaDon as [Mã Hóa Đơn],
        NgayLapHoaDon as [Ngày Lập Hoá Đơn],
        TenKhachHang as [Tên Khách Hàng],
        SDT as [Số Điện Thoại]
    FROM HoaDon
    WHERE (@tenKhachHang IS NULL OR @tenKhachHang = '' OR TenKhachHang LIKE N'%' + @tenKhachHang + '%')
);
GO

-- Hàm tìm kiếm hóa đơn theo số điện thoại
CREATE FUNCTION fn_TimKiemHoaDonTheoSDT(@sdt NVARCHAR(20))
RETURNS TABLE
AS
RETURN
(
    SELECT 
        MaHoaDon as [Mã Hóa Đơn],
        NgayLapHoaDon as [Ngày Lập Hoá Đơn],
        TenKhachHang as [Tên Khách Hàng],
        SDT as [Số Điện Thoại]
    FROM HoaDon
    WHERE (@sdt IS NULL OR @sdt = '' OR SDT LIKE '%' + @sdt + '%')
);
GO

-- Hàm tìm kiếm hóa đơn theo ngày lập
CREATE FUNCTION fn_TimKiemHoaDonTheoNgay(@ngayLap DATE)
RETURNS TABLE
AS
RETURN
(
    SELECT 
        MaHoaDon as [Mã Hóa Đơn],
        NgayLapHoaDon as [Ngày Lập Hoá Đơn],
        TenKhachHang as [Tên Khách Hàng],
        SDT as [Số Điện Thoại]
    FROM HoaDon
    WHERE (@ngayLap IS NULL OR CAST(NgayLapHoaDon AS DATE) = @ngayLap)
);
GO





