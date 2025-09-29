use QLBanSach;
go


-- Thêm Sách (UPSERT) với validation
create proc proc_ThemSach
@tenSach nvarchar(256), @maLoaiSach int, @tacGia nvarchar(256), @soLuong int, @giaBan float
as
begin
	-- Validation dữ liệu đầu vào
	if @tenSach is null or LTRIM(RTRIM(@tenSach)) = ''
	begin
		RAISERROR(N'Tên sách không được để trống!', 16, 1);
		return;
	end

	if @maLoaiSach is null or @maLoaiSach <= 0
	begin
		RAISERROR(N'Mã loại sách không hợp lệ!', 16, 1);
		return;
	end

	if @tacGia is null or LTRIM(RTRIM(@tacGia)) = ''
	begin
		RAISERROR(N'Tác giả không được để trống!', 16, 1);
		return;
	end

	if @soLuong is null or @soLuong < 0
	begin
		RAISERROR(N'Số lượng phải lớn hơn hoặc bằng 0!', 16, 1);
		return;
	end

	if @giaBan is null or @giaBan <= 0
	begin
		RAISERROR(N'Giá bán phải lớn hơn 0!', 16, 1);
		return;
	end

	-- Kiểm tra loại sách có tồn tại không
	if not exists(select 1 from LoaiSach where MaLoaiSach = @maLoaiSach)
	begin
		RAISERROR(N'Loại sách không tồn tại!', 16, 1);
		return;
	end

	-- Trim dữ liệu trước khi xử lý
	set @tenSach = LTRIM(RTRIM(@tenSach));
	set @tacGia = LTRIM(RTRIM(@tacGia));

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

-- Cập nhật sách với validation
create proc proc_CapNhatSach
@maSach int, @tenSach nvarchar(256), @maLoaiSach int, @tacGia nvarchar(256), @soLuong int, @giaBan float
as
begin
	-- Validation dữ liệu đầu vào
	if @maSach is null or @maSach <= 0
	begin
		RAISERROR(N'Mã sách không hợp lệ!', 16, 1);
		return;
	end

	if @tenSach is null or LTRIM(RTRIM(@tenSach)) = ''
	begin
		RAISERROR(N'Tên sách không được để trống!', 16, 1);
		return;
	end

	if @maLoaiSach is null or @maLoaiSach <= 0
	begin
		RAISERROR(N'Mã loại sách không hợp lệ!', 16, 1);
		return;
	end

	if @tacGia is null or LTRIM(RTRIM(@tacGia)) = ''
	begin
		RAISERROR(N'Tác giả không được để trống!', 16, 1);
		return;
	end

	if @soLuong is null or @soLuong < 0
	begin
		RAISERROR(N'Số lượng phải lớn hơn hoặc bằng 0!', 16, 1);
		return;
	end

	if @giaBan is null or @giaBan <= 0
	begin
		RAISERROR(N'Giá bán phải lớn hơn 0!', 16, 1);
		return;
	end

	-- Kiểm tra sách có tồn tại không
	if not exists(select 1 from Sach where MaSach = @maSach)
	begin
		RAISERROR(N'Không tìm thấy sách cần cập nhật!', 16, 1);
		return;
	end

	-- Kiểm tra loại sách có tồn tại không
	if not exists(select 1 from LoaiSach where MaLoaiSach = @maLoaiSach)
	begin
		RAISERROR(N'Loại sách không tồn tại!', 16, 1);
		return;
	end

	-- Trim dữ liệu trước khi cập nhật
	set @tenSach = LTRIM(RTRIM(@tenSach));
	set @tacGia = LTRIM(RTRIM(@tacGia));

	-- Cập nhật dữ liệu
	update Sach
	set TenSach = @tenSach, MaLoaiSach = @maLoaiSach, TacGia = @tacGia, SoLuong = @soLuong, GiaBan = @giaBan
	where MaSach = @maSach;
end;
go

-- Xóa sách với kiểm tra ràng buộc
create proc proc_XoaSach
    @MaSach INT
as
begin
    -- Kiểm tra sách có tồn tại không
    if not exists(select 1 from Sach where MaSach = @MaSach)
    begin
        RAISERROR(N'Không tìm thấy sách cần xóa!', 16, 1);
        return;
    end

    -- Kiểm tra có chi tiết hóa đơn nào đang sử dụng sách này không
    if exists(select 1 from ChiTietHoaDon where MaSach = @MaSach)
    begin
        RAISERROR(N'Không thể xóa sách này vì đã có trong hóa đơn!', 16, 1);
        return;
    end

    -- Nếu không có ràng buộc thì mới xóa
    delete from Sach
    where MaSach = @MaSach;
end;
go

-- Thêm loại sách (UPSERT) với validation
create proc proc_ThemLoaiSach
@tenLoaiSach nvarchar(256)
as 
begin 
	-- Validation dữ liệu đầu vào
	if @tenLoaiSach is null or LTRIM(RTRIM(@tenLoaiSach)) = ''
	begin
		select 0 as Result, N'Tên loại sách không được để trống!' as Message;
		return;
	end

	-- Trim dữ liệu
	set @tenLoaiSach = LTRIM(RTRIM(@tenLoaiSach));

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

-- Cập nhật loại sách với validation
create proc proc_CapNhatLoaiSach
@maLoaiSach int, @tenLoaiSach nvarchar(256)
as  
begin
	-- Validation dữ liệu đầu vào
	if @maLoaiSach is null or @maLoaiSach <= 0
	begin
		RAISERROR(N'Mã loại sách không hợp lệ!', 16, 1);
		return;
	end

	if @tenLoaiSach is null or LTRIM(RTRIM(@tenLoaiSach)) = ''
	begin
		RAISERROR(N'Tên loại sách không được để trống!', 16, 1);
		return;
	end
	
	if not exists(select 1 from LoaiSach where MaLoaiSach = @maLoaiSach)
	begin
		RAISERROR(N'Không tìm thấy loại sách cần cập nhật!', 16, 1);
		return;
	end

	-- Trim dữ liệu trước khi cập nhật
	set @tenLoaiSach = LTRIM(RTRIM(@tenLoaiSach));

	-- Kiểm tra trùng tên với loại sách khác (case-insensitive)
	if exists(select 1 from LoaiSach where UPPER(TenLoaiSach) = UPPER(@tenLoaiSach) and MaLoaiSach != @maLoaiSach)
	begin
		RAISERROR(N'Tên loại sách đã tồn tại!', 16, 1);
		return;
	end

	-- Cập nhật dữ liệu
	update LoaiSach
	set TenLoaiSach = @tenLoaiSach
	where MaLoaiSach = @maLoaiSach;
end;
go

-- Xóa loại sách với kiểm tra ràng buộc
create proc proc_XoaLoaiSach
    @maLoaiSach int
as
begin
    -- Kiểm tra loại sách có tồn tại không
    if not exists(select 1 from LoaiSach where MaLoaiSach = @maLoaiSach)
    begin
        RAISERROR(N'Không tìm thấy loại sách cần xóa!', 16, 1);
        return;
    end

    -- Kiểm tra có sách nào đang sử dụng loại sách này không
    if exists(select 1 from Sach where MaLoaiSach = @maLoaiSach)
    begin
        RAISERROR(N'Không thể xóa loại sách này vì đang có sách sử dụng!', 16, 1);
        return;
    end

    -- Nếu không có ràng buộc thì mới xóa
    delete from LoaiSach
    where MaLoaiSach = @maLoaiSach;
end;
go

-- Thêm hóa đon (UPSERT) với validation
create proc proc_ThemHoaDon
@ngayLapHoaDon datetime, @tenKhachHang nvarchar(30), @sdtKhachHang nvarchar(12)
as
begin
	-- Validation dữ liệu đầu vào
	if @ngayLapHoaDon is null
	begin
		select 0 as Result, N'Ngày lập hóa đơn không được để trống!' as Message;
		return;
	end

	if @tenKhachHang is null or LTRIM(RTRIM(@tenKhachHang)) = ''
	begin
		select 0 as Result, N'Tên khách hàng không được để trống!' as Message;
		return;
	end

	-- Trim dữ liệu
	set @tenKhachHang = LTRIM(RTRIM(@tenKhachHang));
	if @sdtKhachHang is not null
		set @sdtKhachHang = LTRIM(RTRIM(@sdtKhachHang));

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

-- Cập nhật hóa đơn với validation
create proc proc_CapNhatHoaDon
@maHoaDon int, @ngayLapHoaDon datetime, @tenKhachHang nvarchar(30), @sdtKhachHang nvarchar(12)
as
begin
	-- Validation dữ liệu đầu vào
	if @maHoaDon is null or @maHoaDon <= 0
	begin
		RAISERROR(N'Mã hóa đơn không hợp lệ!', 16, 1);
		return;
	end

	if @ngayLapHoaDon is null
	begin
		RAISERROR(N'Ngày lập hóa đơn không được để trống!', 16, 1);
		return;
	end

	if @tenKhachHang is null or LTRIM(RTRIM(@tenKhachHang)) = ''
	begin
		RAISERROR(N'Tên khách hàng không được để trống!', 16, 1);
		return;
	end

	-- Kiểm tra hóa đơn có tồn tại không
	if not exists(select 1 from HoaDon where MaHoaDon = @maHoaDon)
	begin
		RAISERROR(N'Không tìm thấy hóa đơn cần cập nhật!', 16, 1);
		return;
	end

	-- Trim dữ liệu
	set @tenKhachHang = LTRIM(RTRIM(@tenKhachHang));
	if @sdtKhachHang is not null
		set @sdtKhachHang = LTRIM(RTRIM(@sdtKhachHang));

	-- Cập nhật dữ liệu
	update HoaDon
	set NgayLapHoaDon = @ngayLapHoaDon, TenKhachHang = @tenKhachHang, SDT = @sdtKhachHang
	where MaHoaDon = @maHoaDon;
end;
go

-- Xóa hóa đơn với kiểm tra ràng buộc
create proc proc_XoaHoaDon
    @MaHoaDon INT
as
begin
    -- Kiểm tra hóa đơn có tồn tại không
    if not exists(select 1 from HoaDon where MaHoaDon = @MaHoaDon)
    begin
        RAISERROR(N'Không tìm thấy hóa đơn cần xóa!', 16, 1);
        return;
    end

    -- Kiểm tra có chi tiết hóa đơn không
    if exists(select 1 from ChiTietHoaDon where MaHoaDon = @MaHoaDon)
    begin
        RAISERROR(N'Không thể xóa hóa đơn này vì đã có chi tiết hóa đơn!', 16, 1);
        return;
    end

    -- Nếu không có ràng buộc thì mới xóa
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

