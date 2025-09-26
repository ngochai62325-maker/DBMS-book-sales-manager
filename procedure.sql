use QLBanSach;
go


-- Thêm Sách
create proc proc_ThemSach
@tenSach nvarchar(256), @maLoaiSach int, @tacGia nvarchar(256), @soLuong int, @giaBan float
as
begin
	insert into Sach(TenSach, MaLoaiSach, TacGia, SoLuong, GiaBan)
	values(@tenSach, @maLoaiSach, @tacGia, @soLuong, @giaBan);
end;
go

-- Cập nhật sách
create proc proc_CapNhatSach
@maSach int, @tenSach nvarchar(256), @maLoaiSach int, @tacGia nvarchar(256), @soLuong int, @giaBan float
as
begin
	update Sach
	set TenSach = @tenSach, MaLoaiSach = @maLoaiSach, TacGia = @tacGia, SoLuong = @soLuong, GiaBan = @giaBan
	where MaSach = @maSach;
end;
go

-- Xóa sách
create proc proc_XoaSach
    @MaSach INT
as
begin
    delete from Sach
    where MaSach = @MaSach;
end;
go

-- Thêm loại sách
create proc proc_ThemLoaiSach
@tenLoaiSach nvarchar(256)
as 
begin 
	insert into LoaiSach(TenLoaiSach) 
	values (@tenLoaiSach);
end;
go

-- Cập nhật loại sách
create proc proc_CapNhatLoaiSach
@maLoaiSach int, @tenLoaiSach nvarchar(256)
as  
begin
	update LoaiSach
	set TenLoaiSach = @tenLoaiSach
	where MaLoaiSach = @maLoaiSach;
end;
go

-- Xóa loại sách
create proc proc_XoaLoaiSach
    @maLoaiSach int
as
begin
    delete from LoaiSach
    where MaLoaiSach = @maLoaiSach;
end;
go

-- Thêm hóa đon
create proc proc_ThemHoaDon
@ngayLapHoaDon datetime, @tenKhachHang nvarchar(30), @sdtKhachHang nvarchar(12)
as
begin
	insert into HoaDon(NgayLapHoaDon, TenKhachHang, SDT)
	values (@ngayLapHoaDon, @tenKhachHang, @sdtKhachHang);
end;
go

-- Cập nhật hóa đơn
create proc proc_CapNhatHoaDon
@maHoaDon int, @ngayLapHoaDon datetime, @tenKhachHang nvarchar(30), @sdtKhachHang nvarchar(12)
as
begin
	update HoaDon
	set NgayLapHoaDon = @ngayLapHoaDon, TenKhachHang = @tenKhachHang, SDT = @sdtKhachHang
	where MaHoaDon = @maHoaDon;
end;
go

-- Xóa hóa đon
create proc proc_XoaHoaDon
    @MaHoaDon INT
as
begin
    delete from HoaDon
    where MaHoaDon = @MaHoaDon;
end;
go