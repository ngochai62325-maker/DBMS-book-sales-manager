use QLBanSach;
go


-- Thêm Sách (UPSERT)
create proc proc_ThemSach
@tenSach nvarchar(256), @maLoaiSach int, @tacGia nvarchar(256), @soLuong int, @giaBan float
as
begin
	-- Kiểm tra xem sách đã tồn tại chưa (dựa trên tên sách và tác giả)
	if exists(select 1 from Sach where TenSach = @tenSach and TacGia = @tacGia)
	begin
		-- Nếu đã tồn tại thì cộng thêm số lượng và cập nhật giá
		update Sach
		set MaLoaiSach = @maLoaiSach, 
			SoLuong = SoLuong + @soLuong,  -- Cộng thêm số lượng
			GiaBan = @giaBan
		where TenSach = @tenSach and TacGia = @tacGia;
	end
	else
	begin
		-- Nếu chưa tồn tại thì thêm mới
		insert into Sach(TenSach, MaLoaiSach, TacGia, SoLuong, GiaBan)
		values(@tenSach, @maLoaiSach, @tacGia, @soLuong, @giaBan);
	end
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

-- Thêm loại sách (UPSERT)
create proc proc_ThemLoaiSach
@tenLoaiSach nvarchar(256)
as 
begin 
	-- Kiểm tra xem loại sách đã tồn tại chưa
	if exists(select 1 from LoaiSach where TenLoaiSach = @tenLoaiSach)
	begin
		-- Nếu đã tồn tại thì không làm gì (hoặc có thể thông báo)
		select 0 as Result, N'Loại sách đã tồn tại' as Message;
	end
	else
	begin
		-- Nếu chưa tồn tại thì thêm mới
		insert into LoaiSach(TenLoaiSach) 
		values (@tenLoaiSach);
		select 1 as Result, N'Thêm loại sách thành công' as Message;
	end
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

-- Thêm hóa đon (UPSERT)
create proc proc_ThemHoaDon
@ngayLapHoaDon datetime, @tenKhachHang nvarchar(30), @sdtKhachHang nvarchar(12)
as
begin
	-- Kiểm tra xem hóa đơn đã tồn tại chưa (dựa trên ngày, tên và SDT khách hàng)
	if exists(select 1 from HoaDon 
			  where NgayLapHoaDon = @ngayLapHoaDon 
			  and TenKhachHang = @tenKhachHang 
			  and SDT = @sdtKhachHang)
	begin
		-- Nếu đã tồn tại thì cập nhật (trong trường hợp này có thể không cần cập nhật gì)
		select 0 as Result, N'Hóa đơn đã tồn tại' as Message;
	end
	else
	begin
		-- Nếu chưa tồn tại thì thêm mới
		insert into HoaDon(NgayLapHoaDon, TenKhachHang, SDT)
		values (@ngayLapHoaDon, @tenKhachHang, @sdtKhachHang);
		select 1 as Result, N'Thêm hóa đơn thành công' as Message;
	end
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

-- Thêm chi tiết hóa đơn (UPSERT)
create proc proc_ThemChiTietHoaDon
@maHoaDon int, @maSach int, @soLuong int
as
begin
    -- Kiểm tra xem chi tiết hóa đơn đã tồn tại chưa
    if exists(select 1 from ChiTietHoaDon where MaHoaDon = @maHoaDon and MaSach = @maSach)
    begin
        -- Nếu đã tồn tại thì cộng thêm số lượng
        update ChiTietHoaDon
        set SoLuong = SoLuong + @soLuong
        where MaHoaDon = @maHoaDon and MaSach = @maSach;
        
        select 1 as Result, N'Đã cập nhật số lượng sách trong hóa đơn' as Message;
    end
    else
    begin
        -- Nếu chưa tồn tại thì thêm mới
        insert into ChiTietHoaDon(MaHoaDon, MaSach, SoLuong)
        values(@maHoaDon, @maSach, @soLuong);
        
        select 1 as Result, N'Thêm chi tiết hóa đơn thành công' as Message;
    end
end;
go

-- Cập nhật chi tiết hóa đơn
create proc proc_CapNhatChiTietHoaDon
@maHoaDon int, @maSach int, @soLuongMoi int
as
begin
    update ChiTietHoaDon
    set SoLuong = @soLuongMoi
    where MaHoaDon = @maHoaDon and MaSach = @maSach;
end;
go

-- Xóa chi tiết hóa đơn
create proc proc_XoaChiTietHoaDon
@maHoaDon int, @maSach int
as
begin
    delete from ChiTietHoaDon
    where MaHoaDon = @maHoaDon and MaSach = @maSach;
end;
go

